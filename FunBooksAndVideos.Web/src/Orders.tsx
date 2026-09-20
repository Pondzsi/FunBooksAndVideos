import type { Customer, Order } from "./types";
import type { Loaded } from "./useLoad";
import { ErrorNotice, money } from "./ui";

interface Props {
  orders: Loaded<Order[]>;
  customers: Customer[];
  filter: number | undefined; // a customer ID, or undefined for everyone
  onFilterChange: (customerId: number | undefined) => void;
}

export function Orders({ orders, customers, filter, onFilterChange }: Props) {
  const customerName = (id: number) => customers.find((customer) => customer.id === id)?.name;

  return (
    <section>
      <h2>Orders</h2>

      <div className="field filter">
        <label htmlFor="orders-filter">Show orders of</label>
        <select
          id="orders-filter"
          value={filter ?? ""}
          onChange={(e) => onFilterChange(e.target.value === "" ? undefined : Number(e.target.value))}
        >
          <option value="">All customers</option>
          {customers.map((customer) => (
            <option key={customer.id} value={customer.id}>
              {customer.name} ({customer.id})
            </option>
          ))}
        </select>
      </div>

      {orders.error && <ErrorNotice error={orders.error} />}
      {!orders.data && !orders.error && <p className="hint">Loading orders...</p>}
      {orders.data?.length === 0 && <p className="empty">No orders yet.</p>}

      {orders.data && orders.data.length > 0 && (
        <div className="table-scroll">
          <table>
            <thead>
              <tr>
                <th>Order</th>
                <th>Customer</th>
                <th>Items</th>
                <th className="num">Total</th>
              </tr>
            </thead>
            <tbody>
              {orders.data.map((order) => (
                <tr key={order.id}>
                  <td>{order.id}</td>
                  <td>
                    {customerName(order.customerId)} <span className="id">ID {order.customerId}</span>
                  </td>
                  <td>{order.items.map((item) => item.productName).join(", ")}</td>
                  <td className="num">{money(order.total)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
