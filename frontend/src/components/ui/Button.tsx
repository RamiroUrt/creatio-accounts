import { forwardRef } from "react";
import type { ButtonProps } from "../../types";

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = "primary",
      size = "md",
      loading = false,
      className = "",
      disabled,
      children,
      ...props
    },
    ref
  ) => {
    return (
      <button
        ref={ref}
        disabled={disabled || loading}
        className={`btn btn--${variant} btn--${size} ${className}`}
        {...props}
      >
        {loading && <span className="btn__spinner" />}
        {children}
      </button>
    );
  }
);

Button.displayName = "Button";

export default Button;