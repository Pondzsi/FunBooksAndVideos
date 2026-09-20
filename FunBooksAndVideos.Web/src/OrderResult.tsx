import { api } from "./api";
import type { Customer, PlacedOrder } from "./types";
import { useLoad } from "./useLoad";
import { Categories, ErrorNotice, LineItems, when } from "./ui";

interface Props {
  order: PlacedOrder | null;
  customers: Customer[];
}

// The order that was just placed, set out like a slip: the items with the price each had, the total,
// and below the perforation what the two business rules did.
export function OrderResult({ order, customers }: Props) {
  return (
    <section aria-live="polite">
      <h2>Last order</h2>

      {order ? (
        // The key makes a new order remount, so its stamps play again and its shipping slip is fetched again.
        <div key={order.id}>
          <PlacedOrderSlip order={order} customers={customers} />
          {order.shippingSlipGenerated && <ShippingSlipView orderId={order.id} />}
        </div>
      ) : (
        <p className="empty">
          Place an order to see its total, the memberships it activated and whether a shipping slip was generated.
        </p>
      )}
    </section>
  );
}

function PlacedOrderSlip({ order, customers }: { order: PlacedOrder; customers: Customer[] }) {
  const customer = customers.find((c) => c.id === order.customerId);
  const hasMembership = order.items.some((item) => item.kind === "Membership");

  const lines = order.items.map((item, index) => ({
    key: `${index}-${item.productId}`,
    name: item.productName,
    kind: item.kind,
    categories: item.categories,
    price: item.price,
  }));

  // A membership the customer already holds is in the order but is not activated a second time.
  const membershipName = (productId: number) =>
    order.items.find((item) => item.productId === productId)?.productName ?? `Product ${productId}`;

  return (
    <article className="slip">
      <h3 className="slip-title">Purchase order {order.id}</h3>
      <p className="hint">
        {customer ? `${customer.name}, ` : ""}customer ID {order.customerId}
      </p>

      <LineItems lines={lines} total={order.total} />

      <div className="perforation" />

      <div className="outcome">
        <div>
          <h4>Membership (BR1)</h4>
          {order.activatedMemberships.map((membership) => (
            <p key={membership.productId}>
              {membershipName(membership.productId)} <Categories values={membership.categories} />
              <br />
              <span className="hint">Active on the account since {when(membership.activatedAt)}.</span>
            </p>
          ))}
          {order.activatedMemberships.length === 0 && hasMembership && (
            <p>The customer already holds this membership, so nothing changed.</p>
          )}
          {order.activatedMemberships.length === 0 && !hasMembership && <p>No membership in this order.</p>}
        </div>
        {order.activatedMemberships.length > 0 && <span className="stamp">Activated</span>}
      </div>

      <div className="outcome">
        <div>
          <h4>Shipping slip (BR2)</h4>
          <p>
            {order.shippingSlipGenerated
              ? "The order has a physical product, so a shipping slip was generated."
              : "No physical product in this order, so no shipping slip."}
          </p>
        </div>
        {order.shippingSlipGenerated && <span className="stamp">Generated</span>}
      </div>
    </article>
  );
}

// The slip itself is not part of the order response, so it is fetched from its own endpoint.
function ShippingSlipView({ orderId }: { orderId: number }) {
  const slip = useLoad(() => api.getShippingSlip(orderId), [orderId]);

  if (slip.error) return <ErrorNotice error={slip.error} />;
  if (!slip.data) return <p className="hint">Loading the shipping slip...</p>;

  return (
    <article className="slip slip-shipping">
      <h3 className="slip-title">Shipping slip</h3>
      <p className="hint">
        Order {slip.data.purchaseOrderId}, customer ID {slip.data.customerId}. Generated {when(slip.data.generatedAt)}.
      </p>
      <ul className="parcel">
        {slip.data.items.map((item, index) => (
          <li key={`${index}-${item.productId}`}>{item.productName}</li>
        ))}
      </ul>
    </article>
  );
}
