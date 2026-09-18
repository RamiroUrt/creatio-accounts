import Table from "../ui/Table";
import type { AccountItem, AccountsTableProps } from "../../types";
import { ACCOUNT_COLUMNS } from "../../assets/constants/accountColumns";

export default function AccountsTable({ accounts, loading, emptyMessage }: AccountsTableProps) {
  return (
    <Table<AccountItem>
      columns={ACCOUNT_COLUMNS}
      data={accounts}
      loading={loading}
      emptyMessage={emptyMessage ?? "Sin cuentas para estos filtros."}
    />
  );
}