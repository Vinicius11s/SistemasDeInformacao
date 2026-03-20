import { useCallback, useEffect, useMemo, useState, type ChangeEvent } from 'react';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Modal } from '../components/Modal';
import { Combobox } from '../components/Combobox';
import { useToken } from '../context/AuthContext';
import { clientesApi, type Cliente } from '../api/clientes';
import { processosApi, type Processo } from '../api/processos';
import {
  listStagingPrazos,
  editarStagingPrazo,
  confirmarStagingPrazo,
  rejeitarStagingPrazo,
  type StagingPrazoResponse,
  type UpdateStagingPrazoPayload,
} from '../api/stagingPrazos';
// (sem util de máscara aqui)

type PrioridadeTriagem = 'Normal' | 'Urgente';
type TipoContagem = 'Util' | 'Corrido';

type Draft = {
  titulo: string;
  data_vencimento: string; // YYYY-MM-DD
  prioridade: PrioridadeTriagem;
  tipo_contagem: TipoContagem;
  id_cliente?: string;
  id_processo?: string;
};

const EMPTY_DRAFT: Draft = {
  titulo: '',
  data_vencimento: '',
  prioridade: 'Normal',
  tipo_contagem: 'Util',
  id_cliente: undefined,
  id_processo: undefined,
};

