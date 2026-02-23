import { Button } from '../components/Button';

export function Clientes() {
  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-[var(--color-text)]">Clientes</h1>
        <Button variant="primary">+ Novo cliente</Button>
      </div>
      <p className="text-[var(--color-text-muted)]">
        Listagem e formulário de clientes serão implementados quando o endpoint da API estiver
        disponível (Epic 2).
      </p>
    </div>
  );
}
