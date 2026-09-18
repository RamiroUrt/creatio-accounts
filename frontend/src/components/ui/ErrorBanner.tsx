import type { ErrorBannerProps } from "../../types";

export default function ErrorBanner({ title, message }: ErrorBannerProps) {
  return (
    <div role="alert" className="banner banner--error">
      <div>
        <strong>{title}</strong>
        <div className="banner__message">{message}</div>
      </div>
    </div>
  );
}