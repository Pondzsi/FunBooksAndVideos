import { useState, type FormEvent } from "react";
import { api, asApiError, type ApiError } from "./api";
import type { Customer, Product } from "./types";
import type { Loaded } from "./useLoad";
import { Categories, ErrorNotice, when } from "./ui";

interface Props {
  customers: Loaded<Customer[]>;
  products: Product[]; // a membership only carries a product ID, so the name comes from the catalog
  selectedId: number | null;
  onSelect: (id: number) => void;
  onCreated: (customer: Customer) => void;
}

export function Customers({ customers, products, selectedId, onSelect, onCreated }: Props) {
  const [name, setName] = useState("");
  const [error, setError] = useState<ApiError>();

  async function create(event: FormEvent) {
    event.preventDefault();
    setError(undefined);

    try {
      const customer = await api.createCustomer(name);
      setName("");
      onCreated(customer);
    } catch (e) {
      setError(asApiError(e));
    }
  }

  const productName = (id: number) => products.find((product) => product.id === id)?.name ?? `Product ${id}`;

  return (
    <section>
      <h2>Customers</h2>

      {customers.error && <ErrorNotice error={customers.error} />}
      {!customers.data && !customers.error && <p className="hint">Loading customers...</p>}

      <div role="radiogroup" aria-label="Customer for the next order">
        {customers.data?.map((customer) => (
          <label key={customer.id} className="customer">
            <input
              type="radio"
              name="customer"
              checked={customer.id === selectedId}
              onChange={() => onSelect(customer.id)}
            />
            <div>
              <p className="customer-name">
                <strong>{customer.name}</strong> <span className="id">ID {customer.id}</span>
              </p>
              {customer.memberships.length === 0 && <p className="hint">No memberships yet.</p>}
              {customer.memberships.map((membership) => (
                <p key={membership.productId} className="membership">
                  {productName(membership.productId)} <Categories values={membership.categories} />{" "}
                  <span className="hint">since {when(membership.activatedAt)}</span>
                </p>
              ))}
              {customer.accessibleCategories.length > 0 && (
                <p className="membership">
                  Can access <Categories values={customer.accessibleCategories} />
                </p>
              )}
            </div>
          </label>
        ))}
      </div>

      {/* No client-side validation anywhere in this UI (noValidate): the API's own error messages are what this demo shows. */}
      <form className="form-row" onSubmit={create} noValidate>
        <div className="field">
          <label htmlFor="customer-name">New customer</label>
          <input id="customer-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Name" />
        </div>
        <button type="submit">Add customer</button>
      </form>
      {error && <ErrorNotice error={error} />}
    </section>
  );
}
