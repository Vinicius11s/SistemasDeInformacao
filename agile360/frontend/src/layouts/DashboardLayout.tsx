import { Link, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const navItems = [
  { to: '/app', label: 'Início' },
  { to: '/app/clientes', label: 'Clientes' },
  { to: '/app/processos', label: 'Processos' },
  { to: '/app/audiencias', label: 'Audiências' },
  { to: '/app/prazos', label: 'Prazos' },
];

export function DashboardLayout() {
  const location = useLocation();
  const { state, logout } = useAuth();

  return (
    <div className="flex min-h-screen">
      <aside
        className="flex w-56 flex-col border-r border-[var(--color-border)] bg-[var(--color-surface-sidebar)]"
        aria-label="Menu do dashboard"
      >
        <div className="p-4">
          <Link to="/app" className="text-lg font-bold text-[var(--color-text)]">
            Agile360
          </Link>
        </div>
        <nav className="flex flex-1 flex-col gap-1 px-2 py-4">
          {navItems.map(({ to, label }) => {
            const isActive = location.pathname === to || (to !== '/app' && location.pathname.startsWith(to));
            return (
              <Link
                key={to}
                to={to}
                className={`min-h-[44px] flex items-center rounded-[var(--radius)] px-4 py-2 ${
                  isActive
                    ? 'bg-[var(--color-surface)] text-[var(--color-primary)]'
                    : 'text-[var(--color-text)] hover:bg-[var(--color-surface)] hover:text-[var(--color-text)]'
                }`}
              >
                {label}
              </Link>
            );
          })}
        </nav>
        <div className="border-t border-[var(--color-border)] p-4">
          <span className="block truncate text-sm text-[var(--color-text-muted)]">
            {state.user?.nome ?? state.user?.email}
          </span>
          <button
            type="button"
            onClick={logout}
            className="mt-2 text-sm text-[var(--color-primary)] hover:underline"
          >
            Sair
          </button>
        </div>
      </aside>
      <main className="flex-1 overflow-auto bg-[var(--color-bg)] p-6">
        <Outlet />
      </main>
    </div>
  );
}
