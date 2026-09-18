import type { AccountItem, AccountListResponse, AccountType, ApiError, CreateAccountPayload } from "../types";

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "/api";

async function readError(response: Response): Promise<ApiError> {
  try {
    const body = await response.json();
    if (body?.error?.message) {
      return body;
    }
  } catch {
    // sin cuerpo JSON
  }
  return { error: { code: response.status, message: `HTTP ${response.status}` } };
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, init);
  if (!response.ok) {
    const err = await readError(response);
    throw new Error(err.error.message || "Falló la solicitud");
  }
  return (await response.json()) as T;
}

export function getAccounts(
  search: string,
  type: string,
  page: number,
  pageSize: number,
  signal?: AbortSignal,
): Promise<AccountListResponse> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (search.trim()) {
    params.set("search", search.trim());
  }
  if (type.trim()) {
    params.set("type", type.trim());
  }
  return request<AccountListResponse>(`${API_BASE}/accounts?${params.toString()}`, { signal });
}

export function getAccountTypes(signal?: AbortSignal): Promise<AccountType[]> {
  return request<AccountType[]>(`${API_BASE}/account-types`, { signal });
}

export function createAccount(payload: CreateAccountPayload): Promise<AccountItem> {
  return request<AccountItem>(`${API_BASE}/accounts`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}