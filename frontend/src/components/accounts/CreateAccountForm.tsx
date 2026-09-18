import { useState } from "react";
import Button from "../ui/Button";
import Input from "../ui/Input";
import ErrorBanner from "../ui/ErrorBanner";
import { createAccount } from "../../lib/api";
import type { CreateAccountFormProps } from "../../types";

export default function CreateAccountForm({ onCreated }: CreateAccountFormProps) {
  const [name, setName] = useState("");
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;

    setCreating(true);
    setError(null);
    try {
      await createAccount({ name: name.trim() });
      setName("");
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
        <div className="create-form__row">
          <Input
            id="account-name"
            type="text"
            value={name}
            placeholder="Nombre de la cuenta"
            onChange={(e) => setName(e.target.value)}
            disabled={creating}
          />
          <Button type="submit" disabled={creating || !name.trim()} loading={creating}>
            Crear cuenta
          </Button>
        </div>
        {error && <ErrorBanner title="No se pudo crear la cuenta" message={error} />}
      </div>
    </form>
  );
}