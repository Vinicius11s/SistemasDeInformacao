import { Link } from 'react-router-dom';
import { Scale } from 'lucide-react';

interface NavbarProps {
  isAuthenticated?: boolean;
  userName?: string;
  onLogout?: () => void;
}

export function Navbar({ isAuthenticated, userName, onLogout }: NavbarProps) {
  return (
    <header
      className="flex h-14 items-center justify-between border-b px-6"
      style={{
        borderColor:     'var(--lp-border, var(--color-border))',
        backgroundColor: 'var(--lp-bg, var(--color-surface))',
      }}
    >
      {/* Logotipo */}
      <Link
        to="/"
        className="flex items-center gap-2 no-underline"
        aria-label="Ir para a página inicial do Agile360"
      >
        <span
          className="flex h-7 w-7 items-center justify-center rounded"
          style={{ background: '#D95F00', color: '#fff', borderRadius: 4 }}
          aria-hidden="true"
        >
          <Scale size={14} strokeWidth={2.5} />
        </span>
        <span
          className="text-base font-bold"
          style={{ color: 'var(--lp-text-1, var(--color-text-heading))' }}
        >
          Agile<span style={{ color: '#D95F00' }}>360</span>
        </span>
      </Link>

      {/* Navegação */}
      <nav className="flex items-center gap-4" aria-label="Navegação principal">
        {!isAuthenticated ? (
          <>
            <a
              href="#funcionalidades"
              className="text-sm"
              style={{ color: 'var(--lp-text-4, var(--color-text-muted))' }}
              onMouseEnter={e => (e.currentTarget.style.color = 'var(--lp-text-2, var(--color-text))')}
              onMouseLeave={e => (e.currentTarget.style.color = 'var(--lp-text-4, var(--color-text-muted))')}
            >
              Funcionalidades
            </a>

            <Link
              to="/login"
              className="flex h-9 items-center rounded px-4 text-sm font-semibold text-white no-underline transition-colors"
              style={{ background: '#D95F00', borderRadius: 4 }}
              onMouseEnter={e => ((e.currentTarget as HTMLElement).style.background = '#F07010')}
              onMouseLeave={e => ((e.currentTarget as HTMLElement).style.background = '#D95F00')}
            >
              Entrar
            </Link>

            <Link
              to="/register"
              className="text-sm font-medium no-underline transition-colors"
              style={{ color: '#D95F00' }}
              onMouseEnter={e => (e.currentTarget.style.color = '#F07010')}
              onMouseLeave={e => (e.currentTarget.style.color = '#D95F00')}
            >
              Solicitar Acesso
            </Link>
          </>
        ) : (
          <>
            <span
              className="text-sm"
              style={{ color: 'var(--color-text-muted)' }}
            >
              {userName}
            </span>

            <Link
              to="/app"
              className="text-sm no-underline transition-colors"
              style={{ color: 'var(--color-text)' }}
            >
              Dashboard
            </Link>

            <button
              type="button"
              onClick={onLogout}
              className="h-9 rounded px-3 text-sm transition-colors"
              style={{
                color:        'var(--color-text-muted)',
                background:   'transparent',
                border:       '1px solid var(--color-border)',
                borderRadius: 4,
                cursor:       'pointer',
              }}
              onMouseEnter={e => {
                (e.currentTarget as HTMLElement).style.color = '#f87171';
                (e.currentTarget as HTMLElement).style.borderColor = 'rgba(248,113,113,.3)';
              }}
              onMouseLeave={e => {
                (e.currentTarget as HTMLElement).style.color = 'var(--color-text-muted)';
                (e.currentTarget as HTMLElement).style.borderColor = 'var(--color-border)';
              }}
            >
              Sair
            </button>
          </>
        )}
      </nav>
    </header>
  );
}
