import { useState } from "react";
import AccountsTable from "../components/accounts/AccountsTable";
import AccountTypeFilter from "../components/accounts/AccountTypeFilter";
import CreateAccountForm from "../components/accounts/CreateAccountForm";
import Button from "../components/ui/Button";
import ErrorBanner from "../components/ui/ErrorBanner";
import Input from "../components/ui/Input";
import Pagination from "../components/ui/Pagination";
import { useAccountTypes } from "../hooks/useAccountTypes";
import { useAccounts } from "../hooks/useAccounts";
import { useDebounce } from "../hooks/useDebounce";
import { ACCOUNT_PAGE_SIZE } from "../assets/constants/accountColumns";

export default function AccountsView() {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebounce(search, 400);
  const [type, setType] = useState("");
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);

  const types = useAccountTypes();
  const { items, total, loading, error, refresh } = useAccounts(debouncedSearch, type, page, ACCOUNT_PAGE_SIZE);
  const totalPages = Math.max(1, Math.ceil(total / ACCOUNT_PAGE_SIZE));

  function handleSearchChange(value: string) {
    setSearch(value);
    setPage(1);
  }

  function handleTypeChange(typeName: string) {
    setType(typeName);
    setPage(1);
  }

  function handleCreated() {
    setPage(1);
    setSearch("");
    setShowCreate(false);
    refresh();
  }

  return (
    <div className="page">
      <div className="page__wrap">
        <header className="view-header">
          <img src="/alkemia-logo.png" alt="Alkemia" className="view-logo" />
          <div>
            <h1 className="view-header__title">Cuentas · Creatio</h1>
            <p className="view-header__subtitle">
              Lista vía API OData de Creatio con OAuth 2.0
            </p >
            <p className="view-header__subtitle" >
              Creatio v2026.5.6</p>
          </div>
        </header>

        {error && <ErrorBanner title="Ocurrió un error" message={error} />}

        <div className="view-toolbar">
          <div className="field view-toolbar__search">
            <label className="field__label" htmlFor="search-accounts">
              Buscar por nombre
            </label>
            <Input
              id="search-accounts"
              type="text"
              value={search}
              placeholder="Ej: Apex"
              onChange={(e) => handleSearchChange(e.target.value)}
            />
          </div>
          <Button
            variant={showCreate ? "ghost" : "primary"}
            onClick={() => setShowCreate((v) => !v)}
          >
            {showCreate ? "Cancelar" : "+ Nueva cuenta"}
          </Button>
        </div>

        <div className="view-filters">
          <AccountTypeFilter
            types={types}
            selected={type}
            disabled={loading}
            onSelect={handleTypeChange}
          />
        </div>

        {showCreate && (
          <div>
            <CreateAccountForm onCreated={handleCreated} />
          </div>
        )}

        <AccountsTable
          accounts={items}
          loading={loading}
          emptyMessage={
            error ? "No hay resultados para mostrar." : "Sin cuentas para estos filtros."
          }
        />

        <footer className="view-footer">
          <Pagination
            page={page}
            totalPages={totalPages}
            total={total}
            disabled={loading}
            onPrev={() => setPage(page - 1)}
            onNext={() => setPage(page + 1)}
          />
        </footer>
      </div>
    </div>
  );
}