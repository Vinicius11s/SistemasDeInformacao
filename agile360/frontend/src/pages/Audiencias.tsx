import { Button } from '../components/Button';

export function Audiencias() {
  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-[var(--color-text)]">Audiências</h1>
        <Button variant="primary">+ Nova audiência</Button>
      </div>
      <p className="text-[var(--color-text-muted)]">
        Calendário ou lista de audiências (Epic 2).
      </p>
    </div>
  );
}
