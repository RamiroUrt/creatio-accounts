import { useCallback, useEffect, useState } from "react";
import { getAccounts } from "../lib/api";
import type { AccountItem } from "../types";

export function useAccounts(search: string, type: string, page: number, pageSize: number) {
  const [items, setItems] = useState<AccountItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();

    setLoading(true);
    setError(null);

    getAccounts(search, type, page, pageSize, controller.signal)
      .then((result) => {
        setItems(result.items);
        setTotal(result.total);
      })
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        setItems([]);
        setError(err instanceof Error ? err.message : "Error desconocido");
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [search, type, page, pageSize, refreshKey]);

  const refresh = useCallback(() => setRefreshKey((key) => key + 1), []);

  return { items, total, loading, error, refresh };
}