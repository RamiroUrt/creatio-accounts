import type { AccountType } from "../../types";

interface AccountTypeFilterProps {
  types: AccountType[];
  selected: string;
  disabled?: boolean;
  onSelect: (typeName: string) => void;
}

export default function AccountTypeFilter({
  types,
  selected,
  disabled = false,
  onSelect,
}: AccountTypeFilterProps) {
  return (
    <div className="filter-chips">
      <button
        type="button"
        className={`filter-chips__item ${selected === "" ? "filter-chips__item--active" : ""}`}
        disabled={disabled}
        onClick={() => onSelect("")}
      >
        Todas
      </button>
      {types.map((type) => (
        <button
          key={type.id}
          type="button"
          className={`filter-chips__item ${selected === type.name ? "filter-chips__item--active" : ""}`}
          disabled={disabled}
          onClick={() => onSelect(type.name)}
        >
          {type.name}
        </button>
      ))}
    </div>
  );
}