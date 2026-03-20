import { useCallback, useEffect, useMemo, useState, type ChangeEvent } from 'react';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Modal } from '../components/Modal';
import { Combobox } from '../components/Combobox';
import { useToken } from '../context/AuthContext';
import { clientesApi, type Cliente } from '../api/clientes';
import { processosApi, type Processo } from '../api/processos';
import {
  listStagingCompromissos,
  editarStagingCompromisso,
  confirmarStagingCompromisso,
  rejeitarStagingCompromisso,
  type StagingCompromissoResponse,
  type UpdateStagingCompromissoPayload,
  type TipoCompromisso,
} from '../api/stagingCompromissos';
import { rawDigits } from '../utils/masks';

const TIPOS_COMPROMISSO: TipoCompromisso[] = ['Audiência', 'Atendimento', 'Reunião', 'Prazo'];

type Draft = {
  tipo_compromisso: string;
  data: string; // YYYY-MM-DD
  hora: string; // HH:mm
  local: string;
  lembrete_minutos?: number;
  id_cliente?: string;
  id_processo?: string;
};

const EMPTY_DRAFT: Draft = {
  tipo_compromisso: 'Audiência',
  data: '',
  hora: '',
  local: '',
  lembrete_minutos: undefined,
  id_cliente: undefined,
  id_processo: undefined,
};

