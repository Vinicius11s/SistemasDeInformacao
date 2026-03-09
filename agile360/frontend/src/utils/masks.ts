/**
 * masks.ts — Clean Data helpers
 *
 * Two responsibilities:
 *   1. INPUT MASKS  — apply while the user types (returns formatted string)
 *   2. DISPLAY      — format raw digits coming from the API for read-only rendering
 *
 * Contract: the API always sends and receives raw digits only.
 * The UI is the only place where formatted representations appear.
 */

// ── Strip helpers ───────────────────────────────────────────────────────────

/** Returns only digit characters from a string. */
export function digitsOnly(value: string): string {
  return value.replace(/\D/g, '');
}

// ── Input mask functions (called on onChange) ───────────────────────────────
// Each function returns the formatted value as the user types.

export function maskCpf(raw: string): string {
  const d = digitsOnly(raw).slice(0, 11);
  return d
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})$/, '$1-$2');
}

export function maskCnpj(raw: string): string {
  const d = digitsOnly(raw).slice(0, 14);
  return d
    .replace(/(\d{2})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1/$2')
    .replace(/(\d{4})(\d{1,2})$/, '$1-$2');
}

export function maskRg(raw: string): string {
  // Brazilian RG: up to 9 digits → 00.000.000-0
  const d = digitsOnly(raw).slice(0, 9);
  return d
    .replace(/(\d{2})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1})$/, '$1-$2');
}

export function maskPhone(raw: string): string {
  const d = digitsOnly(raw).slice(0, 11);
  if (d.length <= 10) {
    return d
      .replace(/(\d{2})(\d)/, '($1) $2')
      .replace(/(\d{4})(\d{1,4})$/, '$1-$2');
  }
  return d
    .replace(/(\d{2})(\d)/, '($1) $2')
    .replace(/(\d{5})(\d{4})$/, '$1-$2');
}

// ── Display formatters (called when rendering API data) ─────────────────────
// Input is always raw digits (as stored in the DB).

export function formatCpf(raw: string | null | undefined): string {
  if (!raw) return '—';
  const d = digitsOnly(raw);
  if (d.length !== 11) return raw; // return as-is if malformed
  return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9)}`;
}

export function formatCnpj(raw: string | null | undefined): string {
  if (!raw) return '—';
  const d = digitsOnly(raw);
  if (d.length !== 14) return raw;
  return `${d.slice(0, 2)}.${d.slice(2, 5)}.${d.slice(5, 8)}/${d.slice(8, 12)}-${d.slice(12)}`;
}

export function formatRg(raw: string | null | undefined): string {
  if (!raw) return '—';
  const d = digitsOnly(raw);
  if (d.length < 7) return raw;
  return `${d.slice(0, 2)}.${d.slice(2, 5)}.${d.slice(5, 8)}-${d.slice(8)}`;
}

export function formatPhone(raw: string | null | undefined): string {
  if (!raw) return '—';
  const d = digitsOnly(raw);
  if (d.length === 11) return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`;
  if (d.length === 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`;
  return raw;
}

/**
 * Convenience: format any document based on digit count.
 * 11 digits → CPF, 14 digits → CNPJ, anything else → raw.
 */
export function formatDocument(raw: string | null | undefined): string {
  if (!raw) return '—';
  const d = digitsOnly(raw);
  if (d.length === 11) return formatCpf(d);
  if (d.length === 14) return formatCnpj(d);
  return raw;
}

// ── Submit helpers ──────────────────────────────────────────────────────────

/**
 * Strips a masked field value back to raw digits before sending to the API.
 * Returns undefined instead of empty string (omits the field from the payload).
 */
export function rawDigits(masked: string | undefined): string | undefined {
  if (!masked) return undefined;
  const d = digitsOnly(masked);
  return d.length > 0 ? d : undefined;
}
