import { Link, Outlet, useLocation } from 'react-router-dom';

const settingsNavItems = [
  { to: '/app/configuracoes/minha-conta', label: 'Minha Conta', description: 'Perfil, e-mail e dados profissionais' },
  { to: '/app/configuracoes/seguranca', label: 'Segurança e Privacidade', description: '2FA, senha e sessões' },
  { to: '/app/configuracoes/notificacoes', label: 'Notificações', description: 'Alertas e preferências de e-mail' },
  { to: '/app/configuracoes/integracoes', label: 'Integrações', description: 'WhatsApp, Google Agenda e APIs' },
];

export function SettingsLayout() {
  const location = useLocation();

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-[var(--color-text)]">Configurações</h1>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Gerencie sua conta, segurança e preferências do Agile360.
        </p>
      </div>

      <div className="flex flex-col gap-4 lg:flex-row lg:gap-8">
        <nav
          className="shrink-0 lg:w-72"
          aria-label="Menu de configurações"
        >
          <ul className="space-y-1 rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-2">
            {settingsNavItems.map(({ to, label, description }) => {
              const isActive =
                location.pathname === to ||
                (to !== '/app/configuracoes/minha-conta' && location.pathname.startsWith(to));
              return (
                <li key={to}>
                  <Link
                    to={to}
                    className={`block rounded-lg px-4 py-3 transition-colors ${
                      isActive
                        ? 'bg-[var(--color-primary)]/10 text-[var(--color-primary)]'
                        : 'text-[var(--color-text)] hover:bg-[var(--color-surface-sidebar)]'
                    }`}
                  >
                    <span className="block font-medium">{label}</span>
                    <span className="mt-0.5 block text-xs text-[var(--color-text-muted)]">
                      {description}
                    </span>
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>

        <div className="min-w-0 flex-1">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
