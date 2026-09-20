import { useState, type FormEvent } from "react";
import { api, asApiError, type ApiError } from "./api";
import type { Category, Product, ProductKind } from "./types";
import type { Loaded } from "./useLoad";
import { Categories, ErrorNotice, KindBadge, money } from "./ui";

const KINDS: ProductKind[] = ["Physical", "Digital", "Membership"];
const CATEGORIES: Category[] = ["Book", "Video"];

interface Props {
  products: Loaded<Product[]>;
  onCreated: () => void;
}

export function Catalog({ products, onCreated }: Props) {
  const [kind, setKind] = useState<ProductKind>("Physical");
  const [name, setName] = useState("");
  const [price, setPrice] = useState("");
  const [categories, setCategories] = useState<Category[]>(["Book"]);
  const [error, setError] = useState<ApiError>();

  const toggle = (category: Category) =>
    setCategories((current) =>
      current.includes(category) ? current.filter((c) => c !== category) : [...current, category],
    );

  async function create(event: FormEvent) {
    event.preventDefault();
    setError(undefined);

    try {
      // A physical or digital product needs exactly one category. The form does not check that: the API does.
      await api.createProduct({ kind, name, price: Number(price), categories });
      setName("");
      setPrice("");
      onCreated();
    } catch (e) {
      setError(asApiError(e));
    }
  }

  return (
    <section>
      <h2>Catalog</h2>
      <p className="hint">
        Physical products go on a shipping slip (BR2). Memberships are activated on the customer (BR1). Digital products
        need nothing extra.
      </p>

      {products.error && <ErrorNotice error={products.error} />}
      {!products.data && !products.error && <p className="hint">Loading the catalog...</p>}

      {products.data && (
        <div className="table-scroll">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Kind</th>
                <th>Categories</th>
                <th className="num">Price</th>
              </tr>
            </thead>
            <tbody>
              {products.data.map((product) => (
                <tr key={product.id}>
                  <td>{product.id}</td>
                  <td>{product.name}</td>
                  <td>
                    <KindBadge kind={product.kind} />
                  </td>
                  <td>
                    <Categories values={product.categories} />
                  </td>
                  <td className="num">{money(product.price)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <form className="form-row" onSubmit={create} noValidate>
        <div className="field">
          <label htmlFor="product-kind">Kind</label>
          <select id="product-kind" value={kind} onChange={(e) => setKind(e.target.value as ProductKind)}>
            {KINDS.map((k) => (
              <option key={k}>{k}</option>
            ))}
          </select>
        </div>
        <div className="field grow">
          <label htmlFor="product-name">Name</label>
          <input id="product-name" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="field narrow">
          <label htmlFor="product-price">Price</label>
          <input
            id="product-price"
            type="number"
            step="0.01"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
          />
        </div>
        <fieldset className="field">
          <legend>Categories</legend>
          <div className="checks">
            {CATEGORIES.map((category) => (
              <label key={category}>
                <input type="checkbox" checked={categories.includes(category)} onChange={() => toggle(category)} />
                {category}
              </label>
            ))}
          </div>
        </fieldset>
        <button type="submit">Add product</button>
      </form>
      {error && <ErrorNotice error={error} />}
    </section>
  );
}
