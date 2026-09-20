// The shapes the API sends and receives. They mirror the records in FunBooksAndVideos.Api/Contracts:
// JSON is camelCase, enums travel as names, and IDs and prices are plain numbers.

export type Category = "Book" | "Video";
export type ProductKind = "Physical" | "Digital" | "Membership";

// For a membership, `categories` is what it grants access to. Otherwise it is the one category the product belongs to.
export interface Product {
  id: number;
  name: string;
  price: number;
  kind: ProductKind;
  categories: Category[];
}

export interface NewProduct {
  kind: ProductKind;
  name: string;
  price: number;
  categories: Category[];
}

export interface Membership {
  productId: number;
  categories: Category[];
  activatedAt: string;
}

export interface Customer {
  id: number;
  name: string;
  memberships: Membership[];
  accessibleCategories: Category[];
}

// `price` is what the product cost when the order was placed.
export interface OrderItem {
  productId: number;
  productName: string;
  kind: ProductKind;
  categories: Category[];
  price: number;
}

export interface Order {
  id: number;
  customerId: number;
  total: number;
  items: OrderItem[];
}

// What placing an order did: BR1 activated these memberships, and BR2 generated a shipping slip or not.
export interface PlacedOrder extends Order {
  activatedMemberships: Membership[];
  shippingSlipGenerated: boolean;
}

export interface ShippingSlip {
  purchaseOrderId: number;
  customerId: number;
  generatedAt: string;
  items: { productId: number; productName: string }[];
}
