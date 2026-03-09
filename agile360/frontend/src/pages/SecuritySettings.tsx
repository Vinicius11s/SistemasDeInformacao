import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { getMfaStatus, beginMfaSetup, verifyMfaSetup, disableMfa } from '../api/mfa';

type View = 'status' | 'setup-qr' | 'setup-verify' | 'disable';

export function SecuritySettings() {
  const { state } = useAuth();
  const token = state.token ?? '';

  const [mfaEnabled, setMfaEnabled] = useState<boolean | null>(null);
  const [view, setView] = useState<View>('status');
  const [qrUrl, setQrUrl] = useState('');
  const [manualKey, setManualKey] = useState('');
  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!token) return;
    getMfaStatus(token).then((res) => {
      if (res.success && res.data) setMfaEnabled(res.data.mfaEnabled);
    });
  }, [token]);

  async function handleBeginSetup() {
    setError('');
    setLoading(true);
    const res = await beginMfaSetup(token);
    setLoading(false);
    if (!res.success) {
      setError(res.error?.message ?? 'Erro ao iniciar configuração.');
      return;
    }
    setQrUrl(res.data!.qrCodeUrl);
    setManualKey(res.data!.manualEntryKey);
    setView('setup-qr');
  }

  async function handleVerifySetup() {
    setError('');
    if (code.length !== 6) { setError('Digite os 6 dígitos do Google Authenticator.'); return; }
    setLoading(true);
    const res = await verifyMfaSetup(code, token);
    setLoading(false);
    if (!res.success) {
      setError(res.error?.message ?? 'Código inválido. Tente novamente.');
      setCode('');
      return;
    }
    setMfaEnabled(true);
    setSuccess('MFA ativado com sucesso! Seu Google Authenticator está vinculado.');
    setView('status');
    setCode('');
  }

  async function handleDisable() {
    setError('');
    if (code.length !== 6) { setError('Digite os 6 dígitos do Google Authenticator.'); return; }
    setLoading(true);
    const res = await disableMfa(code, token);
    setLoading(false);
    if (!res.success) {
      setError(res.error?.message ?? 'Código inválido.');
      setCode('');
      return;
    }
    setMfaEnabled(false);
    setSuccess('MFA desativado.');
    setView('status');
    setCode('');
  }

  if (mfaEnabled === null) {
    return <p className="text-[var(--color-text-muted)]">Carregando configurações de segurança...</p>;
  }

  return (
    <div className="max-w-lg">
      <h2 className="mb-1 text-lg font-semibold text-[var(--color-text)]">Segurança e Privacidade</h2>
      <p className="mb-6 text-sm text-[var(--color-text-muted)]">
        Proteja sua conta com autenticação em duas etapas (2FA) via Google Authenticator e gerencie suas sessões.
      </p>

      {success && (
        <div className="mb-4 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
          {success}
        </div>
      )}

      {/* ── Status view ─────────────────────────────────────────────────── */}
      {view === 'status' && (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-6">
          <div className="mb-4 flex items-center justify-between">
            <div>
              <p className="font-medium text-[var(--color-text)]">Autenticação em duas etapas (2FA)</p>
              <p className="text-sm text-[var(--color-text-muted)]">
                {mfaEnabled
                  ? 'Ativo — seu login exige um código do Google Authenticator.'
                  : 'Inativo — habilite para uma camada extra de segurança.'}
              </p>
            </div>
            <span className={`rounded-full px-3 py-1 text-xs font-medium ${
              mfaEnabled ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'
            }`}>
              {mfaEnabled ? 'Ativo' : 'Inativo'}
            </span>
          </div>

          {mfaEnabled ? (
            <button
              onClick={() => { setView('disable'); setError(''); setCode(''); }}
              className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50"
            >
              Desativar 2FA
            </button>
          ) : (
            <button
              onClick={handleBeginSetup}
              disabled={loading}
              className="rounded-lg bg-[var(--color-primary)] px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-50"
            >
              {loading ? 'Aguarde...' : 'Ativar Google Authenticator'}
            </button>
          )}
        </div>
      )}

      {/* ── QR Code view ─────────────────────────────────────────────────── */}
      {view === 'setup-qr' && (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-6">
          <h2 className="mb-3 font-semibold text-[var(--color-text)]">Passo 1 — Escanear o QR Code</h2>
          <p className="mb-4 text-sm text-[var(--color-text-muted)]">
            Abra o <strong>Google Authenticator</strong> no seu celular, toque em&nbsp;
            <strong>"+"</strong> e escolha <strong>"Escanear QR code"</strong>.
          </p>

          {/* QR Code via Google Charts API — no extra dependency needed */}
          <div className="mb-4 flex justify-center">
            <img
              src={`https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(qrUrl)}`}
              alt="QR Code para Google Authenticator"
              className="rounded-lg border border-[var(--color-border)]"
              width={200}
              height={200}
            />
          </div>

          <details className="mb-4">
            <summary className="cursor-pointer text-sm text-[var(--color-text-muted)] underline">
              Não consigo escanear — inserir manualmente
            </summary>
            <p className="mt-2 rounded bg-gray-50 p-3 font-mono text-xs tracking-widest text-gray-700 break-all">
              {manualKey}
            </p>
          </details>

          <div className="flex gap-3">
            <button
              onClick={() => setView('setup-verify')}
              className="rounded-lg bg-[var(--color-primary)] px-4 py-2 text-sm font-medium text-white hover:opacity-90"
            >
              Já escaneei — Próximo
            </button>
            <button
              onClick={() => setView('status')}
              className="rounded-lg border border-[var(--color-border)] px-4 py-2 text-sm text-[var(--color-text-muted)] hover:bg-gray-50"
            >
              Cancelar
            </button>
          </div>
        </div>
      )}

      {/* ── Verify setup view ─────────────────────────────────────────────── */}
      {view === 'setup-verify' && (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-6">
          <h2 className="mb-3 font-semibold text-[var(--color-text)]">Passo 2 — Confirmar o código</h2>
          <p className="mb-4 text-sm text-[var(--color-text-muted)]">
            Insira o código de 6 dígitos que o Google Authenticator está exibindo agora para confirmar a vinculação.
          </p>

          <input
            type="text"
            inputMode="numeric"
            maxLength={6}
            autoFocus
            value={code}
            onChange={(e) => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
            placeholder="000000"
            className="mb-3 w-full rounded-lg border border-[var(--color-border)] bg-[var(--color-bg)] px-4 py-3 text-center text-2xl font-mono tracking-[0.5em] text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
          />

          {error && <p className="mb-3 text-sm text-red-500">{error}</p>}

          <div className="flex gap-3">
            <button
              onClick={handleVerifySetup}
              disabled={loading || code.length !== 6}
              className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
            >
              {loading ? 'Verificando...' : 'Ativar 2FA'}
            </button>
            <button
              onClick={() => { setView('setup-qr'); setCode(''); setError(''); }}
              className="rounded-lg border border-[var(--color-border)] px-4 py-2 text-sm text-[var(--color-text-muted)] hover:bg-gray-50"
            >
              Voltar
            </button>
          </div>
        </div>
      )}

      {/* ── Disable view ─────────────────────────────────────────────────── */}
      {view === 'disable' && (
        <div className="rounded-xl border border-red-200 bg-red-50 p-6">
          <h2 className="mb-3 font-semibold text-red-800">Desativar autenticação em duas etapas</h2>
          <p className="mb-4 text-sm text-red-700">
            Insira o código atual do Google Authenticator para confirmar a desativação.
            Após isso, seu login não exigirá mais o código.
          </p>

          <input
            type="text"
            inputMode="numeric"
            maxLength={6}
            autoFocus
            value={code}
            onChange={(e) => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
            placeholder="000000"
            className="mb-3 w-full rounded-lg border border-red-300 bg-white px-4 py-3 text-center text-2xl font-mono tracking-[0.5em] text-gray-900 focus:border-red-500 focus:outline-none"
          />

          {error && <p className="mb-3 text-sm text-red-600">{error}</p>}

          <div className="flex gap-3">
            <button
              onClick={handleDisable}
              disabled={loading || code.length !== 6}
              className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
            >
              {loading ? 'Desativando...' : 'Confirmar Desativação'}
            </button>
            <button
              onClick={() => { setView('status'); setCode(''); setError(''); }}
              className="rounded-lg border border-red-300 bg-white px-4 py-2 text-sm text-red-700 hover:bg-red-50"
            >
              Cancelar
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
