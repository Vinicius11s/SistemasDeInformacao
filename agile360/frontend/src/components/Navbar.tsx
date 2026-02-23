import { Link } from 'react-router-dom';

interface NavbarProps {
  isAuthenticated?: boolean;
  userName?: string;
  onLogout?: () => void;
}

export function Navbar({ isAuthenticated, userName, onLogout }: NavbarProps) {
  return (
    <header className="flex h-14 items-center justify-between border-b border-[var(--color-border)] bg-[var(--color-surface)] px-6">
      <Link to="/" className="text-xl font-bold text-[var(--color-text)] hover:text-[var(--color-primary)]">
        Agile360
      </Link>
      <nav className="flex items-center gap-4" aria-label="Navegação principal">
        {!isAuthenticated ? (
          <>
            <Link
              to="#beneficios"
              className="text-[var(--color-text-muted)] hover:text-[var(--color-text)]"
            >
              Benefícios
            </Link>
            <Link
              to="/login"
              className="min-h-[44px] flex items-center rounded-[var(--radius)] bg-[var(--color-primary)] px-4 py-2 text-white hover:bg-[var(--color-primary-hover)]"
            >
              Login
            </Link>
            <Link to="/register" className="text-[var(--color-primary)] hover:underline">
              Cadastrar
            </Link>
          </>
        ) : (
          <>
            <span className="text-sm text-[var(--color-text-muted)]">{userName}</span>
            <Link to="/app" className="text-[var(--color-text)] hover:text-[var(--color-primary)]">
              Dashboard
            </Link>
            <button
              type="button"
              onClick={onLogout}
              className="min-h-[44px] rounded-[var(--radius)] px-4 py-2 text-[var(--color-text-muted)] hover:bg-[var(--color-surface)] hover:text-[var(--color-text)]"
            >
              Sair
            </button>
          </>
        )}
      </nav>
    </header>
  );
}
