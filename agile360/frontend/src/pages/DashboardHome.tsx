import { useAuth } from '../context/AuthContext';

export function DashboardHome() {
  const { state } = useAuth();

  return (
    <div>
      <h1 className="mb-6 text-2xl font-semibold text-[var(--color-text)]">
        Olá, {state.user?.nome ?? 'Advogado'}
      </h1>
      <p className="text-[var(--color-text-muted)]">
        Use o menu ao lado para acessar Clientes, Processos, Audiências e Prazos. Os CRUDs serão
        disponibilizados conforme a API (Epic 2).
      </p>
    </div>
  );
}
