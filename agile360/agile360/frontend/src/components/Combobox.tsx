/**
 * Combobox — Autocomplete formal para o Agile360 Design System v2.0
 *
 * Funcionalidades:
 *  - Filtragem em tempo real por label E sublabel (ex.: nome + CPF)
 *  - Normalização de acentos: "joao" filtra "João"
 *  - Cap de 50 resultados renderizados (performance com centenas de itens)
 *  - Teclado: ↑↓ navega, Enter seleciona, Esc fecha
 *  - Modo readonly: exibe o valor selecionado com ícone de cadeado
 *  - Modo clearable: botão "×" para limpar a seleção
 *  - Acessível: role="combobox", aria-expanded, aria-activedescendant
 */

import {
  useEffect,
  useId,
  useMemo,
  useRef,
  useState,
} from 'react';
import { ChevronDown, Lock, X } from 'lucide-react';

// ─── Tipos ─────────────────────────────────────────────────────────────────

export interface ComboboxOption {
  value:     string;
  label:     string;
  /** Texto secundário para filtragem e exibição (ex.: CPF, nº processo) */
  sublabel?: string;
}

export interface ComboboxProps {
  label:        string;
  options:      ComboboxOption[];
  /** ID selecionado (string vazia = sem seleção) */
  value:        string;
  onChange:     (value: string) => void;
  placeholder?: string;
  /** Quando true: mostra o valor com cadeado, sem dropdown */
  readonly?:    boolean;
  disabled?:    boolean;
  error?:       string;
  /** Texto exibido quando readonly e valor está preenchido automaticamente */
  readonlyHint?: string;
}

// ─── Utilitário: normalizar acentos para comparação ────────────────────────

function norm(s: string): string {
  return s
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '');
}

// ─── Componente ────────────────────────────────────────────────────────────

