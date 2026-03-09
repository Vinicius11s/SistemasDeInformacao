import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export function MfaChallenge() {
  const navigate = useNavigate();
  const location = useLocation();
  const { completeMfaChallenge } = useAuth();

  const state = location.state as { mfaTempToken?: string; returnUrl?: string } | null;
  const mfaTempToken = state?.mfaTempToken ?? '';
  const returnUrl = state?.returnUrl ?? '/app';

  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  // If someone navigates here directly without a token, send them back to login
  if (!mfaTempToken) {
    navigate('/login', { replace: true });
    return null;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');

    if (code.replace(/\s/g, '').length !== 6) {
      setError('O código deve ter 6 dígitos.');
      return;
    }

    setLoading(true);
    const result = await completeMfaChallenge(mfaTempToken, code.replace(/\s/g, ''));
    setLoading(false);

    if (result.ok) {
      navigate(returnUrl, { replace: true });
    } else {
      setError(result.error ?? 'Código inválido. Tente novamente.');
      setCode('');
    }
  }

  return (
    <div className="mx-auto flex max-w-sm flex-col items-center justify-center px-6 py-20">
      <div className="w-full rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-8 shadow-sm">
        {/* Icon */}
        <div className="mb-5 flex justify-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-full bg-[var(--color-primary)]/10 text-3xl">
            🔐
          </div>
        </div>

        <h1 className="mb-1 text-center text-xl font-semibold text-[var(--color-text)]">
          Verificação em Duas Etapas
        </h1>
        <p className="mb-6 text-center text-sm text-[var(--color-text-muted)]">
          Abra o Google Authenticator e insira o código de 6 dígitos gerado para o Agile360.
        </p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-[var(--color-text)]">
              Código TOTP
            </label>
            <input
              type="text"
              inputMode="numeric"
              pattern="[0-9 ]{6,7}"
              maxLength={7}
              autoFocus
              autoComplete="one-time-code"
              value={code}
              onChange={(e) => setCode(e.target.value.replace(/[^0-9]/g, '').slice(0, 6))}
              placeholder="000000"
              className="w-full rounded-lg border border-[var(--color-border)] bg-[var(--color-bg)] px-4 py-3 text-center text-2xl font-mono tracking-[0.5em] text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
            />
          </div>

          {error && (
            <p className="text-center text-sm text-red-500" role="alert">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={loading || code.length !== 6}
            className="mt-1 w-full rounded-lg bg-[var(--color-primary)] py-2.5 text-sm font-medium text-white hover:opacity-90 disabled:opacity-50"
          >
            {loading ? 'Verificando...' : 'Confirmar'}
          </button>

          <button
            type="button"
            onClick={() => navigate('/login', { replace: true })}
            className="text-center text-sm text-[var(--color-text-muted)] hover:underline"
          >
            Voltar ao login
          </button>
        </form>
      </div>
    </div>
  );
}
