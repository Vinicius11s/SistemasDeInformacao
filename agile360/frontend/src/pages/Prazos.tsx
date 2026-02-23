import { Button } from '../components/Button';

export function Prazos() {
  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-[var(--color-text)]">Prazos</h1>
        <Button variant="primary">+ Novo prazo</Button>
      </div>
      <p className="text-[var(--color-text-muted)]">
        Lista de prazos com filtros e alertas (Epic 2).
      </p>
    </div>
  );
}
