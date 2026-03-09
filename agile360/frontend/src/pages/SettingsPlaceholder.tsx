import { useLocation } from 'react-router-dom';

const labels: Record<string, string> = {
  notificacoes: 'Notificações',
  integracoes: 'Integrações',
};

export function SettingsPlaceholder() {
  const location = useLocation();
  const section = location.pathname.split('/').pop() ?? '';
  const title = labels[section] ?? 'Configuração';

  return (
    <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-8 text-center">
      <h2 className="text-lg font-semibold text-[var(--color-text)]">{title}</h2>
      <p className="mt-2 text-sm text-[var(--color-text-muted)]">
        Esta seção está em desenvolvimento e estará disponível em breve.
      </p>
    </div>
  );
}
