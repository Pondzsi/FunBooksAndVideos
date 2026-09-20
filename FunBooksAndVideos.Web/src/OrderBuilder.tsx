import { useState } from "react";
import { api, asApiError, type ApiError } from "./api";
import type { Customer, PlacedOrder, Product } from "./types";
import { ErrorNotice, LineItems, money, type Line } from "./ui";

// The PDF's example order: customer 4567890 buys the video, the book and the Book Club membership.
const PDF_EXAMPLE = { customerId: 4567890, productIds: [1, 2, 3] };
// An ID that is not in the database, to see the API's 422.
const UNKNOWN_ID = 999999;

interface Props {
  customers: Customer[];
  products: Product[];
  customerId: number | null;
  onCustomerChange: (id: number) => void;
  onPlaced: (order: PlacedOrder) => void;
}

export function OrderBuilder({ customers, products, customerId, onCustomerChange, onPlaced }: Props) {
  const [productIds, setProductIds] = useState<number[]>([]); // the same product can appear twice
  const [picked, setPicked] = useState<number>();
  const [placing, setPlacing] = useState(false);
  const [error, setError] = useState<ApiError>();

  const productToAdd = picked ?? products[0]?.id;
  const customerIsKnown = customers.some((customer) => customer.id === customerId);

  // The running list. Prices are the catalog's today; the API prices the order itself when it is placed.
  const lines: Line[] = productIds.map((id, index) => {
    const product = products.find((p) => p.id === id);

    return {
      key: `${index}-${id}`,
      name: product?.name ?? `Product ${id} (not in the catalog)`,
      kind: product?.kind,
      categories: product?.categories,
      price: product?.price,
      action: (
        <button
          type="button"
          className="link"
          aria-label={`Remove line ${index + 1}`}
          onClick={() => setProductIds((ids) => ids.filter((_, i) => i !== index))}
        >
          Remove
        </button>
      ),
    };
  });
  const total = lines.reduce((sum, line) => sum + (line.price ?? 0), 0);

  function loadPdfExample() {
    onCustomerChange(PDF_EXAMPLE.customerId);
    setProductIds(PDF_EXAMPLE.productIds);
    setError(undefined);
  }

  async function place() {
    setPlacing(true);
    setError(undefined);

    try {
      const order = await api.placeOrder(customerId ?? 0, productIds);
      onPlaced(order);
    } catch (e) {
      setError(asApiError(e));
    } finally {
      setPlacing(false);
    }
  }

  return (
    <section>
      <h2>New order</h2>

      <div className="field">
        <label htmlFor="order-customer">Customer</label>
        <select
          id="order-customer"
          value={customerId ?? ""}
          onChange={(e) => onCustomerChange(Number(e.target.value))}
        >
          {customers.map((customer) => (
            <option key={customer.id} value={customer.id}>
              {customer.name} ({customer.id})
            </option>
          ))}
          {customerId !== null && !customerIsKnown && (
            <option value={customerId}>Customer {customerId} (does not exist)</option>
          )}
        </select>
      </div>

      <div className="form-row">
        <div className="field grow">
          <label htmlFor="order-product">Product</label>
          <select id="order-product" value={productToAdd ?? ""} onChange={(e) => setPicked(Number(e.target.value))}>
            {products.map((product) => (
              <option key={product.id} value={product.id}>
                {product.name} ({money(product.price)})
              </option>
            ))}
          </select>
        </div>
        <button
          type="button"
          disabled={productToAdd === undefined}
          onClick={() => setProductIds((ids) => [...ids, productToAdd!])}
        >
          Add to order
        </button>
      </div>

      {lines.length === 0 ? (
        <p className="empty">
          No products yet. Add some above or load the PDF example. Placing an empty order shows the API's validation
          error.
        </p>
      ) : (
        <LineItems lines={lines} total={total} />
      )}

      <div className="form-row actions">
        <button type="button" className="primary" disabled={placing} onClick={place}>
          Place order
        </button>
        <button type="button" onClick={loadPdfExample}>
          Load the PDF example
        </button>
        <button
          type="button"
          className="link"
          onClick={() => {
            setProductIds([]);
            setError(undefined);
          }}
        >
          Clear
        </button>
      </div>

      {/* Nothing is validated in the browser, so an empty order goes to the API and comes back as a 400. */}
      <div className="form-row try-error">
        <span className="hint">See an error response:</span>
        <button type="button" className="link" onClick={() => setProductIds((ids) => [...ids, UNKNOWN_ID])}>
          add an unknown product
        </button>
        <button type="button" className="link" onClick={() => onCustomerChange(UNKNOWN_ID)}>
          use an unknown customer
        </button>
      </div>

      {error && <ErrorNotice error={error} />}
    </section>
  );
}
