import type { AccountItem } from "./AccountItem";

export interface AccountListResponse {
  items: AccountItem[];
  total: number;
  page: number;
  pageSize: number;
}