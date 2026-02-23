import { Button } from '../components/Button';

export function Processos() {
  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-[var(--color-text)]">Processos</h1>
        <Button variant="primary">+ Novo processo</Button>
      </div>
      <p className="text-[var(--color-text-muted)]">
        Listagem e formulário de processos (Epic 2).
      </p>
    </div>
  );
}