export function Combobox({
  label,
  options,
  value,
  onChange,
  placeholder = 'Digite para buscar…',
  readonly = false,
  disabled = false,
  error,
  readonlyHint,
}: ComboboxProps) {
  const uid          = useId();
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef     = useRef<HTMLInputElement>(null);
  const listRef      = useRef<HTMLUListElement>(null);

  const [open,        setOpen]        = useState(false);
  const [query,       setQuery]       = useState('');
  const [highlighted, setHighlighted] = useState(0);

  // Opção atualmente selecionada
  const selected = useMemo(
    () => options.find(o => o.value === value),
    [options, value],
  );

  // ── Filtragem com cap de 50 para performance ─────────────────────────────
  const filtered = useMemo<ComboboxOption[]>(() => {
    const q = norm(query.trim());
    if (!q) return options.slice(0, 50);
    return options
      .filter(o => {
        const haystack = norm(o.label + ' ' + (o.sublabel ?? ''));
        return haystack.includes(q);
      })
      .slice(0, 50);
  }, [options, query]);

  // Resetar highlight quando a lista filtrada muda
  useEffect(() => { setHighlighted(0); }, [filtered]);

  // ── Fechar ao clicar fora ─────────────────────────────────────────────────
  useEffect(() => {
    if (!open) return;
    const handler = (e: MouseEvent) => {
      if (!containerRef.current?.contains(e.target as Node)) {
        setOpen(false);
        setQuery('');
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [open]);

  // ── Scroll automático do item destacado ──────────────────────────────────
  useEffect(() => {
    if (!open) return;
    const el = listRef.current?.children[highlighted] as HTMLElement | undefined;
    el?.scrollIntoView({ block: 'nearest' });
  }, [highlighted, open]);

  // ── Handlers ─────────────────────────────────────────────────────────────

  function handleFocus() {
    if (readonly || disabled) return;
    setQuery('');
    setHighlighted(0);
    setOpen(true);
  }

  function handleInput(e: React.ChangeEvent<HTMLInputElement>) {
    setQuery(e.target.value);
    if (!open) setOpen(true);
    setHighlighted(0);
  }

  function handleSelect(opt: ComboboxOption) {
    onChange(opt.value);
    setQuery('');
    setOpen(false);
    inputRef.current?.blur();
  }

  function handleClear(e: React.MouseEvent) {
    e.stopPropagation();
    onChange('');
    setQuery('');
    setOpen(false);
    setTimeout(() => inputRef.current?.focus(), 0);
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    switch (e.key) {
      case 'Escape':
        setOpen(false);
        setQuery('');
        inputRef.current?.blur();
        break;
      case 'ArrowDown':
        e.preventDefault();
        if (!open) { setOpen(true); break; }
        setHighlighted(h => Math.min(h + 1, filtered.length - 1));
        break;
      case 'ArrowUp':
        e.preventDefault();
        setHighlighted(h => Math.max(h - 1, 0));
        break;
      case 'Enter':
        e.preventDefault();
        if (open && filtered[highlighted]) {
          handleSelect(filtered[highlighted]);
        }
        break;
      case 'Tab':
        setOpen(false);
        setQuery('');
        break;
    }
  }

  // ── Valor exibido no input ────────────────────────────────────────────────
  //  - Quando aberto: mostra o query digitado
  //  - Quando fechado: mostra o label do item selecionado (ou vazio)
  const inputDisplayValue = open ? query : (selected?.label ?? '');

  // ── Classes do input ──────────────────────────────────────────────────────
  const inputBase = [
    'w-full min-h-[var(--input-min-height)] rounded-[var(--radius)] border',
    'pl-3 pr-10 text-sm outline-none transition-colors',
    'placeholder:text-[var(--color-text-muted)]',
  ].join(' ');

  const inputState = readonly
    ? 'cursor-default'
    : disabled
    ? 'cursor-not-allowed opacity-50'
    : 'cursor-text';

  const inputBorder = error
    ? 'border-[var(--color-error)] focus:border-[var(--color-error)]'
    : open
    ? 'border-[var(--color-primary)]'
    : 'border-[var(--color-border)] hover:border-[var(--color-border-strong)]';

  return (
    <div className="flex flex-col gap-1" ref={containerRef}>

      {/* Label */}
      <label
        htmlFor={uid}
        className="text-sm"
        style={{ color: 'var(--color-text-muted)' }}
      >
        {label}
        {readonly && readonlyHint && (
          <span className="ml-2 text-xs" style={{ color: 'var(--color-primary)' }}>
            {readonlyHint}
          </span>
        )}
      </label>

      {/* Input wrapper */}
      <div className="relative">
        <input
          id={uid}
          ref={inputRef}
          type="text"
          role="combobox"
          aria-expanded={open}
          aria-haspopup="listbox"
          aria-autocomplete="list"
          aria-activedescendant={open && filtered[highlighted] ? `${uid}-opt-${highlighted}` : undefined}
          aria-invalid={!!error}
          autoComplete="off"
          value={inputDisplayValue}
          onChange={handleInput}
          onFocus={handleFocus}
          onKeyDown={handleKeyDown}
          readOnly={readonly}
          disabled={disabled}
          placeholder={readonly ? (selected?.label ?? '—') : placeholder}
          className={`${inputBase} ${inputState} ${inputBorder}`}
          style={{
            background: 'var(--color-surface-elevated)',
            color:      readonly
              ? 'var(--color-text-secondary)'
              : 'var(--color-text)',
          }}
        />

        {/* Ícone à direita */}
        <div
          className="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3"
          aria-hidden="true"
        >
          {readonly ? (
            <Lock
              size={13}
              strokeWidth={1.5}
              style={{ color: 'var(--color-text-muted)' }}
            />
          ) : value ? (
            /* Botão limpar — tem pointer-events */
            <button
              type="button"
              onMouseDown={handleClear}
              aria-label="Limpar seleção"
              tabIndex={-1}
              className="pointer-events-auto p-0.5 rounded transition-colors"
              style={{ color: 'var(--color-text-muted)' }}
              onMouseEnter={e => (e.currentTarget.style.color = 'var(--color-error)')}
              onMouseLeave={e => (e.currentTarget.style.color = 'var(--color-text-muted)')}
            >
              <X size={13} strokeWidth={2} />
            </button>
          ) : (
            <ChevronDown
              size={14}
              strokeWidth={1.5}
              style={{
                color: 'var(--color-text-muted)',
                transform: open ? 'rotate(180deg)' : 'none',
                transition: 'transform 0.15s ease',
              }}
            />
          )}
        </div>

        {/* Dropdown */}
        {open && !readonly && !disabled && (
          <ul
            ref={listRef}
            role="listbox"
            aria-label={label}
            className="absolute left-0 right-0 top-[calc(100%+4px)] z-[200] max-h-52 overflow-y-auto rounded-[var(--radius)] border py-1 shadow-lg"
            style={{
              background:   'var(--color-surface-elevated)',
              borderColor:  'var(--color-border-strong)',
              boxShadow:    '0 8px 24px rgba(0,0,0,.45)',
            }}
          >
            {filtered.length === 0 ? (
              <li
                className="px-3 py-2 text-sm"
                style={{ color: 'var(--color-text-muted)' }}
              >
                Nenhum resultado para <strong>"{query}"</strong>
              </li>
            ) : (
              filtered.map((opt, i) => {
                const isHighlighted = i === highlighted;
                return (
                  <li
                    key={opt.value}
                    id={`${uid}-opt-${i}`}
                    role="option"
                    aria-selected={opt.value === value}
                    onMouseDown={e => { e.preventDefault(); handleSelect(opt); }}
                    onMouseEnter={() => setHighlighted(i)}
                    className="flex cursor-pointer flex-col px-3 py-2 text-sm transition-colors"
                    style={{
                      background: isHighlighted
                        ? 'var(--color-surface-hover)'
                        : opt.value === value
                        ? 'var(--color-nav-active-bg)'
                        : 'transparent',
                      color: opt.value === value
                        ? 'var(--color-primary)'
                        : 'var(--color-text)',
                      borderLeft: opt.value === value
                        ? '2px solid var(--color-primary)'
                        : '2px solid transparent',
                    }}
                  >
                    <span className="font-medium leading-snug">{opt.label}</span>
                    {opt.sublabel && (
                      <span
                        className="text-xs leading-snug"
                        style={{ color: isHighlighted ? 'var(--color-text-secondary)' : 'var(--color-text-muted)' }}
                      >
                        {opt.sublabel}
                      </span>
                    )}
                  </li>
                );
              })
            )}

            {/* Rodapé: quantos foram filtrados */}
            {options.length > 50 && (
              <li
                className="label-uppercase border-t px-3 py-1.5"
                style={{
                  borderColor: 'var(--color-border)',
                  color:       'var(--color-text-muted)',
                }}
              >
                {filtered.length < options.length
                  ? `${filtered.length} resultado${filtered.length !== 1 ? 's' : ''} de ${options.length}`
                  : `${options.length} total — refine a busca`}
              </li>
            )}
          </ul>
        )}
      </div>

      {/* Mensagem de erro */}
      {error && (
        <p className="text-xs" style={{ color: 'var(--color-error)' }} role="alert">
          {error}
        </p>
      )}
    </div>
  );
}
