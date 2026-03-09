import { useEffect, useMemo, useState } from 'react';
import { Button }   from '../components/Button';
import { Input }    from '../components/Input';
import { Modal }    from '../components/Modal';
import { Combobox } from '../components/Combobox';
import { useToken } from '../context/AuthContext';
import {
  type Compromisso,
  type CriarCompromissoPayload,
  type TipoCompromisso,
  compromissosApi,
} from '../api/compromissos';
import { clientesApi, type Cliente } from '../api/clientes';
import { processosApi, type Processo } from '../api/processos';

// ─── Constantes ───────────────────────────────────────────────────────────────

const TIPOS_COMPROMISSO: TipoCompromisso[] = ['Audiência', 'Atendimento', 'Reunião', 'Prazo'];
const TIPOS_AUDIENCIA = ['Conciliação', 'Instrução e Julgamento', 'Una', 'Inicial'];

type FormState = Omit<CriarCompromissoPayload, never>;

const EMPTY_FORM: FormState = {
  tipo_compromisso: 'Audiência',
  tipo_audiencia:   '',
  is_active:        true,
  data:             '',
  hora:             '',
  local:            '',
  id_cliente:       undefined,
  id_processo:      undefined,
  observacoes:      '',
  lembrete_minutos: undefined,
};

// ─── Componente ───────────────────────────────────────────────────────────────

