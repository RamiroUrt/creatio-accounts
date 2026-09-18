import { useEffect, useState } from "react";
import { getAccountTypes } from "../lib/api";
import type { AccountType } from "../types";

export function useAccountTypes() {
  const [types, setTypes] = useState<AccountType[]>([]);

  useEffect(() => {
    const controller = new AbortController();
    getAccountTypes(controller.signal)
      .then(setTypes)
      .catch(() => setTypes([]));
    return () => controller.abort();
  }, []);

  return types;
}