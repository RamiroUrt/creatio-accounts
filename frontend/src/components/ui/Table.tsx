import type { ReactNode } from "react";
import type { TableProps } from "../../types";

export default function Table<T extends object>({
  columns,
  data,
  onRowClick,
  emptyMessage = "No hay datos para mostrar",
  loading = false,
}: TableProps<T>) {
  if (loading) {
    return (
      <div className="table-card">
        <p className="empty">Cargando cuentas…</p>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="table-card">
        <p className="empty">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div className="table-card">
      <table className="table">
        <thead>
          <tr>
            {columns.map((col) => (
              <th key={col.key} className={col.className || ""}>
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((item, idx) => (
            <tr
              key={idx}
              onClick={() => onRowClick?.(item)}
              style={onRowClick ? { cursor: "pointer" } : undefined}
            >
              {columns.map((col) => (
                <td key={col.key} className={col.className || ""}>
                  {col.render
                    ? col.render(item)
                    : ((item as Record<string, unknown>)[col.key] as ReactNode)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}