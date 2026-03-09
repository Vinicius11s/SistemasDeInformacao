import { Link, Outlet, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  FolderOpen,
  CalendarDays,
  Clock,
  LogOut,
  Scale,
  Sun,
  Moon,
} from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';

// ─── Estrutura de navegação ────────────────────────────────────────────────
const navItems = [
  { to: '/app',            label: 'Painel',       icon: LayoutDashboard, exact: true  },
  { to: '/app/clientes',   label: 'Clientes',     icon: Users,           exact: false },
  { to: '/app/processos',  label: 'Processos',    icon: FolderOpen,      exact: false },
  { to: '/app/audiencias', label: 'Compromissos', icon: CalendarDays,    exact: false },
  { to: '/app/prazos',     label: 'Prazos',       icon: Clock,           exact: false },
];

// ─── Constantes de estilo inline ───────────────────────────────────────────
// Usa variáveis CSS para funcionar em ambos os temas (dark / light).
const ACTIVE_STYLE: React.CSSProperties = {
  backgroundColor: 'var(--color-nav-active-bg)',
  color:           'var(--color-primary)',
  fontWeight:      600,
  borderLeft:      '2px solid var(--color-primary)',
  paddingLeft:     10,
};

const INACTIVE_STYLE: React.CSSProperties = {
  color:       'var(--color-text-secondary)',
  borderLeft:  '2px solid transparent',
  paddingLeft: 10,
};

export function DashboardLayout() {
  const location           = useLocation();
  const { state, logout }  = useAuth();
  const { isDark, toggleTheme } = useTheme();

  return (
    <div className="flex min-h-screen">

      {/* ═══════════════════════════════════════════════════════════
          Sidebar — Deep Navy, tipografia sóbria, sem cores quentes
          ═══════════════════════════════════════════════════════════ */}
      <aside
        className="flex w-56 flex-col"
        style={{
          background:  'var(--color-surface-sidebar)',
          borderRight: '1px solid var(--color-border)',
        }}
        aria-label="Navegação principal"
      >

        {/* ── Marca / Logo ─────────────────────────────────────── */}
        <div
          className="flex items-center gap-2.5 px-4 py-[18px]"
          style={{ borderBottom: '1px solid var(--color-border)' }}
        >
          {/* Ícone da marca — institucional */}
          <span
            className="flex h-7 w-7 shrink-0 items-center justify-center rounded-[var(--radius)]"
            style={{ background: 'var(--color-primary)' }}
            aria-hidden="true"
          >
            <Scale size={14} color="#fff" strokeWidth={2} />
          </span>

          <Link
            to="/app"
            className="text-sm font-bold tracking-tight no-underline"
            style={{ color: 'var(--color-text-heading)', letterSpacing: '-0.02em' }}
          >
            Agile<span style={{ color: 'var(--color-primary)' }}>360</span>
          </Link>
        </div>

        {/* ── Navegação ────────────────────────────────────────── */}
        <nav
          className="flex flex-1 flex-col px-2 py-3"
          style={{ gap: 2 }}
          aria-label="Menu do sistema"
        >
          {/* Rótulo de seção */}
          <p
            className="label-uppercase mb-1 px-2"
            style={{ color: 'var(--color-text-muted)' }}
          >
            Menu
          </p>

          {navItems.map(({ to, label, icon: Icon, exact }) => {
            const isActive = exact
              ? location.pathname === to
              : location.pathname.startsWith(to);

            return (
              <Link
                key={to}
                to={to}
                className="flex min-h-[38px] items-center gap-2.5 rounded-[var(--radius)] pr-3 text-sm transition-colors no-underline"
                style={isActive ? ACTIVE_STYLE : INACTIVE_STYLE}
                onMouseEnter={e => {
                  if (!isActive) {
                    const el = e.currentTarget as HTMLElement;
                    el.style.backgroundColor = 'var(--color-surface-elevated)';
                    el.style.color           = 'var(--color-text)';
                  }
                }}
                onMouseLeave={e => {
                  if (!isActive) {
                    const el = e.currentTarget as HTMLElement;
                    el.style.backgroundColor = '';
                    el.style.color           = 'var(--color-text-secondary)';
                  }
                }}
              >
                <Icon
                  size={15}
                  strokeWidth={isActive ? 2 : 1.5}
                  style={{ flexShrink: 0 }}
                />
                {label}
              </Link>
            );
          })}
        </nav>

        {/* ── Perfil / Sair ────────────────────────────────────── */}
        <div
          className="px-3 py-4"
          style={{ borderTop: '1px solid var(--color-border)' }}
        >
          {/* Avatar inicial + nome */}
          <div className="flex items-center gap-2.5">
            <span
              className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-xs font-bold"
              style={{
                background: 'rgba(37,99,235,.15)',
                color:      'var(--color-primary)',
              }}
              aria-hidden="true"
            >
              {(state.user?.nome ?? state.user?.email ?? 'A').charAt(0).toUpperCase()}
            </span>
            <div className="min-w-0 flex-1">
              <p
                className="truncate text-xs font-semibold"
                style={{ color: 'var(--color-text-heading)' }}
              >
                {state.user?.nome ?? 'Usuário'}
              </p>
              <p
                className="truncate text-[10px]"
                style={{ color: 'var(--color-text-muted)' }}
              >
                {state.user?.email ?? ''}
              </p>
            </div>
          </div>

          {/* Linha de ações: toggle de tema + sair */}
          <div className="mt-3 flex items-center justify-between gap-2">

            {/* Toggle dark / light */}
            <button
              type="button"
              onClick={toggleTheme}
              title={isDark ? 'Ativar tema claro' : 'Ativar tema escuro'}
              className="flex items-center gap-1.5 rounded-[var(--radius)] px-2 py-1.5 text-xs transition-colors"
              style={{ color: 'var(--color-text-muted)' }}
              onMouseEnter={e => {
                const el = e.currentTarget as HTMLElement;
                el.style.color           = 'var(--color-primary)';
                el.style.backgroundColor = 'var(--color-nav-active-bg)';
              }}
              onMouseLeave={e => {
                const el = e.currentTarget as HTMLElement;
                el.style.color           = 'var(--color-text-muted)';
                el.style.backgroundColor = '';
              }}
            >
              {isDark
                ? <Sun  size={13} strokeWidth={1.5} />
                : <Moon size={13} strokeWidth={1.5} />
              }
              {isDark ? 'Claro' : 'Escuro'}
            </button>

            {/* Encerrar sessão */}
            <button
              type="button"
              onClick={logout}
              className="flex items-center gap-1.5 rounded-[var(--radius)] px-2 py-1.5 text-xs transition-colors"
              style={{ color: 'var(--color-text-muted)' }}
              onMouseEnter={e => {
                const el = e.currentTarget as HTMLElement;
                el.style.color           = 'var(--color-error)';
                el.style.backgroundColor = 'var(--color-error-bg)';
              }}
              onMouseLeave={e => {
                const el = e.currentTarget as HTMLElement;
                el.style.color           = 'var(--color-text-muted)';
                el.style.backgroundColor = '';
              }}
            >
              <LogOut size={12} strokeWidth={1.5} />
              Sair
            </button>

          </div>
        </div>
      </aside>

      {/* ═══════════════════════════════════════════════════════════
          Área de conteúdo principal
          ═══════════════════════════════════════════════════════════ */}
      <main
        className="flex-1 overflow-auto p-7"
        style={{ background: 'var(--color-bg)' }}
      >
        <Outlet />
      </main>
    </div>
  );
}