export function StagingCompromissos() {
  const token = useToken();

  const [items, setItems] = useState<StagingCompromissoResponse[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [processos, setProcessos] = useState<Processo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [processingId, setProcessingId] = useState<string | null>(null);

  // Modal de edição
  const [editId, setEditId] = useState<string | null>(null);
  const [draft, setDraft] = useState<Draft>(EMPTY_DRAFT);
  const [editSaving, setEditSaving] = useState(false);
  const [editConfirming, setEditConfirming] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  const carregar = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);

    const [resStaging, resClientes, resProcessos] = await Promise.all([
      listStagingCompromissos(token),
      clientesApi.listar(token),
      processosApi.listar(token),
    ]);

    if (resStaging.success) setItems(resStaging.data ?? []);
    else setError(resStaging.error?.message ?? 'Erro ao carregar compromissos pendentes.');

    if (resClientes.success) setClientes(resClientes.data ?? []);
    if (resProcessos.success) setProcessos(resProcessos.data ?? []);

    setLoading(false);
  }, [token]);

  useEffect(() => { void carregar(); }, [carregar]);

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

  function openEdit(item: StagingCompromissoResponse) {
    const cpfDigits = rawDigits(item.cliente_nome ?? undefined);
    const guessedClienteId =
      cpfDigits
        ? clientes.find(c => rawDigits(c.cpf) === cpfDigits)?.id
        : undefined;

    const numProcDigits = rawDigits(item.num_processo ?? undefined);
    const guessedProcessoId =
      numProcDigits
        ? processos.find(p => rawDigits(p.num_processo) === numProcDigits)?.id
        : undefined;

    setEditId(item.id);
    setEditError(null);
    setEditSaving(false);
    setEditConfirming(false);
    setDraft({
      tipo_compromisso: item.tipo_compromisso ?? 'Audiência',
      data: item.data ?? '',
      hora: item.hora ? item.hora.substring(0, 5) : '',
      local: item.local ?? '',
      lembrete_minutos: item.lembrete_minutos,
      id_cliente: guessedClienteId ?? undefined,
      id_processo: guessedProcessoId ?? undefined,
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
      if (field === 'lembrete_minutos') {
        setDraft(d => ({ ...d, lembrete_minutos: raw === '' ? undefined : Number(raw) }));
        return;
      }
      setDraft(d => ({ ...d, [field]: raw }));
    };

  async function handleSave() {
    if (!token || !editId) return;
    setEditSaving(true);
    setEditError(null);

    const payload: UpdateStagingCompromissoPayload = {};

    if (draft.tipo_compromisso.trim()) payload.tipo_compromisso = draft.tipo_compromisso.trim();
    if (draft.data) payload.data = draft.data;
    if (draft.hora) payload.hora = draft.hora.length === 5 ? `${draft.hora}:00` : draft.hora;
    if (draft.local.trim()) payload.local = draft.local.trim();
    if (draft.lembrete_minutos !== undefined) payload.lembrete_minutos = draft.lembrete_minutos;

    const res = await editarStagingCompromisso(editId, payload, token);
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

    // Valida vínculo mínimo antes de promover.
    if (!draft.id_cliente) {
      setEditError('Selecione um cliente (id_cliente) para vincular este compromisso.');
      return;
    }
    if (draft.tipo_compromisso === 'Audiência' && !draft.id_processo) {
      setEditError('Para Audiência, selecione um processo válido para vincular.');
      return;
    }

    setEditConfirming(true);
    setEditError(null);

    const res = await confirmarStagingCompromisso(editId, {
      id_cliente: draft.id_cliente,
      id_processo: draft.id_processo,
    }, token);
    setEditConfirming(false);

    if (res.success) {
      setItems(prev => prev.filter(i => i.id !== editId));
      closeEdit();
    } else {
      setEditError(res.error?.message ?? 'Erro ao confirmar compromisso.');
    }
  }

  async function handleRejeitar(id: string) {
    if (!token) return;
    const confirmed = window.confirm('Rejeitar este compromisso? O registro será descartado.');
    if (!confirmed) return;

    setProcessingId(id);
    const res = await rejeitarStagingCompromisso(id, token);
    setProcessingId(null);

    if (res.success) setItems(prev => prev.filter(i => i.id !== id));
    else alert(res.error?.message ?? 'Erro ao rejeitar compromisso.');
  }

  return (
    <div className="mx-auto max-w-5xl">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-xl font-semibold text-[var(--color-text)] sm:text-2xl">Triagem de Compromissos</h1>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Revise e confirme os compromissos enviados pelo bot.
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
          <p className="text-[var(--color-text-muted)]">Nenhum compromisso pendente no momento.</p>
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
                  <p className="font-semibold text-[var(--color-text-heading)]">{item.tipo_compromisso ?? '—'}</p>
                  <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                    {(item.data ?? '—')}{item.hora ? ` · ${item.hora.substring(0, 5)}` : ''}
                  </p>
                  {item.cliente_nome && (
                    <p className="mt-0.5 text-xs text-[var(--color-text-muted)]">
                      Cliente: {item.cliente_nome}
                    </p>
                  )}
                </div>
                <div className="text-right text-xs text-[var(--color-text-muted)]">
                  Expira: {item.expires_at ? new Date(item.expires_at).toLocaleDateString('pt-BR') : '—'}
                </div>
              </div>

              <dl className="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-2 text-sm">
                <div>
                  <dt className="text-[var(--color-text-muted)]">Local</dt>
                  <dd className="font-medium text-[var(--color-text)]">{item.local ?? '—'}</dd>
                </div>
                <div>
                  <dt className="text-[var(--color-text-muted)]">Processo</dt>
                  <dd className="font-medium text-[var(--color-text)]">{item.num_processo ?? '—'}</dd>
                </div>
                <div className="md:col-span-2">
                  <dt className="text-[var(--color-text-muted)]">Lembrete</dt>
                  <dd className="font-medium text-[var(--color-text)]">
                    {item.lembrete_minutos !== undefined ? `${item.lembrete_minutos} min antes` : '—'}
                  </dd>
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
                  Confirmar
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
        title="Triagem — Editar compromisso"
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

          <div className="flex flex-col gap-1">
            <label className="text-sm text-[var(--color-text-muted)]">Tipo de compromisso *</label>
            <select
              value={draft.tipo_compromisso}
              onChange={setField('tipo_compromisso')}
              className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
            >
              {TIPOS_COMPROMISSO.map(t => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>
          </div>

          <Combobox
            label="Cliente *"
            options={opcoesClientes}
            value={draft.id_cliente ?? ''}
            onChange={val => setDraft(d => ({ ...d, id_cliente: val || undefined }))}
            placeholder="Busque pelo nome ou CPF…"
            error={editError?.includes('cliente') ? editError : undefined}
          />

          {draft.tipo_compromisso === 'Audiência' && (
            <Combobox
              label="Processo *"
              options={opcoesProcessos}
              value={draft.id_processo ?? ''}
              onChange={val => setDraft(d => ({ ...d, id_processo: val || undefined }))}
              placeholder="Busque pelo número do processo…"
              error={editError?.includes('processo') ? editError : undefined}
            />
          )}

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <Input label="Data *" name="data" type="date" value={draft.data} onChange={setField('data')} />
            <Input
              label="Hora *"
              name="hora"
              type="time"
              value={draft.hora}
              onChange={setField('hora')}
            />
            <Input
              label="Lembrete (minutos antes)"
              name="lembrete_minutos"
              type="number"
              min="0"
              value={draft.lembrete_minutos ?? ''}
              onChange={setField('lembrete_minutos')}
              placeholder="ex.: 60"
            />
            <Input
              label="Local"
              name="local"
              value={draft.local}
              onChange={setField('local')}
              placeholder="Fórum Cível - Sala 402 ou link do Google Meet"
            />
          </div>

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

