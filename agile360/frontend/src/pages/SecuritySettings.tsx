import { useState, useEffect } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { useAuth } from '../context/AuthContext';
import {
  getMfaStatus,
  beginMfaSetup,
  verifyMfaSetup,
  disableMfa,
  getRecoveryCodesCount,
  regenerateRecoveryCodes,
} from '../api/mfa';

// ── Tipos de view ──────────────────────────────────────────────────────────────

type View = 'status' | 'setup-qr' | 'setup-verify' | 'setup-backup' | 'disable' | 'regen-confirm';

// ── F3 — SetupStepper: 3 passos ───────────────────────────────────────────────

/** Indicador visual dos 3 passos do fluxo de ativação 2FA */
function SetupStepper({ step }: { step: 1 | 2 | 3 }) {
  const steps = [
    { n: 1, label: 'Escanear QR Code' },
    { n: 2, label: 'Confirmar código' },
    { n: 3, label: 'Backup de Emergência' },
  ] as const;

  return (
    <div className="mb-6 flex items-center gap-1 text-xs">
      {steps.map(({ n, label }, idx) => (
        <div key={n} className="flex items-center gap-1 min-w-0">
          {idx > 0 && (
            <span className={`flex-1 h-px w-6 border-t ${step > idx ? 'border-[var(--color-primary)]' : 'border-[var(--color-border)]'}`} />
          )}
          <span className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-xs font-bold transition-colors ${
            step > n
              ? 'bg-green-500 text-white'
              : step === n
              ? 'bg-[var(--color-primary)] text-white'
              : 'bg-gray-100 text-gray-400'
          }`}>{step > n ? '✓' : n}</span>
          <span className={`hidden sm:inline whitespace-nowrap font-medium ${step >= n ? 'text-[var(--color-text)]' : 'text-[var(--color-text-muted)]'}`}>
            {label}
          </span>
        </div>
      ))}
    </div>
  );
}

// ── F6 / F6a — Componente de badge de Recovery Codes na view de status ────────

function RecoveryCodesBadge({ token, onRegen }: { token: string; onRegen: () => void }) {
  const [remaining, setRemaining] = useState<number | null>(null);
  const [loading] = useState(false);

  useEffect(() => {
    if (!token) return;
    getRecoveryCodesCount(token).then((res) => {
      if (res.success && res.data != null) setRemaining(res.data.remaining);
    });
  }, [token]);

  if (remaining === null) return null;

  // F6a: alerta visual quando ≤ 2 códigos restantes
  const isCritical = remaining <= 2;
  const isLow      = remaining <= 4 && remaining > 2;

  return (
    <div className={`mt-4 rounded-lg border p-4 ${
      isCritical
        ? 'border-red-300 bg-red-50'
        : isLow
        ? 'border-amber-200 bg-amber-50'
        : 'border-[var(--color-border)] bg-[var(--color-bg)]'
    }`}>
      <div className="flex items-center justify-between gap-3 flex-wrap">
        <div>
          <p className={`text-sm font-medium ${isCritical ? 'text-red-800' : isLow ? 'text-amber-800' : 'text-[var(--color-text)]'}`}>
            🔑 Códigos de recuperação de emergência
          </p>

          {/* F6a — alerta ≤ 2 */}
          {isCritical ? (
            <p className="mt-0.5 text-sm text-red-700">
              ⚠️ <strong>Atenção: restam apenas {remaining} código{remaining !== 1 ? 's' : ''}!</strong>{' '}
              Gere novos agora para não perder o acesso à conta.
            </p>
          ) : isLow ? (
            <p className="mt-0.5 text-sm text-amber-700">
              ⚠️ Restam apenas <strong>{remaining}</strong> de 10 códigos. Considere gerar novos em breve.
            </p>
          ) : (
            <p className="mt-0.5 text-sm text-[var(--color-text-muted)]">
              {remaining === 0 ? (
                <span className="text-red-600 font-medium">Nenhum código disponível — gere novos agora.</span>
              ) : (
                <>{remaining} de 10 códigos restantes</>
              )}
            </p>
          )}
        </div>

        <button
          onClick={onRegen}
          disabled={loading}
          className={`shrink-0 rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors ${
            isCritical
              ? 'border-red-400 bg-red-600 text-white hover:bg-red-700'
              : 'border-[var(--color-border)] bg-white text-[var(--color-text-muted)] hover:bg-gray-50'
          }`}
        >
          {loading ? 'Gerando...' : 'Gerar novos códigos'}
        </button>
      </div>
    </div>
  );
}

// ── Componente principal ──────────────────────────────────────────────────────

export function SecuritySettings() {
  const { state } = useAuth();
  const token = state.token ?? '';

  const [mfaEnabled, setMfaEnabled]   = useState<boolean | null>(null);
  const [view, setView]               = useState<View>('status');
  const [qrUrl, setQrUrl]             = useState('');
  const [manualKey, setManualKey]     = useState('');
  const [code, setCode]               = useState('');
  const [error, setError]             = useState('');
  const [success, setSuccess]         = useState('');
  const [loading, setLoading]         = useState(false);
  const [copied, setCopied]           = useState(false);

  // F4 — Passo 3: backup codes
  const [backupCodes, setBackupCodes]         = useState<string[]>([]);
  const [backupConfirmed, setBackupConfirmed] = useState(false);
  const [copiedAll, setCopiedAll]             = useState(false);
  // Controla re-render do badge após regeneração
  const [codesVersion, setCodesVersion]       = useState(0);

  function resetSetupFlow() {
    setCode('');
    setError('');
    setQrUrl('');
    setManualKey('');
    setBackupCodes([]);
    setBackupConfirmed(false);
  }

  useEffect(() => {
    if (!token) return;
    getMfaStatus(token).then((res) => {
      if (res.success && res.data) setMfaEnabled(res.data.mfaEnabled);
    });
  }, [token]);

  // ── Handlers ──────────────────────────────────────────────────────────────

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

  // F5 — Avança para 'setup-backup' (não mais 'status') após verificação TOTP
  async function handleVerifySetup() {
    setError('');
    if (code.length !== 6) {
      setError('Digite os 6 dígitos do Google Authenticator.');
      return;
    }
    setLoading(true);
    const res = await verifyMfaSetup(code, token);
    setLoading(false);
    if (!res.success) {
      setError(res.error?.message ?? 'Código inválido. Tente novamente.');
      setCode('');
      return;
    }
    // F3/F4: armazena os códigos e avança para o Passo 3
    setBackupCodes(res.data!.recoveryCodes);
    setBackupConfirmed(false);
    setMfaEnabled(true);
    setCode('');
    setError('');
    setView('setup-backup');
  }

  // F4 — Conclusão do Passo 3: volta ao status
  function handleBackupDone() {
    setSuccess('🔒 2FA ativado com sucesso! Guarde seus códigos de recuperação em local seguro.');
    setCodesVersion((v) => v + 1);
    resetSetupFlow();
    setView('status');
  }

  async function handleDisable() {
    setError('');
    if (code.length !== 6) {
      setError('Digite os 6 dígitos do Google Authenticator.');
      return;
    }
    setLoading(true);
    const res = await disableMfa(code, token);
    setLoading(false);
    if (!res.success) {
      setError(res.error?.message ?? 'Código inválido.');
      setCode('');
      return;
    }
    setMfaEnabled(false);
    setSuccess('MFA desativado. Seus códigos de recuperação foram removidos.');
    setCodesVersion((v) => v + 1);
    setView('status');
    setCode('');
  }

  // F4 — Baixar codes como .txt
  function handleDownloadTxt() {
    const now = new Date();
    const dateStr = now.toLocaleDateString('pt-BR') + ' às ' + now.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }) + ' UTC';
    const lines = [
      'Agile360 — Códigos de Recuperação de Emergência',
      '================================================',
      `Gerados em: ${dateStr}`,
      `Conta: ${state.user?.email ?? ''}`,
      '',
      'IMPORTANTE: Guarde este arquivo em local seguro.',
      'Cada código pode ser usado UMA única vez.',
      'Após o uso, o código é invalidado automaticamente.',
      '',
      ...backupCodes,
    ];
    const blob = new Blob([lines.join('\n')], { type: 'text/plain;charset=utf-8' });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href     = url;
    a.download = 'agile360-codigos-recuperacao.txt';
    a.click();
    URL.revokeObjectURL(url);
  }

  // F4 — Copiar todos os códigos
  function handleCopyAll() {
    navigator.clipboard.writeText(backupCodes.join('\n')).then(() => {
      setCopiedAll(true);
      setTimeout(() => setCopiedAll(false), 2500);
    });
  }

  function handleCopyKey() {
    navigator.clipboard.writeText(manualKey).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  }

  // Regeneração de códigos (botão no status view)
  async function handleRegenRequest() {
    setView('regen-confirm');
    setError('');
  }

  async function handleRegenConfirm() {
    setLoading(true);
    const res = await regenerateRecoveryCodes(token);
    setLoading(false);
    if (!res.success) {
      setError(res.error?.message ?? 'Erro ao gerar novos códigos.');
      return;
    }
    setBackupCodes(res.data!.codes);
    setBackupConfirmed(false);
    setCopiedAll(false);
    setView('setup-backup');
  }

  // ── Loading inicial ───────────────────────────────────────────────────────

  if (mfaEnabled === null) {
    return <p className="text-[var(--color-text-muted)]">Carregando configurações de segurança...</p>;
  }

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div className="max-w-lg">
      <h2 className="mb-1 text-lg font-semibold text-[var(--color-text)]">Segurança e Privacidade</h2>
      <p className="mb-6 text-sm text-[var(--color-text-muted)]">
        Proteja sua conta com autenticação em duas etapas (2FA) via Google Authenticator e gerencie seus códigos de emergência.
      </p>

      {success && (
        <div className="mb-4 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
          {success}
        </div>
      )}

      {/* ── F6 / F6a — Status view ─────────────────────────────────────────── */}
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
            <>
              {/* F6 — badge de códigos restantes (com alerta F6a quando ≤ 2) */}
              <RecoveryCodesBadge
                key={codesVersion}
                token={token}
                onRegen={handleRegenRequest}
              />
              <div className="mt-4">
                <button
                  onClick={() => { setView('disable'); setError(''); setCode(''); }}
                  className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50"
                >
                  Desativar 2FA
                </button>
              </div>
            </>
          ) : (
            <button
              onClick={handleBeginSetup}
              disabled={loading}
              className="mt-2 rounded-lg bg-[var(--color-primary)] px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-50"
            >
              {loading ? 'Aguarde...' : 'Ativar Google Authenticator'}
            </button>
          )}
        </div>
      )}

      {/* ── QR Code view (Passo 1) ─────────────────────────────────────────── */}
      {view === 'setup-qr' && (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-6">
          <SetupStepper step={1} />

          <h2 className="mb-1 font-semibold text-[var(--color-text)]">Passo 1 — Escanear o QR Code</h2>
          <p className="mb-5 text-sm text-[var(--color-text-muted)]">
            Abra o <strong>Google Authenticator</strong>, toque em <strong>"+"</strong> e
            escolha <strong>"Escanear QR code"</strong>.
          </p>

          <div className="mb-5 flex flex-col items-center gap-3">
            <div className="rounded-xl border-2 border-[var(--color-border)] bg-white p-4 shadow-sm">
              <QRCodeSVG value={qrUrl} size={192} level="M" includeMargin={false} />
            </div>
            <p className="text-xs text-[var(--color-text-muted)]">
              🔒 QR Code gerado localmente — sua chave nunca sai do dispositivo
            </p>
          </div>

          <details className="mb-5 rounded-lg border border-[var(--color-border)] bg-[var(--color-bg)]">
            <summary className="cursor-pointer select-none px-4 py-3 text-sm font-medium text-[var(--color-text-muted)]">
              📵 Não consigo escanear — inserir chave manualmente
            </summary>
            <div className="border-t border-[var(--color-border)] px-4 py-3">
              <p className="mb-2 text-xs text-[var(--color-text-muted)]">
                No Google Authenticator, escolha <strong>"Inserir chave"</strong> e cole:
              </p>
              <div className="flex items-center gap-2">
                <code className="flex-1 rounded bg-gray-100 px-3 py-2 font-mono text-xs tracking-widest text-gray-800 break-all select-all">
                  {manualKey}
                </code>
                <button type="button" onClick={handleCopyKey}
                  className="shrink-0 rounded-lg border border-[var(--color-border)] bg-white px-3 py-2 text-xs font-medium text-[var(--color-text-muted)] hover:bg-gray-50 transition-colors">
                  {copied ? '✓ Copiado' : 'Copiar'}
                </button>
              </div>
              <p className="mt-2 text-xs text-amber-600">
                ⚠️ Guarde essa chave em local seguro — ela não será exibida novamente.
              </p>
            </div>
          </details>

          <div className="flex gap-3">
            <button
              onClick={() => { setView('setup-verify'); setCode(''); setError(''); }}
              className="rounded-lg bg-[var(--color-primary)] px-4 py-2 text-sm font-medium text-white hover:opacity-90"
            >
              Já escaneei — Próximo →
            </button>
            <button onClick={() => { setView('status'); resetSetupFlow(); }}
              className="rounded-lg border border-[var(--color-border)] px-4 py-2 text-sm text-[var(--color-text-muted)] hover:bg-gray-50">
              Cancelar
            </button>
          </div>
        </div>
      )}

      {/* ── Verify setup view (Passo 2) ────────────────────────────────────── */}
      {view === 'setup-verify' && (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-6">
          <SetupStepper step={2} />

          <h2 className="mb-1 font-semibold text-[var(--color-text)]">Passo 2 — Confirmar o código</h2>
          <p className="mb-4 text-sm text-[var(--color-text-muted)]">
            Abra o Google Authenticator e insira o código de <strong>6 dígitos</strong> que
            aparece agora para o Agile360.
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
            <button onClick={() => { setView('setup-qr'); setCode(''); setError(''); }}
              className="rounded-lg border border-[var(--color-border)] px-4 py-2 text-sm text-[var(--color-text-muted)] hover:bg-gray-50">
              Voltar
            </button>
          </div>
        </div>
      )}

      {/* ── F4 — Passo 3: Backup de Emergência ───────────────────────────────
           Exibido após verificação TOTP bem-sucedida (verifyMfaSetup retorna códigos).
           Checkbox obrigatório para habilitar o botão "Concluir ativação do 2FA".
           DoD: grid 5×2, botão download .txt, copiar todos, aviso único.
      ─────────────────────────────────────────────────────────────────────── */}
      {view === 'setup-backup' && (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-6">
          <SetupStepper step={3} />

          <div className="mb-4 flex items-center gap-2">
            <span className="text-2xl">🔑</span>
            <div>
              <h2 className="font-semibold text-[var(--color-text)]">
                Passo 3 — Seus Códigos de Recuperação de Emergência
              </h2>
              <p className="text-xs text-[var(--color-text-muted)]">
                Guarde estes códigos em local seguro. Cada um pode ser usado <strong>UMA ÚNICA VEZ</strong> para
                acessar sua conta se perder o celular.
              </p>
            </div>
          </div>

          {/* Grid 5×2 de códigos */}
          <div className="mb-4 rounded-lg border border-[var(--color-border)] bg-[var(--color-bg)] p-4">
            <div className="grid grid-cols-2 gap-2 sm:grid-cols-2">
              {backupCodes.map((c, i) => (
                <div key={i}
                  className="rounded-md border border-[var(--color-border)] bg-[var(--color-surface)] px-3 py-2 text-center font-mono text-sm font-semibold tracking-widest text-[var(--color-text)] select-all">
                  {c}
                </div>
              ))}
            </div>
          </div>

          {/* Ações: download e copiar */}
          <div className="mb-4 flex flex-wrap gap-2">
            <button
              type="button"
              onClick={handleDownloadTxt}
              className="flex items-center gap-1.5 rounded-lg border border-[var(--color-border)] bg-white px-3 py-2 text-sm font-medium text-[var(--color-text-muted)] hover:bg-gray-50 transition-colors"
            >
              ⬇ Baixar como .txt
            </button>
            <button
              type="button"
              onClick={handleCopyAll}
              className="flex items-center gap-1.5 rounded-lg border border-[var(--color-border)] bg-white px-3 py-2 text-sm font-medium text-[var(--color-text-muted)] hover:bg-gray-50 transition-colors"
            >
              {copiedAll ? '✓ Copiado!' : '📋 Copiar todos'}
            </button>
          </div>

          {/* Aviso único de exibição */}
          <div className="mb-5 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            ⚠️ <strong>Estes códigos NÃO serão exibidos novamente.</strong> Guarde-os agora antes de fechar esta página.
          </div>

          {/* Checkbox obrigatório (F4 DoD) */}
          <label className="mb-5 flex cursor-pointer items-start gap-3 rounded-lg border border-[var(--color-border)] bg-[var(--color-bg)] p-4">
            <input
              type="checkbox"
              className="mt-0.5 h-4 w-4 shrink-0 accent-[var(--color-primary)]"
              checked={backupConfirmed}
              onChange={(e) => setBackupConfirmed(e.target.checked)}
            />
            <span className="text-sm text-[var(--color-text)]">
              Eu guardei meus códigos em um local seguro e entendo que cada um pode ser usado apenas{' '}
              <strong>1 vez</strong> para recuperar o acesso à minha conta.
            </span>
          </label>

          {/* Botão Concluir — desabilitado até o checkbox ser marcado */}
          <button
            onClick={handleBackupDone}
            disabled={!backupConfirmed}
            className="w-full rounded-lg bg-green-600 py-2.5 text-sm font-semibold text-white hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-40 transition-opacity"
            title={!backupConfirmed ? 'Confirme que guardou os códigos antes de concluir' : undefined}
          >
            {backupConfirmed ? '✓ Concluir ativação do 2FA' : 'Marque a caixa acima para concluir'}
          </button>
        </div>
      )}

      {/* ── Regen confirm view ────────────────────────────────────────────── */}
      {view === 'regen-confirm' && (
        <div className="rounded-xl border border-amber-200 bg-amber-50 p-6">
          <h2 className="mb-2 font-semibold text-amber-900">Gerar novos códigos de recuperação?</h2>
          <p className="mb-5 text-sm text-amber-800">
            Esta ação irá <strong>invalidar permanentemente</strong> todos os seus códigos de recuperação atuais
            e gerar 10 novos. Os novos códigos serão exibidos apenas uma vez.
          </p>
          {error && <p className="mb-3 text-sm text-red-500">{error}</p>}
          <div className="flex gap-3">
            <button
              onClick={handleRegenConfirm}
              disabled={loading}
              className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white hover:bg-amber-700 disabled:opacity-50"
            >
              {loading ? 'Gerando...' : 'Sim, gerar novos códigos'}
            </button>
            <button
              onClick={() => { setView('status'); setError(''); }}
              className="rounded-lg border border-amber-300 px-4 py-2 text-sm text-amber-800 hover:bg-amber-100"
            >
              Cancelar
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
            Após isso, seu login não exigirá mais o código e todos os seus{' '}
            <strong>códigos de recuperação serão deletados</strong>.
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
