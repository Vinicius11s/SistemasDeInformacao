import { useCallback, useEffect, useMemo, useState, type ChangeEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Modal } from '../components/Modal';
import { Combobox } from '../components/Combobox';
import { useToken } from '../context/AuthContext';
import { clientesApi, type Cliente } from '../api/clientes';
import {
  listStagingProcessos,
  editarStagingProcesso,
  confirmarStagingProcesso,
  rejeitarStagingProcesso,
  type StagingProcessoResponse,
  type UpdateStagingProcessoPayload,
} from '../api/stagingProcessos';
import { rawDigits } from '../utils/masks';

type Draft = {
  id_cliente: string;
  num_processo: string;
  parte_contraria: string;
  valor_causa?: number;
  tribunal: string;
  comarca_vara: string;
  assunto: string;
};

function guessClientId(clienteNome: string | undefined, clientes: Cliente[]): string {
  if (!clienteNome) return '';
  const digits = rawDigits(clienteNome);
  if (digits?.length === 11) {
    const match = clientes.find(c => rawDigits(c.cpf) === digits);
    if (match) return match.id;
  }

  const lowered = clienteNome.trim().toLowerCase();
  const nameMatch = clientes.find(c => (c.nome_completo ?? '').toLowerCase().includes(lowered));
  return nameMatch?.id ?? '';
}

const EMPTY_DRAFT: Draft = {
  id_cliente: '',
  num_processo: '',
  parte_contraria: '',
  valor_causa: undefined,
  tribunal: '',
  comarca_vara: '',
  assunto: '',
};