export function Audiencias() {
  const token = useToken();

  const [compromissos, setCompromissos] = useState<Compromisso[]>([]);
  const [clientes, setClientes]         = useState<Cliente[]>([]);
  const [processos, setProcessos]       = useState<Processo[]>([]);
  const [carregando, setCarregando]     = useState(true);
  const [erroLista, setErroLista]       = useState<string | null>(null);

  const [modalAberto, setModalAberto] = useState(false);
  const [editando, setEditando]       = useState<Compromisso | null>(null);
  const [form, setForm]               = useState<FormState>(EMPTY_FORM);
  const [salvando, setSalvando]       = useState(false);
  const [erroForm, setErroForm]       = useState<string | null>(null);

  // ─── Carregar ────────────────────────────────────────────────────────────
  const carregar = async () => {
    if (!token) return;
    setCarregando(true);
    setErroLista(null);
    const [resC, rCl, rPr] = await Promise.all([
      compromissosApi.listar(token),
      clientesApi.listar(token),
      processosApi.listar(token),
    ]);
    if (resC.success) setCompromissos(resC.data ?? []);
    else setErroLista(resC.error?.message ?? 'Erro ao carregar.');
    if (rCl.success) setClientes(rCl.data ?? []);
    if (rPr.success) setProcessos(rPr.data ?? []);
    setCarregando(false);
  };

  useEffect(() => { carregar(); }, [token]);

  const nomeCliente  = (id?: string) => id ? (clientes.find(c => c.id === id)?.nome_completo ?? '—') : '—';
  const numProcesso  = (id?: string) => id ? (processos.find(p => p.id === id)?.num_processo ?? '—') : '—';

  // Opções para Combobox
  const opcoesClientes = useMemo(() =>
    clientes.map(c => ({ value: c.id, label: c.nome_completo, sublabel: c.cpf ?? undefined })),
    [clientes]);

  const opcoesProcessos = useMemo(() =>
    processos.map(p => ({ value: p.id, label: p.num_processo, sublabel: p.assunto ?? undefined })),
    [processos]);

  // ─── Modal ───────────────────────────────────────────────────────────────
  const abrirCriar = () => {
    setEditando(null); setForm(EMPTY_FORM); setErroForm(null); setModalAberto(true);
  };

  const abrirEditar = (c: Compromisso) => {
    setEditando(c);
    setForm({
      tipo_compromisso: c.tipo_compromisso,
      tipo_audiencia:   c.tipo_audiencia ?? '',
      is_active:        c.is_active,
      data:             c.data,
      hora:             c.hora.substring(0, 5),   // HH:mm
      local:            c.local ?? '',
      id_cliente:       c.id_cliente,
      id_processo:      c.id_processo,
      observacoes:      c.observacoes ?? '',
      lembrete_minutos: c.lembrete_minutos,
    });
    setErroForm(null);
    setModalAberto(true);
  };

  const salvar = async () => {
    if (!token) return;
    if (!form.data) { setErroForm('Data é obrigatória.'); return; }
    if (!form.hora) { setErroForm('Hora é obrigatória.'); return; }
    if (form.tipo_compromisso === 'Audiência' && !form.id_processo) {
      setErroForm('Processo é obrigatório para Audiência.'); return;
    }
    setSalvando(true); setErroForm(null);
    const payload: CriarCompromissoPayload = {
      ...form,
      hora: form.hora.length === 5 ? form.hora + ':00' : form.hora,  // HH:mm → HH:mm:ss
      id_cliente:  form.id_cliente  || undefined,
      id_processo: form.id_processo || undefined,
      tipo_audiencia: form.tipo_compromisso === 'Audiência' ? form.tipo_audiencia : undefined,
    };
    try {
      if (editando) {
        const res = await compromissosApi.atualizar(editando.id, payload, token);
        if (!res.success) { setErroForm(res.error?.message ?? 'Erro ao salvar.'); return; }
      } else {
        const res = await compromissosApi.criar(payload, token);
        if (!res.success) { setErroForm(res.error?.message ?? 'Erro ao criar.'); return; }
      }
      setModalAberto(false);
      carregar();
    } finally {
      setSalvando(false);
    }
  };

  const excluir = async (id: string) => {
    if (!token || !confirm('Deseja excluir este compromisso?')) return;
    await compromissosApi.excluir(id, token);
    carregar();
  };

  const set = (field: keyof FormState) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setForm(f => ({
        ...f,
        [field]: e.target.type === 'checkbox'
          ? (e.target as HTMLInputElement).checked
          : e.target.type === 'number'
            ? (e.target.value === '' ? undefined : Number(e.target.value))
            : e.target.value || undefined,
      }));

  // ─── Render ─────────────────────────────────────────────────────────────
  return (
    <div>
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-text)] sm:text-2xl">Compromissos</h1>
        <Button variant="primary" onClick={abrirCriar} className="w-full sm:w-auto min-h-[44px]">+ Novo compromisso</Button>
      </div>

      {carregando ? (
        <p className="text-[var(--color-text-muted)]">Carregando…</p>
      ) : erroLista ? (
        <p className="text-[var(--color-error)]">{erroLista}</p>
      ) : compromissos.length === 0 ? (
        <p className="text-[var(--color-text-muted)]">Nenhum compromisso cadastrado ainda.</p>
      ) : (
        <>
          {/* Mobile: cards roláveis — uma mão, sem tabela que vaza */}
          <div className="flex flex-col gap-3 md:hidden">
            {[...compromissos]
              .sort((a, b) => a.data.localeCompare(b.data))
              .map(c => (
                <article
                  key={c.id}
                  className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-4 shadow-sm"
                >
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0 flex-1">
                      <p className="font-semibold text-[var(--color-text-heading)]">{c.tipo_compromisso}</p>
                      {c.tipo_audiencia && (
                        <p className="text-xs text-[var(--color-text-muted)]">{c.tipo_audiencia}</p>
                      )}
                      <p className="mt-1 text-sm text-[var(--color-text)]">
                        {new Date(c.data + 'T00:00:00').toLocaleDateString('pt-BR', { weekday: 'short', day: '2-digit', month: 'short' })}
                        {' · '}{c.hora.substring(0, 5)}
                      </p>
                      {nomeCliente(c.id_cliente) !== '—' && (
                        <p className="mt-0.5 text-sm text-[var(--color-text-secondary)]">Cliente: {nomeCliente(c.id_cliente)}</p>
                      )}
                      {numProcesso(c.id_processo) !== '—' && (
                        <p className="font-mono text-xs text-[var(--color-text-muted)]">{numProcesso(c.id_processo)}</p>
                      )}
                    </div>
                    <StatusBadge isActive={c.is_active} />
                  </div>
                  <div className="mt-3 flex gap-2">
                    <button
                      type="button"
                      onClick={() => abrirEditar(c)}
                      className="min-h-[44px] min-w-[44px] flex-1 rounded-[var(--radius)] border border-[var(--color-border)] px-4 py-2 text-sm font-medium text-[var(--color-primary)] touch-manipulation"
                    >
                      Editar
                    </button>
                    <button
                      type="button"
                      onClick={() => excluir(c.id)}
                      className="min-h-[44px] min-w-[44px] flex-1 rounded-[var(--radius)] border border-[var(--color-border)] px-4 py-2 text-sm font-medium text-[var(--color-error)] touch-manipulation"
                    >
                      Excluir
                    </button>
                  </div>
                </article>
              ))}
          </div>

          {/* Desktop: tabela */}
          <div className="hidden overflow-x-auto rounded-xl border border-[var(--color-border)] md:block">
            <table className="min-w-full text-sm text-[var(--color-text)]">
              <thead className="bg-[var(--color-surface)] text-[var(--color-text-muted)] text-xs uppercase tracking-wider">
                <tr>
                  {['Tipo', 'Data', 'Hora', 'Cliente', 'Processo', 'Status', 'Ações'].map(h => (
                    <th key={h} className="px-4 py-3 text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--color-border)]">
                {compromissos
                  .sort((a, b) => a.data.localeCompare(b.data))
                  .map(c => (
                    <tr key={c.id} className="hover:bg-[var(--color-surface)] transition-colors">
                      <td className="px-4 py-3">
                        <span className="font-medium">{c.tipo_compromisso}</span>
                        {c.tipo_audiencia && <span className="block text-xs text-[var(--color-text-muted)]">{c.tipo_audiencia}</span>}
                      </td>
                      <td className="px-4 py-3">{new Date(c.data + 'T00:00:00').toLocaleDateString('pt-BR')}</td>
                      <td className="px-4 py-3">{c.hora.substring(0, 5)}</td>
                      <td className="px-4 py-3">{nomeCliente(c.id_cliente)}</td>
                      <td className="px-4 py-3 font-mono text-xs">{numProcesso(c.id_processo)}</td>
                      <td className="px-4 py-3"><StatusBadge isActive={c.is_active} /></td>
                      <td className="px-4 py-3">
                        <div className="flex gap-2">
                          <button onClick={() => abrirEditar(c)} className="text-[var(--color-primary)] hover:underline text-xs">Editar</button>
                          <button onClick={() => excluir(c.id)} className="text-[var(--color-error)] hover:underline text-xs">Excluir</button>
                        </div>
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {/* Modal */}
      <Modal open={modalAberto} onClose={() => setModalAberto(false)}
        title={editando ? 'Editar compromisso' : 'Novo compromisso'} size="max-w-2xl">
        <div className="flex flex-col gap-4">
          {erroForm && (
            <p className="rounded-lg bg-red-500/10 border border-red-500/30 px-3 py-2 text-sm text-red-400">{erroForm}</p>
          )}

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {/* Tipo compromisso */}
            <div className="flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Tipo de compromisso *</label>
              <select value={form.tipo_compromisso} onChange={set('tipo_compromisso')}
                className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none">
                {TIPOS_COMPROMISSO.map(t => <option key={t}>{t}</option>)}
              </select>
            </div>

            {/* Tipo audiência (só se audiência) */}
            {form.tipo_compromisso === 'Audiência' && (
              <div className="flex flex-col gap-1">
                <label className="text-sm text-[var(--color-text-muted)]">Tipo de audiência</label>
                <select value={form.tipo_audiencia ?? ''} onChange={set('tipo_audiencia')}
                  className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none">
                  <option value="">Selecione…</option>
                  {TIPOS_AUDIENCIA.map(t => <option key={t}>{t}</option>)}
                </select>
              </div>
            )}

            {/* Ativo */}
            <div className="flex min-h-[44px] items-center gap-2">
              <input
                type="checkbox"
                id="form-is_active"
                checked={form.is_active}
                onChange={set('is_active')}
                className="h-4 w-4 rounded border-[var(--color-border)] accent-[var(--color-primary)]"
              />
              <label htmlFor="form-is_active" className="text-sm text-[var(--color-text)]">Compromisso ativo</label>
            </div>

            <Input label="Data *" name="data" type="date" value={form.data} onChange={set('data')} />
            <Input label="Hora *" name="hora" type="time" value={form.hora} onChange={set('hora')} />

            <div className="sm:col-span-2">
              <Input label="Local" name="local" value={form.local ?? ''} onChange={set('local')} placeholder="Fórum Cível - Sala 402 ou Link do Google Meet" />
            </div>

            {/* Cliente (opcional) */}
            <Combobox
              label="Cliente (opcional)"
              options={opcoesClientes}
              value={form.id_cliente ?? ''}
              onChange={val => setForm(f => ({ ...f, id_cliente: val || undefined }))}
              placeholder="Busque pelo nome ou CPF…"
            />

            {/* Processo */}
            <Combobox
              label={`Processo ${form.tipo_compromisso === 'Audiência' ? '*' : '(opcional)'}`}
              options={opcoesProcessos}
              value={form.id_processo ?? ''}
              onChange={val => setForm(f => ({ ...f, id_processo: val || undefined }))}
              placeholder="Busque pelo número do processo…"
            />

            <Input label="Lembrete (minutos antes)" name="lembrete_minutos" type="number" min="0"
              value={form.lembrete_minutos ?? ''} onChange={set('lembrete_minutos')} placeholder="ex.: 60" />

            <div className="sm:col-span-2 flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Observações</label>
              <textarea rows={3} value={form.observacoes ?? ''} onChange={set('observacoes')}
                placeholder="Levar documentos originais e avisar testemunhas."
                className="rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 py-2 text-[var(--color-text)] placeholder:text-[var(--color-text-muted)] focus:border-[var(--color-primary)] focus:outline-none resize-none"
              />
            </div>
          </div>

          <div className="flex justify-end gap-3 pt-2 border-t border-[var(--color-border)]">
            <Button variant="secondary" onClick={() => setModalAberto(false)}>Cancelar</Button>
            <Button variant="primary" loading={salvando} onClick={salvar}>
              {editando ? 'Salvar alterações' : 'Cadastrar compromisso'}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

function StatusBadge({ isActive }: { isActive: boolean }) {
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${isActive ? 'bg-green-500/15 text-green-400' : 'bg-red-500/15 text-red-400'}`}>
      {isActive ? 'Ativo' : 'Inativo'}
    </span>
  );
}