export function StagingPrazos() {
  const token = useToken();

  const [items, setItems] = useState<StagingPrazoResponse[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [processos, setProcessos] = useState<Processo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [processingId, setProcessingId] = useState<string | null>(null);

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

  const opcoesProcessos = useMemo(
    () =>
      processos.map(p => ({
        value: p.id,
        label: p.num_processo,
        sublabel: p.assunto ?? undefined,
      })),
    [processos],
  );

  const carregar = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);

    const [resStaging, resClientes, resProcessos] = await Promise.all([
      listStagingPrazos(token),
      clientesApi.listar(token),
      processosApi.listar(token),
    ]);

    if (resStaging.success) setItems(resStaging.data ?? []);
    else setError(resStaging.error?.message ?? 'Erro ao carregar prazos pendentes.');

    if (resClientes.success) setClientes(resClientes.data ?? []);
    if (resProcessos.success) setProcessos(resProcessos.data ?? []);

    setLoading(false);
  }, [token]);

  useEffect(() => { void carregar(); }, [carregar]);

  function guessIds(item: StagingPrazoResponse) {
    // Preferir id_cliente/id_processo vindos da staging.
    const guessedClienteId = item.cliente_id ?? undefined;
    const guessedProcessoId = item.processo_id ?? undefined;

    // Se vier vazio, tentamos inferir por CPF (se o schema/DTO trouxer algo; aqui não há cliente_nome).
    // Deixamos como undefined caso não exista.
    void guessedProcessoId;
    return { guessedClienteId, guessedProcessoId };
  }

  function openEdit(item: StagingPrazoResponse) {
    const { guessedClienteId, guessedProcessoId } = guessIds(item);
    const prioridade =
      item.prioridade === 'Urgente' ? 'Urgente' : 'Normal';

    const tipo_contagem =
      item.tipo_contagem === 'Corrido' ? 'Corrido' : 'Util';

    setEditId(item.id);
    setEditError(null);
    setEditSaving(false);
    setEditConfirming(false);

    setDraft({
      titulo: item.titulo ?? '',
      data_vencimento: item.data_vencimento ?? '',
      prioridade,
      tipo_contagem,
      id_cliente: guessedClienteId,
      id_processo: guessedProcessoId,
    });
  }

  function closeEdit() {
    setEditId(null);
    setDraft(EMPTY_DRAFT);
    setEditSaving(false);
    setEditConfirming(false);
    setEditError(null);
  }

  const setField =
    (field: keyof Draft) =>
    (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
      const v = e.target.value;
      setDraft(d => ({ ...d, [field]: v as any }));
    };

  async function handleSave() {
    if (!token || !editId) return;
    setEditSaving(true);
    setEditError(null);

    const payload: UpdateStagingPrazoPayload = {
      titulo: draft.titulo.trim() || undefined,
      data_vencimento: draft.data_vencimento || undefined,
      prioridade: draft.prioridade,
      tipo_contagem: draft.tipo_contagem,
    };

    const res = await editarStagingPrazo(editId, payload, token);
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
      setEditError('Selecione um cliente para vincular o prazo.');
      return;
    }

    setEditConfirming(true);
    setEditError(null);

    const res = await confirmarStagingPrazo(editId, {
      id_cliente: draft.id_cliente,
      id_processo: draft.id_processo,
    }, token);
    setEditConfirming(false);

    if (res.success) {
      setItems(prev => prev.filter(i => i.id !== editId));
      closeEdit();
    } else {
      setEditError(res.error?.message ?? 'Erro ao confirmar prazo.');
    }
  }

  async function handleRejeitar(id: string) {
    if (!token) return;
    const confirmed = window.confirm('Rejeitar este prazo? O registro será descartado.');
    if (!confirmed) return;

    setProcessingId(id);
    const res = await rejeitarStagingPrazo(id, token);
    setProcessingId(null);

    if (res.success) setItems(prev => prev.filter(i => i.id !== id));
    else alert(res.error?.message ?? 'Erro ao rejeitar prazo.');
  }

  return (
    <div className="mx-auto max-w-5xl">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-xl font-semibold text-[var(--color-text)] sm:text-2xl">Triagem de Prazos</h1>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Revise e confirme os prazos enviados pelo bot antes de ativar.
          </p>
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
          <p className="text-[var(--color-text-muted)]">Nenhum prazo pendente no momento.</p>
        </div>
      )}

      {!loading && items.length > 0 && (
        <div className="space-y-4">
          {items.map(item => (
            <div key={item.id} className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-5 shadow-sm">
              <div className="mb-3 flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-semibold text-[var(--color-text-heading)]">{item.titulo ?? '—'}</p>
                  <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                    Venc.: {item.data_vencimento ?? '—'} · {item.tipo_contagem ?? '—'}
                  </p>
                </div>
                <div className="text-right text-xs text-[var(--color-text-muted)]">
                  Expira: {item.expires_at ? new Date(item.expires_at).toLocaleDateString('pt-BR') : '—'}
                </div>
              </div>

              <div className="mt-4 flex gap-3">
                <button
                  type="button"
                  onClick={() => openEdit(item)}
                  disabled={processingId === item.id}
                  className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-4 py-2 text-sm font-medium text-[var(--color-text)] hover:bg-[var(--color-surface-elevated)] disabled:opacity-50"
                >
                  Revisar (triagem)
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
        title="Triagem — Editar prazo"
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
            <p className="rounded-lg border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700">{editError}</p>
          )}

          <Input
            label="Título *"
            name="titulo"
            value={draft.titulo}
            onChange={setField('titulo')}
            placeholder="Ex.: Intimação para audiência / Prazo de recurso…"
          />

          <Input
            label="Data de vencimento *"
            name="data_vencimento"
            type="date"
            value={draft.data_vencimento}
            onChange={setField('data_vencimento')}
          />

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div className="flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Prioridade</label>
              <select
                value={draft.prioridade}
                onChange={setField('prioridade')}
                className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
              >
                <option value="Normal">Normal</option>
                <option value="Urgente">Urgente</option>
              </select>
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Tipo de contagem</label>
              <select
                value={draft.tipo_contagem}
                onChange={setField('tipo_contagem')}
                className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
              >
                <option value="Util">Útil</option>
                <option value="Corrido">Corrido</option>
              </select>
            </div>
          </div>

          <Combobox
            label="Cliente *"
            options={opcoesClientes}
            value={draft.id_cliente ?? ''}
            onChange={val => setDraft(d => ({ ...d, id_cliente: val || undefined }))}
            placeholder="Busque pelo nome ou CPF…"
          />

          <Combobox
            label="Processo (opcional)"
            options={opcoesProcessos}
            value={draft.id_processo ?? ''}
            onChange={val => setDraft(d => ({ ...d, id_processo: val || undefined }))}
            placeholder="Busque pelo número do processo…"
          />

          <div className="flex justify-end gap-3 border-t pt-3 border-[var(--color-border)]">
            <Button variant="secondary" type="button" onClick={closeEdit}>
              Cancelar
            </Button>
            <Button variant="primary" type="button" loading={editSaving} onClick={() => void handleSave()}>
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