export function StagingProcessos() {
  const token = useToken();
  const navigate = useNavigate();

  const [items, setItems] = useState<StagingProcessoResponse[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [processingId, setProcessingId] = useState<string | null>(null);

  // Modal de edição
  const [editId, setEditId] = useState<string | null>(null);
  const [draft, setDraft] = useState<Draft>(EMPTY_DRAFT);
  const [editSaving, setEditSaving] = useState(false);
  const [editConfirming, setEditConfirming] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  const opcoesClientes = useMemo(
    () =>
      clientes.map(c => ({
        value: c.id,
        label: c.nome_completo,
        sublabel: c.cpf ?? undefined,
      })),
    [clientes],
  );

  const carregar = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    const [resP, resC] = await Promise.all([
      listStagingProcessos(token),
      clientesApi.listar(token),
    ]);

    if (resP.success) setItems(resP.data ?? []);
    else setError(resP.error?.message ?? 'Erro ao carregar processos pendentes.');

    if (resC.success) setClientes(resC.data ?? []);
    else setError(resC.error?.message ?? 'Erro ao carregar clientes.');

    setLoading(false);
  }, [token]);

  useEffect(() => { void carregar(); }, [carregar]);

  function openEdit(item: StagingProcessoResponse) {
    setEditId(item.id);
    setEditError(null);
    setEditSaving(false);
    setEditConfirming(false);

    const guessedClientId = guessClientId(item.cliente_nome, clientes);

    setDraft({
      id_cliente: guessedClientId,
      num_processo: item.num_processo ?? '',
      parte_contraria: item.parte_contraria ?? '',
      valor_causa: item.valor_causa,
      tribunal: item.tribunal ?? '',
      comarca_vara: item.comarca_vara ?? '',
      assunto: item.assunto ?? '',
    });
  }

  function closeEdit() {
    setEditId(null);
    setEditSaving(false);
    setEditConfirming(false);
    setEditError(null);
    setDraft(EMPTY_DRAFT);
  }

  const setField =
    (field: keyof Draft) =>
    (e: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
      const raw = e.target.value;
      if (field === 'valor_causa') {
        setDraft(d => ({
          ...d,
          valor_causa: raw === '' ? undefined : Number(raw),
        }));
        return;
      }
      setDraft(d => ({ ...d, [field]: raw }));
    };

  async function handleSave() {
    if (!token || !editId) return;
    setEditSaving(true);
    setEditError(null);

    const payload: UpdateStagingProcessoPayload = {};

    if (draft.num_processo.trim()) payload.num_processo = draft.num_processo.trim();
    if (draft.parte_contraria.trim()) payload.parte_contraria = draft.parte_contraria.trim();
    if (draft.valor_causa !== undefined) payload.valor_causa = draft.valor_causa;
    if (draft.tribunal.trim()) payload.tribunal = draft.tribunal.trim();
    if (draft.comarca_vara.trim()) payload.comarca_vara = draft.comarca_vara.trim();
    if (draft.assunto.trim()) payload.assunto = draft.assunto.trim();

    const res = await editarStagingProcesso(editId, payload, token);
    setEditSaving(false);

    if (res.success && res.data) {
      const updated = res.data;
      setItems(prev => prev.map(i => (i.id === editId ? updated : i)));
    } else {
      setEditError(res.error?.message ?? 'Erro ao salvar alterações.');
    }
  }

  async function handleConfirm() {
    if (!token || !editId) return;
    if (!draft.id_cliente) {
      setEditError('Selecione um cliente para vincular ao processo.');
      return;
    }

    setEditConfirming(true);
    setEditError(null);

    const res = await confirmarStagingProcesso(editId, { id_cliente: draft.id_cliente }, token);
    setEditConfirming(false);

    if (res.success) {
      setItems(prev => prev.filter(i => i.id !== editId));
      closeEdit();
    } else {
      setEditError(res.error?.message ?? 'Erro ao confirmar processo.');
    }
  }

  async function handleRejeitar(id: string) {
    if (!token) return;
    const confirmed = window.confirm('Rejeitar este processo? O registro será descartado.');
    if (!confirmed) return;

    setProcessingId(id);
    const res = await rejeitarStagingProcesso(id, token);
    setProcessingId(null);

    if (res.success) setItems(prev => prev.filter(i => i.id !== id));
    else alert(res.error?.message ?? 'Erro ao rejeitar processo.');
  }

  return (
    <div className="mx-auto max-w-5xl">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-xl font-semibold text-[var(--color-text)] sm:text-2xl">Triagem de Processos</h1>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Revise os processos enviados pelo bot antes de ativar na base principal.
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="secondary" onClick={() => navigate('/app/staging')} className="min-h-[44px]">
            Triagem de Clientes
          </Button>
        </div>
      </div>

      {loading && <p className="text-[var(--color-text-muted)]">Carregando…</p>}
      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {!loading && !error && items.length === 0 && (
        <div className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-6 py-10 text-center">
          <p className="text-[var(--color-text-muted)]">Nenhum processo pendente no momento.</p>
        </div>
      )}

      {!loading && items.length > 0 && (
        <div className="space-y-4">
          {items.map(item => (
            <div
              key={item.id}
              className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-5 shadow-sm"
            >
              <div className="mb-3 flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-mono text-sm font-medium text-[var(--color-text-heading)]">
                    {item.num_processo ?? '—'}
                  </p>
                  <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                    {item.cliente_nome ?? '—'}
                  </p>
                </div>
                <div className="text-right text-xs text-[var(--color-text-muted)]">
                  Expira: {item.expires_at ? new Date(item.expires_at).toLocaleDateString('pt-BR') : '—'}
                </div>
              </div>

              <dl className="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-2 text-sm">
                <div>
                  <dt className="text-[var(--color-text-muted)]">Parte contrária</dt>
                  <dd className="font-medium text-[var(--color-text)]">{item.parte_contraria ?? '—'}</dd>
                </div>
                <div>
                  <dt className="text-[var(--color-text-muted)]">Tribunal / Comarca / Vara</dt>
                  <dd className="font-medium text-[var(--color-text)]">
                    {[item.tribunal, item.comarca_vara].filter(Boolean).join(' · ') || '—'}
                  </dd>
                </div>
                <div className="md:col-span-2">
                  <dt className="text-[var(--color-text-muted)]">Assunto</dt>
                  <dd className="font-medium text-[var(--color-text)]">{item.assunto ?? '—'}</dd>
                </div>
              </dl>

              {item.origem_mensagem && (
                <p className="mt-3 text-xs text-[var(--color-text-muted)]">
                  <span className="font-medium">Obs bot:</span> {item.origem_mensagem}
                </p>
              )}

              <div className="mt-4 flex gap-3">
                <button
                  type="button"
                  onClick={() => openEdit(item)}
                  disabled={processingId === item.id}
                  className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-4 py-2 text-sm font-medium text-[var(--color-text)] hover:bg-[var(--color-surface-elevated)] disabled:opacity-50"
                >
                  Editar (triagem)
                </button>
                <button
                  type="button"
                  onClick={() => openEdit(item)}
                  disabled={processingId === item.id}
                  className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
                >
                  {processingId === item.id ? 'Processando…' : 'Confirmar'}
                </button>
                <button
                  type="button"
                  onClick={() => handleRejeitar(item.id)}
                  disabled={processingId === item.id}
                  className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
                >
                  Rejeitar
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal
        open={!!editId}
        onClose={closeEdit}
        title="Triagem — Editar processo"
        size="max-w-2xl"
      >
        <form
          className="flex flex-col gap-4"
          onSubmit={e => {
            e.preventDefault();
            void handleSave();
          }}
        >
          {editError && (
            <p className="rounded-lg border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700">
              {editError}
            </p>
          )}

          <Combobox
            label="Cliente (para vincular na confirmação) *"
            options={opcoesClientes}
            value={draft.id_cliente}
            onChange={val => setDraft(d => ({ ...d, id_cliente: val }))}
            placeholder="Busque pelo nome ou CPF…"
            error={editError?.includes('Selecione') ? editError : undefined}
          />

          <Input
            label="Número do processo *"
            name="num_processo"
            value={draft.num_processo}
            onChange={setField('num_processo')}
            placeholder="0000000-00.2026.8.26.0000"
          />

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <Input
              label="Tribunal"
              name="tribunal"
              value={draft.tribunal}
              onChange={setField('tribunal')}
              placeholder="TJSP, TRT2, STF…"
            />
            <Input
              label="Comarca / Vara"
              name="comarca_vara"
              value={draft.comarca_vara}
              onChange={setField('comarca_vara')}
              placeholder="2ª Vara Cível de …"
            />
            <Input
              label="Valor da causa (R$)"
              name="valor_causa"
              type="number"
              value={draft.valor_causa ?? ''}
              onChange={setField('valor_causa')}
              placeholder="0,00"
              step="0.01"
              min="0"
            />
            <Input
              label="Parte contrária"
              name="parte_contraria"
              value={draft.parte_contraria}
              onChange={setField('parte_contraria')}
              placeholder="Banco do Brasil S/A"
            />
          </div>

          <Input
            label="Assunto"
            name="assunto"
            value={draft.assunto}
            onChange={setField('assunto')}
            placeholder="Danos Morais, Reclamação Trabalhista…"
          />

          <div className="flex justify-end gap-3 border-t pt-3 border-[var(--color-border)]">
            <Button variant="secondary" type="button" onClick={closeEdit}>
              Cancelar
            </Button>
            <Button
              variant="primary"
              type="button"
              loading={editSaving}
              onClick={() => void handleSave()}
            >
              Salvar alterações (PATCH)
            </Button>
            <Button
              variant="primary"
              type="button"
              loading={editConfirming}
              onClick={() => void handleConfirm()}
              className="bg-green-700 hover:bg-green-800"
            >
              Confirmar (ativar)
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

