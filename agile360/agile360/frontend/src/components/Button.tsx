import type { ButtonHTMLAttributes, ReactNode } from 'react';

type Variant = 'primary' | 'secondary' | 'ghost';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  loading?: boolean;
  children: ReactNode;
}

const base =
  'min-h-[44px] px-6 py-3 rounded-[var(--radius)] font-semibold transition-colors disabled:opacity-60 disabled:cursor-not-allowed focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]';

const variants: Record<Variant, string> = {
  primary:
    'bg-[var(--color-primary)] text-[var(--color-white)] hover:bg-[var(--color-primary-hover)]',
  secondary:
    'bg-transparent border border-[var(--color-border)] text-[var(--color-text)] hover:border-[var(--color-text-muted)] hover:bg-[var(--color-surface)]',
  ghost: 'bg-transparent text-[var(--color-primary)] hover:text-[var(--color-primary-hover)] hover:underline',
};

export function Button({
  variant = 'primary',
  loading,
  disabled,
  children,
  className = '',
  ...props
}: ButtonProps) {
  return (
    <button
      type="button"
      className={`${base} ${variants[variant]} ${className}`}
      disabled={disabled || loading}
      aria-busy={loading}
      {...props}
    >
      {loading ? 'Aguarde…' : children}
    </button>
  );
}
