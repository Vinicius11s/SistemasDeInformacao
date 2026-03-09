import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/** Dois modos de autenticação: TOTP padrão ou código de recuperação de emergência */
type Mode = 'totp' | 'recovery';

export function MfaChallenge() {
  const navigate = useNavigate();
  const location = useLocation();
  const { completeMfaChallenge, completeMfaChallengeWithRecovery } = useAuth();

  const state = location.state as { mfaTempToken?: string; returnUrl?: string } | null;
  const mfaTempToken = state?.mfaTempToken ?? '';
  const returnUrl    = state?.returnUrl ?? '/app';

  const [mode, setMode]       = useState<Mode>('totp');
  const [code, setCode]       = useState('');
  const [error, setError]     = useState('');
  const [loading, setLoading] = useState(false);

  // Navega de volta ao login se não houver token de MFA
  if (!mfaTempToken) {
    navigate('/login', { replace: true });
    return null;
  }

  function switchMode(m: Mode) {
    setMode(m);
    setCode('');
    setError('');
  }

  // ── Validações por modo ───────────────────────────────────────────────────

  function getInputError(): string | null {
    if (mode === 'totp') {
      return code.replace(/\s/g, '').length !== 6 ? 'O código deve ter 6 dígitos.' : null;
    }
    // Recovery code: XXXX-XXXX (8 chars + hífen) ou XXXXXXXX (sem hífen)
    const normalized = code.replace(/-/g, '').replace(/\s/g, '');
    return normalized.length !== 8 ? 'Código de recuperação inválido (deve ter 8 caracteres).' : null;
  }

  // ── Submit ────────────────────────────────────────────────────────────────

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');

    const inputErr = getInputError();
    if (inputErr) { setError(inputErr); return; }

    setLoading(true);
    const clean  = code.replace(/\s/g, '');
    const result = mode === 'totp'
      ? await completeMfaChallenge(mfaTempToken, clean)
      : await completeMfaChallengeWithRecovery(mfaTempToken, clean);
    setLoading(false);

    if (result.ok) {
      navigate(returnUrl, { replace: true });
    } else {
      setError(result.error ?? 'Código inválido. Tente novamente.');
      setCode('');
    }
  }

  // ── UI helpers ────────────────────────────────────────────────────────────

  const isTotp     = mode === 'totp';
  const codeLength = isTotp ? 6 : 9; // XXXX-XXXX = 9 chars com hífen
  const isValid    = getInputError() === null && code.length > 0;

  return (
    <div className="mx-auto flex max-w-sm flex-col items-center justify-center px-6 py-20">
      <div className="w-full rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-8 shadow-sm">

        {/* Ícone */}
        <div className="mb-5 flex justify-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-full bg-[var(--color-primary)]/10 text-3xl">
            {isTotp ? '🔐' : '🔑'}
          </div>
        </div>

        <h1 className="mb-1 text-center text-xl font-semibold text-[var(--color-text)]">
          Verificação em Duas Etapas
        </h1>

        {/* F7/F8 — Abas TOTP / Recovery */}
        <div className="mb-5 mt-4 flex rounded-lg border border-[var(--color-border)] bg-[var(--color-bg)] p-1">
          <button
            type="button"
            onClick={() => switchMode('totp')}
            className={`flex-1 rounded-md py-1.5 text-sm font-medium transition-colors ${
              isTotp
                ? 'bg-[var(--color-primary)] text-white shadow-sm'
                : 'text-[var(--color-text-muted)] hover:text-[var(--color-text)]'
            }`}
          >
            🔐 Autenticador
          </button>
          <button
            type="button"
            onClick={() => switchMode('recovery')}
            className={`flex-1 rounded-md py-1.5 text-sm font-medium transition-colors ${
              !isTotp
                ? 'bg-[var(--color-primary)] text-white shadow-sm'
                : 'text-[var(--color-text-muted)] hover:text-[var(--color-text)]'
            }`}
          >
            🔑 Código de emergência
          </button>
        </div>

        {/* Descrição contextual */}
        <p className="mb-4 text-center text-sm text-[var(--color-text-muted)]">
          {isTotp
            ? 'Abra o Google Authenticator e insira o código de 6 dígitos gerado para o Agile360.'
            : 'Insira um dos seus códigos de recuperação de emergência (formato XXXX-XXXX). Cada código só pode ser usado uma vez.'}
        </p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-[var(--color-text)]">
              {isTotp ? 'Código TOTP' : 'Código de recuperação'}
            </label>
            {isTotp ? (
              // Input TOTP: 6 dígitos, espaçado
              <input
                key="totp"
                type="text"
                inputMode="numeric"
                maxLength={6}
                autoFocus
                autoComplete="one-time-code"
                value={code}
                onChange={(e) => setCode(e.target.value.replace(/[^0-9]/g, '').slice(0, 6))}
                placeholder="000000"
                className="w-full rounded-lg border border-[var(--color-border)] bg-[var(--color-bg)] px-4 py-3 text-center text-2xl font-mono tracking-[0.5em] text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
              />
            ) : (
              // Input Recovery: XXXX-XXXX
              <input
                key="recovery"
                type="text"
                inputMode="text"
                maxLength={codeLength}
                autoFocus
                autoComplete="off"
                value={code}
                onChange={(e) => {
                  // Formata automaticamente XXXX-XXXX enquanto o usuário digita
                  const raw = e.target.value.replace(/[^A-Za-z0-9]/g, '').toUpperCase().slice(0, 8);
                  setCode(raw.length > 4 ? `${raw.slice(0, 4)}-${raw.slice(4)}` : raw);
                }}
                placeholder="XXXX-XXXX"
                className="w-full rounded-lg border border-[var(--color-border)] bg-[var(--color-bg)] px-4 py-3 text-center text-xl font-mono tracking-widest text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none uppercase"
              />
            )}
          </div>

          {error && (
            <p className="text-center text-sm text-red-500" role="alert">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={loading || !isValid}
            className="mt-1 w-full rounded-lg bg-[var(--color-primary)] py-2.5 text-sm font-medium text-white hover:opacity-90 disabled:opacity-50"
          >
            {loading ? 'Verificando...' : isTotp ? 'Confirmar' : 'Usar código de emergência'}
          </button>

          <button
            type="button"
            onClick={() => navigate('/login', { replace: true })}
            className="text-center text-sm text-[var(--color-text-muted)] hover:underline"
          >
            Voltar ao login
          </button>
        </form>

        {/* F8 — Aviso de conta bloqueada quando não tem mais recovery codes */}
        {!isTotp && (
          <p className="mt-4 text-center text-xs text-[var(--color-text-muted)]">
            Não tem mais códigos?{' '}
            <a href="mailto:suporte@agile360.com.br" className="text-[var(--color-primary)] hover:underline">
              Contacte o suporte
            </a>
          </p>
        )}
      </div>
    </div>
  );
}
