import type { ReactNode } from "react";
import type { AccountItem } from "../../types";

export const ACCOUNT_PAGE_SIZE = 10;

export const ACCOUNT_COLUMNS: {
  key: string;
  header: string;
  className?: string;
  render: (item: AccountItem) => ReactNode;
}[] = [
  {
    key: "name",
    header: "Nombre",
    render: (item) => <strong>{item.name}</strong>,
  },
  {
    key: "type",
    header: "Tipo de cuenta",
    className: "table-cell--center",
    render: (item) => (
      <span className="tag">{item.typeName ?? "—"}</span>
    ),
  },
];