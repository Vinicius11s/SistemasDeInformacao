import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import {
  listStagingPendentes,
  confirmarStaging,
  rejeitarStaging,
  type StagingClienteResponse,
} from '../api/staging';
import { formatCpf, formatCnpj, formatPhone } from '../utils/masks';
import { getLabels } from '../utils/clienteLabels';

function formatDoc(s: StagingClienteResponse) {
  if (s.tipoPessoa === 'PessoaJuridica') return formatCnpj(s.cnpj);
  return formatCpf(s.cpf);
}

function formatDate(raw?: string) {
  if (!raw) return '—';
  const [y, m, d] = raw.split('-');
  return `${d}/${m}/${y}`;
}

export function StagingClientes() {
  const { state } = useAuth();
  const navigate = useNavigate();

  const [items, setItems] = useState<StagingClienteResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [processing, setProcessing] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!state.token) return;
    setLoading(true);
    const res = await listStagingPendentes(state.token);
    if (res.success && res.data) {
      setItems(res.data);
      setError(null);
    } else {
      setError(res.error?.message ?? 'Erro ao carregar registros pendentes.');
    }
    setLoading(false);
  }, [state.token]);

  useEffect(() => { load(); }, [load]);

  async function handleConfirmar(id: string) {
    if (!state.token) return;
    setProcessing(id);
    const res = await confirmarStaging(id, state.token);
    setProcessing(null);
    if (res.success) {
      setItems(prev => prev.filter(i => i.id !== id));
      navigate('/app/clientes');
    } else {
      alert(res.error?.message ?? 'Erro ao confirmar cadastro.');
    }
  }

  async function handleRejeitar(id: string) {
    if (!state.token) return;
    if (!window.confirm('Rejeitar este cadastro? O registro será descartado.')) return;
    setProcessing(id);
    const res = await rejeitarStaging(id, state.token);
    setProcessing(null);
    if (res.success) {
      setItems(prev => prev.filter(i => i.id !== id));
    } else {
      alert(res.error?.message ?? 'Erro ao rejeitar cadastro.');
    }
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-[var(--color-text)]">
            Ações Pendentes do Bot
          </h1>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Cadastros enviados via WhatsApp aguardando sua revisão. Confirme para adicionar à base ou rejeite para descartar.
          </p>
        </div>
      </div>

      {loading && (
        <p className="text-[var(--color-text-muted)]">Carregando...</p>
      )}

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {!loading && !error && items.length === 0 && (
        <div className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-6 py-10 text-center">
          <p className="text-[var(--color-text-muted)]">Nenhum cadastro pendente no momento.</p>
        </div>
      )}

      {!loading && items.length > 0 && (
        <div className="space-y-4">
          {items.map(item => {
            const labels = getLabels(item.tipoPessoa);
            const isPending = processing === item.id;
            const expiresAt = new Date(item.expiresAt);
            const isExpiringSoon = (expiresAt.getTime() - Date.now()) < 1000 * 60 * 60 * 3; // 3h

            return (
              <div
                key={item.id}
                className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-5 shadow-sm"
              >
                {/* Header row */}
                <div className="mb-3 flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <span className="inline-block rounded-full bg-amber-100 px-2.5 py-0.5 text-xs font-medium text-amber-800">
                      {item.tipoPessoa === 'PessoaJuridica' ? 'Pessoa Jurídica' : 'Pessoa Física'}
                    </span>
                    <h2 className="mt-1 text-base font-semibold text-[var(--color-text)]">
                      {item.tipoPessoa === 'PessoaJuridica' ? (item.razaoSocial || '—') : (item.nome || '—')}
                    </h2>
                  </div>
                  <div className="text-right text-xs text-[var(--color-text-muted)]">
                    <div>Recebido: {formatDate(item.createdAt.slice(0, 10))}</div>
                    <div className={isExpiringSoon ? 'font-medium text-amber-600' : ''}>
                      Expira: {formatDate(item.expiresAt.slice(0, 10))}
                    </div>
                  </div>
                </div>

                {/* Data grid */}
                <dl className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm sm:grid-cols-3 lg:grid-cols-4">
                  <DataItem label={labels.documentoHeader} value={formatDoc(item)} />
                  {item.tipoPessoa === 'PessoaFisica' && item.rg && (
                    <DataItem label="RG" value={item.rg} />
                  )}
                  {item.email && <DataItem label="E-mail" value={item.email} />}
                  {item.telefone && <DataItem label="Telefone" value={formatPhone(item.telefone)} />}
                  {item.whatsAppNumero && <DataItem label="WhatsApp" value={formatPhone(item.whatsAppNumero)} />}
                  {item.dataReferencia && <DataItem label={labels.dataHeader} value={formatDate(item.dataReferencia)} />}
                  {item.areaAtuacao && <DataItem label={labels.areaAtuacaoHeader} value={item.areaAtuacao} />}
                  {item.endereco && <DataItem label="Endereço" value={item.endereco} />}
                </dl>

                {/* Original WhatsApp message */}
                {item.origemMensagem && (
                  <details className="mt-3">
                    <summary className="cursor-pointer text-xs text-[var(--color-text-muted)] underline">
                      Ver mensagem original do WhatsApp
                    </summary>
                    <p className="mt-1 rounded bg-gray-50 px-3 py-2 text-xs text-gray-700 whitespace-pre-wrap">
                      {item.origemMensagem}
                    </p>
                  </details>
                )}

                {/* Observações */}
                {item.observacoes && (
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    <span className="font-medium">Obs:</span> {item.observacoes}
                  </p>
                )}

                {/* Action buttons */}
                <div className="mt-4 flex gap-3">
                  <button
                    onClick={() => handleConfirmar(item.id)}
                    disabled={isPending}
                    className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
                  >
                    {isPending ? 'Processando...' : 'Confirmar Cadastro'}
                  </button>
                  <button
                    onClick={() => handleRejeitar(item.id)}
                    disabled={isPending}
                    className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
                  >
                    Rejeitar
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

function DataItem({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[var(--color-text-muted)]">{label}</dt>
      <dd className="font-medium text-[var(--color-text)]">{value || '—'}</dd>
    </div>
  );
}
