import type { InputHTMLAttributes } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  id?: string;
}

export function Input({ label, error, id: idProp, ...props }: InputProps) {
  const id = idProp ?? props.name ?? `input-${Math.random().toString(36).slice(2)}`;
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={id} className="text-sm text-[var(--color-text-muted)]">
        {label}
      </label>
      <input
        id={id}
        className="min-h-[var(--input-min-height)] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] placeholder:text-[var(--color-text-muted)] focus:border-[var(--color-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--color-focus-ring)] focus:ring-offset-2 focus:ring-offset-[var(--color-bg)] aria-invalid={error ? 'true' : undefined}"
        style={{ minHeight: 'var(--input-min-height)' }}
        aria-describedby={error ? `${id}-error` : undefined}
        aria-invalid={!!error}
        {...props}
      />
      {error && (
        <p id={`${id}-error`} className="text-sm text-[var(--color-error)]" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}
