import { useEffect } from 'react';

export type ToastType = 'success' | 'error';

interface ToastProps {
  message: string;
  type: ToastType;
  onClose: () => void;
  duration?: number;
}

export function Toast({ message, type, onClose, duration = 5000 }: ToastProps) {
  useEffect(() => {
    const t = setTimeout(onClose, duration);
    return () => clearTimeout(t);
  }, [onClose, duration]);

  const borderColor = type === 'success' ? 'var(--color-success)' : 'var(--color-error)';
  return (
    <div
      role="status"
      aria-live={type === 'error' ? 'assertive' : 'polite'}
      className="fixed top-4 right-4 z-50 flex max-w-sm rounded-[var(--radius)] border-l-4 bg-[var(--color-surface)] p-4 shadow-lg"
      style={{ borderLeftColor: borderColor }}
    >
      <p className="text-[var(--color-text)]">{message}</p>
    </div>
  );
}
