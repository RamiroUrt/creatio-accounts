import type { AccountItem } from "./AccountItem";

export interface AccountsTableProps {
  accounts: AccountItem[];
  loading?: boolean;
  emptyMessage?: string;
}