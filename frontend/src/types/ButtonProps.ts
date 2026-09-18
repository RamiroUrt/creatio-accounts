import type { ButtonHTMLAttributes } from "react";
import type { ButtonSize } from "./ButtonSize";
import type { ButtonVariant } from "./ButtonVariant";

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
}