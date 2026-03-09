import { useState, useEffect } from 'react';
import { Link, Outlet, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  FolderOpen,
  CalendarDays,
  Clock,
  LogOut,
  Sun,
  Moon,
  Settings,
  Menu,
  X,
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
  { to: '/app/configuracoes', label: 'Configurações', icon: Settings,   exact: false },
];

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

const MOBILE_BREAKPOINT = 768;

export function DashboardLayout() {
  const location           = useLocation();
  const { state, logout }  = useAuth();
  const { isDark, toggleTheme } = useTheme();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const mql = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT - 1}px)`);
    const handler = () => setIsMobile(mql.matches);
    handler();
    mql.addEventListener('change', handler);
    return () => mql.removeEventListener('change', handler);
  }, []);

  useEffect(() => {
    setMobileMenuOpen(false);
  }, [location.pathname]);

  const NavLinkContent = ({ to, label, icon: Icon, exact }: typeof navItems[0]) => {
    const isActive = exact ? location.pathname === to : location.pathname.startsWith(to);
    return (
      <Link
        to={to}
        onClick={() => setMobileMenuOpen(false)}
        className="flex min-h-[44px] min-w-[44px] items-center gap-2.5 rounded-[var(--radius)] pr-3 text-sm transition-colors no-underline touch-manipulation"
        style={isActive ? ACTIVE_STYLE : INACTIVE_STYLE}
        onMouseEnter={e => {
          if (!isActive) {
            (e.currentTarget as HTMLElement).style.backgroundColor = 'var(--color-surface-elevated)';
            (e.currentTarget as HTMLElement).style.color = 'var(--color-text)';
          }
        }}
        onMouseLeave={e => {
          if (!isActive) {
            (e.currentTarget as HTMLElement).style.backgroundColor = '';
            (e.currentTarget as HTMLElement).style.color = 'var(--color-text-secondary)';
          }
        }}
      >
        <Icon size={18} strokeWidth={isActive ? 2 : 1.5} style={{ flexShrink: 0 }} />
        <span className="truncate">{label}</span>
      </Link>
    );
  };

  return (
    <div className="flex min-h-screen flex-col md:flex-row">
      {/* ═══ Sidebar — visível apenas >= 768px ═══════════════════════════════ */}
      <aside
        className="hidden w-56 flex-col md:flex"
        style={{
          background:   'var(--color-surface-sidebar)',
          borderRight:  '1px solid var(--color-border)',
        }}
        aria-label="Navegação principal"
      >
        <div
          className="flex items-center px-4 py-[18px]"
          style={{ borderBottom: '1px solid var(--color-border)' }}
        >
          <Link
            to="/app"
            className="text-sm font-bold tracking-tight no-underline"
            style={{ color: 'var(--color-text-heading)', letterSpacing: '-0.02em' }}
          >
            Agile<span style={{ color: 'var(--color-primary)' }}>360</span>
          </Link>
        </div>

        <nav className="flex flex-1 flex-col px-2 py-3" style={{ gap: 2 }} aria-label="Menu do sistema">
          <p className="label-uppercase mb-1 px-2" style={{ color: 'var(--color-text-muted)' }}>Menu</p>
          {navItems.map(({ to, label, icon: Icon, exact }) => {
            const isActive = exact ? location.pathname === to : location.pathname.startsWith(to);
            return (
              <Link
                key={to}
                to={to}
                className="flex min-h-[44px] items-center gap-2.5 rounded-[var(--radius)] pr-3 text-sm transition-colors no-underline"
                style={isActive ? ACTIVE_STYLE : INACTIVE_STYLE}
                onMouseEnter={e => {
                  if (!isActive) {
                    const el = e.currentTarget as HTMLElement;
                    el.style.backgroundColor = 'var(--color-surface-elevated)';
                    el.style.color = 'var(--color-text)';
                  }
                }}
                onMouseLeave={e => {
                  if (!isActive) {
                    const el = e.currentTarget as HTMLElement;
                    el.style.backgroundColor = '';
                    el.style.color = 'var(--color-text-secondary)';
                  }
                }}
              >
                <Icon size={15} strokeWidth={isActive ? 2 : 1.5} style={{ flexShrink: 0 }} />
                {label}
              </Link>
            );
          })}
        </nav>

        <div className="px-3 py-4" style={{ borderTop: '1px solid var(--color-border)' }}>
          <div className="flex items-center gap-2.5">
            <span
              className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-xs font-bold"
              style={{ background: 'rgba(37,99,235,.15)', color: 'var(--color-primary)' }}
              aria-hidden="true"
            >
              {(state.user?.nome ?? state.user?.email ?? 'A').charAt(0).toUpperCase()}
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate text-xs font-semibold" style={{ color: 'var(--color-text-heading)' }}>
                {state.user?.nome ?? 'Usuário'}
              </p>
              <p className="truncate text-[10px]" style={{ color: 'var(--color-text-muted)' }}>
                {state.user?.email ?? ''}
              </p>
            </div>
          </div>
          <div className="mt-3 flex items-center justify-between gap-2">
            <button
              type="button"
              onClick={toggleTheme}
              title={isDark ? 'Ativar tema claro' : 'Ativar tema escuro'}
              className="flex min-h-[44px] min-w-[44px] items-center justify-center gap-1.5 rounded-[var(--radius)] px-2 py-1.5 text-xs transition-colors touch-manipulation"
              style={{ color: 'var(--color-text-muted)' }}
              onMouseEnter={e => {
                const el = e.currentTarget as HTMLElement;
                el.style.color = 'var(--color-primary)';
                el.style.backgroundColor = 'var(--color-nav-active-bg)';
              }}
              onMouseLeave={e => {
                const el = e.currentTarget as HTMLElement;
                el.style.color = 'var(--color-text-muted)';
                el.style.backgroundColor = '';
              }}
            >
              {isDark ? <Sun size={13} strokeWidth={1.5} /> : <Moon size={13} strokeWidth={1.5} />}
              {isDark ? 'Claro' : 'Escuro'}
            </button>
            <button
              type="button"
              onClick={logout}
              className="flex min-h-[44px] min-w-[44px] items-center justify-center gap-1.5 rounded-[var(--radius)] px-2 py-1.5 text-xs transition-colors touch-manipulation"
              style={{ color: 'var(--color-text-muted)' }}
              onMouseEnter={e => {
                const el = e.currentTarget as HTMLElement;
                el.style.color = 'var(--color-error)';
                el.style.backgroundColor = 'var(--color-error-bg)';
              }}
              onMouseLeave={e => {
                const el = e.currentTarget as HTMLElement;
                el.style.color = 'var(--color-text-muted)';
                el.style.backgroundColor = '';
              }}
            >
              <LogOut size={12} strokeWidth={1.5} />
              Sair
            </button>
          </div>
        </div>
      </aside>

      {/* ═══ Mobile: top bar (logo + hamburger) ═══════════════════════════════ */}
      {isMobile && (
        <header
          className="flex h-14 shrink-0 items-center justify-between gap-3 px-4 md:hidden"
          style={{
            background:   'var(--color-surface-sidebar)',
            borderBottom: '1px solid var(--color-border)',
          }}
        >
          <Link
            to="/app"
            className="no-underline"
            style={{ color: 'var(--color-text-heading)' }}
          >
            <span className="text-sm font-bold tracking-tight">
              Agile<span style={{ color: 'var(--color-primary)' }}>360</span>
            </span>
          </Link>
          <button
            type="button"
            onClick={() => setMobileMenuOpen(o => !o)}
            className="flex h-11 w-11 min-h-[44px] min-w-[44px] items-center justify-center rounded-[var(--radius)] touch-manipulation"
            style={{ color: 'var(--color-text)' }}
            aria-label={mobileMenuOpen ? 'Fechar menu' : 'Abrir menu'}
            aria-expanded={mobileMenuOpen}
          >
            {mobileMenuOpen ? <X size={22} /> : <Menu size={22} />}
          </button>
        </header>
      )}

      {/* ═══ Mobile: overlay + drawer quando menu aberto ═════════════════════ */}
      {isMobile && mobileMenuOpen && (
        <>
          <div
            className="fixed inset-0 z-40 bg-black/50 md:hidden"
            style={{ top: 56 }}
            onClick={() => setMobileMenuOpen(false)}
            aria-hidden="true"
          />
          <div
            className="fixed left-0 right-0 top-14 z-50 max-h-[calc(100vh-3.5rem)] overflow-y-auto md:hidden"
            style={{
              background: 'var(--color-surface-sidebar)',
              borderBottom: '1px solid var(--color-border)',
              boxShadow: '0 10px 25px rgba(0,0,0,.25)',
            }}
          >
            <nav className="flex flex-col px-2 py-4 gap-1" aria-label="Menu do sistema">
              {navItems.map(item => (
                <NavLinkContent key={item.to} {...item} />
              ))}
            </nav>
            <div className="border-t border-[var(--color-border)] px-3 py-4">
              <div className="flex items-center gap-2.5 mb-3">
                <span
                  className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-sm font-bold"
                  style={{ background: 'rgba(37,99,235,.15)', color: 'var(--color-primary)' }}
                >
                  {(state.user?.nome ?? state.user?.email ?? 'A').charAt(0).toUpperCase()}
                </span>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-semibold" style={{ color: 'var(--color-text-heading)' }}>
                    {state.user?.nome ?? 'Usuário'}
                  </p>
                  <p className="truncate text-xs" style={{ color: 'var(--color-text-muted)' }}>
                    {state.user?.email ?? ''}
                  </p>
                </div>
              </div>
              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={() => { toggleTheme(); setMobileMenuOpen(false); }}
                  className="flex min-h-[44px] flex-1 items-center justify-center gap-2 rounded-[var(--radius)] text-sm touch-manipulation"
                  style={{ color: 'var(--color-text-muted)', border: '1px solid var(--color-border)' }}
                >
                  {isDark ? <Sun size={16} /> : <Moon size={16} />}
                  {isDark ? 'Tema claro' : 'Tema escuro'}
                </button>
                <button
                  type="button"
                  onClick={() => { logout(); setMobileMenuOpen(false); }}
                  className="flex min-h-[44px] flex-1 items-center justify-center gap-2 rounded-[var(--radius)] text-sm touch-manipulation"
                  style={{ color: 'var(--color-error)' }}
                >
                  <LogOut size={16} />
                  Sair
                </button>
              </div>
            </div>
          </div>
        </>
      )}

      {/* ═══ Área de conteúdo + padding mobile-safe ═══════════════════════════ */}
      <main
        className="flex-1 overflow-auto p-4 pb-24 md:pb-7 md:p-7"
        style={{
          background: 'var(--color-bg)',
          paddingLeft:  'max(1rem, env(safe-area-inset-left))',
          paddingRight: 'max(1rem, env(safe-area-inset-right))',
          paddingBottom: 'max(6rem, env(safe-area-inset-bottom) + 5rem)',
        }}
      >
        <Outlet />
      </main>

      {/* ═══ Mobile: barra de navegação inferior (5 itens principais) ═════════ */}
      {isMobile && (
        <nav
          className="fixed bottom-0 left-0 right-0 z-30 flex items-center justify-around border-t md:hidden"
          style={{
            background:   'var(--color-surface-sidebar)',
            borderColor:  'var(--color-border)',
            paddingBottom: 'max(0.5rem, env(safe-area-inset-bottom))',
            minHeight:    56,
          }}
          aria-label="Navegação inferior"
        >
          {navItems.filter(n => n.to !== '/app/configuracoes').map(({ to, label, icon: Icon, exact }) => {
            const isActive = exact ? location.pathname === to : location.pathname.startsWith(to);
            return (
              <Link
                key={to}
                to={to}
                className="flex min-h-[44px] min-w-[44px] flex-col items-center justify-center gap-0.5 rounded-[var(--radius)] px-2 py-2 text-[10px] no-underline touch-manipulation"
                style={{
                  color: isActive ? 'var(--color-primary)' : 'var(--color-text-muted)',
                  fontWeight: isActive ? 600 : 400,
                }}
              >
                <Icon size={20} strokeWidth={isActive ? 2 : 1.5} />
                <span className="truncate max-w-[64px]">{label}</span>
              </Link>
            );
          })}
        </nav>
      )}
    </div>
  );
}
