import { useAuth } from '../context/AuthContext';

export function MinhaConta() {
  const { state } = useAuth();
  const user = state.user;

  return (
    <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-6">
      <h2 className="mb-1 text-lg font-semibold text-[var(--color-text)]">Minha Conta</h2>
      <p className="mb-6 text-sm text-[var(--color-text-muted)]">
        Seus dados profissionais e de acesso ao Agile360.
      </p>

      <dl className="space-y-5">
        <div>
          <dt className="text-sm font-medium text-[var(--color-text-muted)]">Nome</dt>
          <dd className="mt-1 text-[var(--color-text)]">{user?.nome ?? '—'}</dd>
        </div>
        <div>
          <dt className="text-sm font-medium text-[var(--color-text-muted)]">E-mail</dt>
          <dd className="mt-1 text-[var(--color-text)]">{user?.email ?? '—'}</dd>
        </div>
        <div>
          <dt className="text-sm font-medium text-[var(--color-text-muted)]">OAB</dt>
          <dd className="mt-1 text-[var(--color-text)]">{user?.oab ?? '—'}</dd>
        </div>
        <div>
          <dt className="text-sm font-medium text-[var(--color-text-muted)]">Telefone</dt>
          <dd className="mt-1 text-[var(--color-text)]">{user?.telefone ?? 'Não informado'}</dd>
        </div>
      </dl>

      <p className="mt-6 text-sm text-[var(--color-text-muted)]">
        A alteração de dados do perfil estará disponível em breve. Entre em contato com o suporte se precisar atualizar seu e-mail ou OAB.
      </p>
    </div>
  );
}
