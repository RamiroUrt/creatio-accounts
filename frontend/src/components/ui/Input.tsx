import { forwardRef } from "react";
import type { InputProps } from "../../types";

const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ className = "", ...props }, ref) => {
    return (
      <input
        ref={ref}
        className={`input ${className}`}
        {...props}
      />
    );
  }
);

Input.displayName = "Input";

export default Input;