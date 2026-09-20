import type { Customer, NewProduct, Order, PlacedOrder, Product, ShippingSlip } from "./types";

// Every failure the UI can meet comes out of this file as one ApiError, so the screens only ever render one kind of error.
// The API answers all errors as RFC 9457 Problem Details (application/problem+json).
export class ApiError extends Error {
  constructor(
    readonly status: number, // 0 when the request never got an answer
    readonly title: string,
    readonly detail?: string,
    // 400: the validation messages per field, from the Problem Details `errors` object.
    readonly fieldErrors: Record<string, string[]> = {},
    // 422: the customer or product the request named that does not exist.
    readonly missing?: { field: "customerId" | "productId"; id: number },
  ) {
    super(detail ?? title);
  }
}

export function asApiError(error: unknown): ApiError {
  return error instanceof ApiError ? error : new ApiError(0, "Something went wrong", String(error));
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
  customerId?: number;
  productId?: number;
}

async function toApiError(response: Response): Promise<ApiError> {
  if (!response.headers.get("content-type")?.includes("application/problem+json")) {
    // Not an answer from the API. The dev proxy replies 500 with an empty body when the API is not running.
    return new ApiError(
      response.status,
      response.status >= 500 ? "Cannot reach the API" : "Unexpected response",
      `The server answered ${response.status} without Problem Details. Start the API on port 5163 (or set API_URL) and reload.`,
    );
  }

  const problem: ProblemDetails = await response.json().catch(() => ({}));
  let missing: ApiError["missing"];

  if (response.status === 422) {
    if (problem.customerId !== undefined) missing = { field: "customerId", id: problem.customerId };
    if (problem.productId !== undefined) missing = { field: "productId", id: problem.productId };
  }

  return new ApiError(response.status, problem.title ?? "The request failed", problem.detail, problem.errors, missing);
}

async function request<T>(path: string, body?: unknown): Promise<T> {
  let response: Response;

  try {
    response = await fetch(`/api/v1${path}`, {
      method: body === undefined ? "GET" : "POST",
      headers: body === undefined ? undefined : { "Content-Type": "application/json" },
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch {
    throw new ApiError(0, "Cannot reach the API", "The request did not get an answer. Is the dev server running?");
  }

  if (!response.ok) {
    throw await toApiError(response);
  }

  return response.json();
}

// One function per endpoint the UI uses.
export const api = {
  getProducts: () => request<Product[]>("/products"),
  createProduct: (product: NewProduct) => request<Product>("/products", product),

  getCustomers: () => request<Customer[]>("/customers"),
  createCustomer: (name: string) => request<Customer>("/customers", { name }),

  placeOrder: (customerId: number, productIds: number[]) =>
    request<PlacedOrder>("/purchase-orders", { customerId, productIds }),
  getOrders: (customerId?: number) =>
    request<Order[]>(customerId === undefined ? "/purchase-orders" : `/purchase-orders?customerId=${customerId}`),
  getShippingSlip: (orderId: number) => request<ShippingSlip>(`/purchase-orders/${orderId}/shipping-slip`),
};
