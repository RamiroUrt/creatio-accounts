import { useState } from "react";
import Button from "../ui/Button";
import Input from "../ui/Input";
import ErrorBanner from "../ui/ErrorBanner";
import { createAccount } from "../../lib/api";
import type { AccountType, CreateAccountFormProps } from "../../types";
import { useAccountTypes } from "../../hooks/useAccountTypes";

export default function CreateAccountForm({ onCreated }: CreateAccountFormProps) {
  const [name, setName] = useState("");
  const [typeId, setTypeId] = useState("");
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const types = useAccountTypes();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;

    setCreating(true);
    setError(null);
    try {
      await createAccount({
        name: name.trim(),
        ...(typeId ? { typeId } : {}),
      });
      setName("");
      setTypeId("");
      onCreated();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error desconocido");
    } finally {
      setCreating(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="card">
      <div className="field">
        <label className="field__label" htmlFor="account-name">
          Nombre de la cuenta
        </label>
        <Input
          id="account-name"
          type="text"
          value={name}
          placeholder="Nombre de la cuenta"
          onChange={(e) => setName(e.target.value)}
          disabled={creating}
        />
      </div>
      <div className="field">
        <label className="field__label" htmlFor="account-type">
          Tipo de cuenta
        </label>
        <select
          id="account-type"
          className="input"
          value={typeId}
          onChange={(e) => setTypeId(e.target.value)}
          disabled={creating || types.length === 0}
        >
          <option value="">(Sin tipo)</option>
          {types.map((t: AccountType) => (
            <option key={t.id} value={t.id}>
              {t.name}
            </option>
          ))}
        </select>
      </div>
      <div className="create-form__row">
        <Button type="submit" disabled={creating || !name.trim()} loading={creating}>
          Crear cuenta
        </Button>
      </div>
      {error && <ErrorBanner title="No se pudo crear la cuenta" message={error} />}
    </form>
  );
}