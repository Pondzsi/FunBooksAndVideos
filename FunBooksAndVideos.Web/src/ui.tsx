import type { ReactNode } from "react";
import type { ApiError } from "./api";
import type { Category, ProductKind } from "./types";

// The API sends prices as numbers with no currency, and the PDF shows none either, so this is plain 2-decimal formatting.
export const money = (amount: number) => amount.toFixed(2);

export const when = (iso: string) => new Date(iso).toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" });

// Shows any ApiError: title and detail, the field messages of a 400, and the missing ID of a 422.
export function ErrorNotice({ error }: { error: ApiError }) {
  const fields = Object.entries(error.fieldErrors);

  return (
    <div className="notice" role="alert">
      <p className="notice-title">
        {error.title}
        {error.status > 0 && <span className="notice-status">{error.status}</span>}
      </p>
      {error.detail && <p>{error.detail}</p>}
      {error.missing && (
        <p>
          Missing <code>{error.missing.field}</code>: {error.missing.id}
        </p>
      )}
      {fields.length > 0 && (
        <ul>
          {fields.map(([field, messages]) => (
            <li key={field}>
              <strong>{field}</strong>: {messages.join(" ")}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

// Physical products are shipped (BR2), memberships are activated (BR1), digital products need nothing.
// The border style tells them apart (see styles.css), so the kind is never only a color.
export function KindBadge({ kind }: { kind: ProductKind }) {
  return (
    <span className="kind" data-kind={kind}>
      {kind}
    </span>
  );
}

// Book or video. For a membership these are the categories it grants access to.
export function Categories({ values }: { values: Category[] }) {
  return (
    <span className="tags">
      {values.map((category) => (
        <span key={category} className="category" data-category={category}>
          {category}
        </span>
      ))}
    </span>
  );
}

export interface Line {
  key: string;
  name: string;
  kind?: ProductKind;
  categories?: Category[];
  price?: number;
  action?: ReactNode;
}

// Order lines the way a receipt sets them: the name, a dotted leader, the price, and a total under a double rule.
// Used for the order being built and for the order that was placed.
export function LineItems({ lines, total }: { lines: Line[]; total: number }) {
  return (
    <>
      <ul className="lines">
        {lines.map((line) => (
          <li key={line.key}>
            <div className="line-main">
              <span>{line.name}</span>
              <span className="leader" aria-hidden="true" />
              <span className="num">{line.price === undefined ? "?" : money(line.price)}</span>
            </div>
            {/* The action sits on the second line so the prices stay in one column, level with the total. */}
            {(line.kind || line.categories || line.action) && (
              <div className="line-tags">
                {line.kind && <KindBadge kind={line.kind} />}
                {line.categories && <Categories values={line.categories} />}
                {line.action && <span className="line-action">{line.action}</span>}
              </div>
            )}
          </li>
        ))}
      </ul>
      <p className="total">
        <span>Total</span>
        <span className="num">{money(total)}</span>
      </p>
    </>
  );
}
