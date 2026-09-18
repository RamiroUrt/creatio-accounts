import Button from "./Button";
import type { PaginationProps } from "../../types";

export default function Pagination({
  page,
  totalPages,
  total,
  disabled = false,
  onPrev,
  onNext,
}: PaginationProps) {
  return (
    <div className="pagination">
      <span className="pagination__info">
        Página {page} de {totalPages} · {total} cuentas
      </span>
      <div className="pagination__actions">
        <Button
          variant="secondary"
          size="sm"
          disabled={disabled || page <= 1}
          onClick={onPrev}
        >
          ← Anterior
        </Button>
        <Button
          variant="secondary"
          size="sm"
          disabled={disabled || page >= totalPages}
          onClick={onNext}
        >
          Siguiente →
        </Button>
      </div>
    </div>
  );
}