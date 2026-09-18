export interface PaginationProps {
  page: number;
  totalPages: number;
  total: number;
  disabled?: boolean;
  onPrev: () => void;
  onNext: () => void;
}