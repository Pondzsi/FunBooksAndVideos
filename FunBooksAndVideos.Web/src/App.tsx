import { useState } from "react";
import { api } from "./api";
import { Catalog } from "./Catalog";
import { Customers } from "./Customers";
import { OrderBuilder } from "./OrderBuilder";
import { OrderResult } from "./OrderResult";
import { Orders } from "./Orders";
import type { PlacedOrder } from "./types";
import { useLoad } from "./useLoad";

export function App() {
  // Data that several sections show is loaded here once and handed down. After a change the sections call
  // back, and this component reloads whatever the change touched.
  const products = useLoad(api.getProducts);
  const customers = useLoad(api.getCustomers);
  const [orderFilter, setOrderFilter] = useState<number>();
  const orders = useLoad(() => api.getOrders(orderFilter), [orderFilter]);

  const [chosenCustomerId, setChosenCustomerId] = useState<number | null>(null);
  const [lastOrder, setLastOrder] = useState<PlacedOrder | null>(null);

  // Until somebody picks a customer the first one is used, so the page works straight after it loads.
  const customerId = chosenCustomerId ?? customers.data?.[0]?.id ?? null;

  function orderPlaced(order: PlacedOrder) {
    setLastOrder(order);
    customers.reload(); // BR1 may have added a membership to the customer
    orders.reload();
  }

  return (
    <>
      <header className="masthead">
        <h1>FunBooksAndVideos</h1>
        <p>Click through the API: place a purchase order and watch the two business rules run.</p>
      </header>

      <main className="page">
        <div className="columns">
          <div>
            <Customers
              customers={customers}
              products={products.data ?? []}
              selectedId={customerId}
              onSelect={setChosenCustomerId}
              onCreated={(customer) => {
                setChosenCustomerId(customer.id);
                customers.reload();
              }}
            />
            <Catalog products={products} onCreated={products.reload} />
          </div>
          <div>
            <OrderBuilder
              customers={customers.data ?? []}
              products={products.data ?? []}
              customerId={customerId}
              onCustomerChange={setChosenCustomerId}
              onPlaced={orderPlaced}
            />
            <OrderResult order={lastOrder} customers={customers.data ?? []} />
          </div>
        </div>

        <Orders orders={orders} customers={customers.data ?? []} filter={orderFilter} onFilterChange={setOrderFilter} />
      </main>
    </>
  );
}
