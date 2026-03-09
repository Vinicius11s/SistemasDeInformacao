import { useEffect, useMemo, useState } from 'react';
import { Button }   from '../components/Button';
import { Input }    from '../components/Input';
import { Modal }    from '../components/Modal';
import { Combobox } from '../components/Combobox';
import { useToken } from '../context/AuthContext';
import {
  type Processo,
  type CriarProcessoPayload,
  type StatusProcesso,
  processosApi,
} from '../api/processos';
import { clientesApi, type Cliente } from '../api/clientes';

// ─── Estado do formulário ────────────────────────────────────────────────────

type FormState = Omit<CriarProcessoPayload, never>;

const EMPTY_FORM: FormState = {
  id_cliente:           '',
  num_processo:         '',
  status:               'Ativo',
  parte_contraria:      '',
  tribunal:             '',
  comarca_vara:         '',
  assunto:              '',
  valor_causa:          undefined,
  honorarios_estimados: undefined,
  fase_processual:      undefined,
  data_distribuicao:    '',
  observacoes:          '',
};

const STATUS_OPTIONS: StatusProcesso[] = ['Ativo', 'Suspenso', 'Arquivado', 'Encerrado'];
const FASE_OPTIONS = ['Conhecimento', 'Recursal', 'Execução'];

// ─── Componente ───────────────────────────────────────────────────────────────

export function Processos() {
  const token = useToken();

  const [processos, setProcessos]   = useState<Processo[]>([]);
  const [clientes, setClientes]     = useState<Cliente[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erroLista, setErroLista]   = useState<string | null>(null);

  const [modalAberto, setModalAberto] = useState(false);
  const [editando, setEditando]       = useState<Processo | null>(null);
  const [form, setForm]               = useState<FormState>(EMPTY_FORM);
  const [salvando, setSalvando]       = useState(false);
  const [erroForm, setErroForm]       = useState<string | null>(null);

  // ─── Carregar dados ──────────────────────────────────────────────────────
  const carregar = async () => {
    if (!token) return;
    setCarregando(true);
    setErroLista(null);
    const [resP, resC] = await Promise.all([
      processosApi.listar(token),
      clientesApi.listar(token),
    ]);
    if (resP.success) setProcessos(resP.data ?? []);
    else setErroLista(resP.error?.message ?? 'Erro ao carregar processos.');
    if (resC.success) setClientes(resC.data ?? []);
    setCarregando(false);
  };

  useEffect(() => { carregar(); }, [token]);

  const nomeCliente = (id: string) =>
    clientes.find(c => c.id === id)?.nome_completo ?? id.slice(0, 8) + '…';

  const opcoesClientes = useMemo(() =>
    clientes.map(c => ({ value: c.id, label: c.nome_completo, sublabel: c.cpf ?? undefined })),
    [clientes]);

  // ─── Modal ───────────────────────────────────────────────────────────────
  const abrirCriar = () => {
    setEditando(null); setForm(EMPTY_FORM); setErroForm(null); setModalAberto(true);
  };

  const abrirEditar = (p: Processo) => {
    setEditando(p);
    setForm({
      id_cliente:           p.id_cliente,
      num_processo:         p.num_processo,
      status:               p.status,
      parte_contraria:      p.parte_contraria ?? '',
      tribunal:             p.tribunal ?? '',
      comarca_vara:         p.comarca_vara ?? '',
      assunto:              p.assunto ?? '',
      valor_causa:          p.valor_causa,
      honorarios_estimados: p.honorarios_estimados,
      fase_processual:      p.fase_processual,
      data_distribuicao:    p.data_distribuicao ?? '',
      observacoes:          p.observacoes ?? '',
    });
    setErroForm(null);
    setModalAberto(true);
  };

  const salvar = async () => {
    if (!token) return;
    if (!form.id_cliente) { setErroForm('Selecione o cliente.'); return; }
    if (!form.num_processo.trim()) { setErroForm('Número do processo é obrigatório.'); return; }
    setSalvando(true); setErroForm(null);
    try {
      if (editando) {
        const res = await processosApi.atualizar(editando.id, form, token);
        if (!res.success) { setErroForm(res.error?.message ?? 'Erro ao salvar.'); return; }
      } else {
        const res = await processosApi.criar(form, token);
        if (!res.success) { setErroForm(res.error?.message ?? 'Erro ao criar.'); return; }
      }
      setModalAberto(false);
      carregar();
    } finally {
      setSalvando(false);
    }
  };

  const excluir = async (id: string) => {
    if (!token || !confirm('Deseja excluir este processo?')) return;
    await processosApi.excluir(id, token);
    carregar();
  };

  const set = (field: keyof FormState) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setForm(f => ({
        ...f,
        [field]: e.target.type === 'number'
          ? (e.target.value === '' ? undefined : Number(e.target.value))
          : e.target.value || undefined,
      }));

  // ─── Render ─────────────────────────────────────────────────────────────
  return (
    <div>
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-text)] sm:text-2xl">Processos</h1>
        <Button variant="primary" onClick={abrirCriar} className="w-full min-h-[44px] sm:w-auto">+ Novo processo</Button>
      </div>

      {carregando ? (
        <p className="text-[var(--color-text-muted)]">Carregando…</p>
      ) : erroLista ? (
        <p className="text-[var(--color-error)]">{erroLista}</p>
      ) : processos.length === 0 ? (
        <p className="text-[var(--color-text-muted)]">Nenhum processo cadastrado ainda.</p>
      ) : (
        <>
          <div className="flex flex-col gap-3 md:hidden">
            {processos.map(p => (
              <article
                key={p.id}
                className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-4 shadow-sm"
              >
                <p className="font-mono text-sm font-medium text-[var(--color-text-heading)]">{p.num_processo}</p>
                <p className="text-sm text-[var(--color-text-secondary)]">{nomeCliente(p.id_cliente)}</p>
                {(p.assunto || p.tribunal) && (
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {[p.assunto, p.tribunal].filter(Boolean).join(' · ')}
                  </p>
                )}
                <div className="mt-2">
                  <StatusBadge status={p.status} />
                </div>
                <div className="mt-3 flex gap-2">
                  <button type="button" onClick={() => abrirEditar(p)}
                    className="min-h-[44px] min-w-[44px] flex-1 rounded-[var(--radius)] border border-[var(--color-border)] px-4 py-2 text-sm font-medium text-[var(--color-primary)] touch-manipulation">Editar</button>
                  <button type="button" onClick={() => excluir(p.id)}
                    className="min-h-[44px] min-w-[44px] flex-1 rounded-[var(--radius)] border border-[var(--color-border)] px-4 py-2 text-sm font-medium text-[var(--color-error)] touch-manipulation">Excluir</button>
                </div>
              </article>
            ))}
          </div>
          <div className="hidden overflow-x-auto rounded-xl border border-[var(--color-border)] md:block">
            <table className="min-w-full text-sm text-[var(--color-text)]">
              <thead className="bg-[var(--color-surface)] text-[var(--color-text-muted)] text-xs uppercase tracking-wider">
                <tr>
                  {['Nº Processo', 'Cliente', 'Assunto', 'Tribunal', 'Status', 'Ações'].map(h => (
                    <th key={h} className="px-4 py-3 text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--color-border)]">
                {processos.map(p => (
                  <tr key={p.id} className="hover:bg-[var(--color-surface)] transition-colors">
                    <td className="px-4 py-3 font-mono text-xs">{p.num_processo}</td>
                    <td className="px-4 py-3">{nomeCliente(p.id_cliente)}</td>
                    <td className="px-4 py-3">{p.assunto ?? '—'}</td>
                    <td className="px-4 py-3">{p.tribunal ?? '—'}</td>
                    <td className="px-4 py-3"><StatusBadge status={p.status} /></td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        <button onClick={() => abrirEditar(p)} className="text-[var(--color-primary)] hover:underline text-xs">Editar</button>
                        <button onClick={() => excluir(p.id)} className="text-[var(--color-error)] hover:underline text-xs">Excluir</button>
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
      <Modal open={modalAberto} onClose={() => setModalAberto(false)} title={editando ? 'Editar processo' : 'Novo processo'} size="max-w-2xl">
        <div className="flex flex-col gap-4">
          {erroForm && (
            <p className="rounded-lg bg-red-500/10 border border-red-500/30 px-3 py-2 text-sm text-red-400">{erroForm}</p>
          )}

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {/* Cliente */}
            <div className="sm:col-span-2">
              <Combobox
                label="Cliente *"
                options={opcoesClientes}
                value={form.id_cliente ?? ''}
                onChange={val => setForm(f => ({ ...f, id_cliente: val }))}
                placeholder="Busque pelo nome ou CPF…"
                error={erroForm === 'Selecione o cliente.' ? erroForm : undefined}
              />
            </div>

            {/* Número */}
            <div className="sm:col-span-2">
              <Input label="Número do processo *" name="num_processo" value={form.num_processo} onChange={set('num_processo')} placeholder="0000000-00.2026.8.26.0000" />
            </div>

            {/* Status */}
            <div className="flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Status</label>
              <select value={form.status} onChange={set('status')}
                className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none">
                {STATUS_OPTIONS.map(s => <option key={s}>{s}</option>)}
              </select>
            </div>

            {/* Fase processual */}
            <div className="flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Fase processual</label>
              <select value={form.fase_processual ?? ''} onChange={set('fase_processual')}
                className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none">
                <option value="">Selecione…</option>
                {FASE_OPTIONS.map(f => <option key={f}>{f}</option>)}
              </select>
            </div>

            <Input label="Parte contrária" name="parte_contraria" value={form.parte_contraria ?? ''} onChange={set('parte_contraria')} placeholder="Banco do Brasil S/A" />
            <Input label="Tribunal" name="tribunal" value={form.tribunal ?? ''} onChange={set('tribunal')} placeholder="TJSP, TRT2, STF…" />
            <div className="sm:col-span-2">
              <Input label="Comarca / Vara" name="comarca_vara" value={form.comarca_vara ?? ''} onChange={set('comarca_vara')} placeholder="2ª Vara Cível de Presidente Prudente" />
            </div>
            <div className="sm:col-span-2">
              <Input label="Assunto" name="assunto" value={form.assunto ?? ''} onChange={set('assunto')} placeholder="Danos Morais, Reclamação Trabalhista…" />
            </div>
            <Input label="Valor da causa (R$)" name="valor_causa" type="number" min="0" step="0.01"
              value={form.valor_causa ?? ''} onChange={set('valor_causa')} placeholder="0,00" />
            <Input label="Honorários estimados (R$)" name="honorarios_estimados" type="number" min="0" step="0.01"
              value={form.honorarios_estimados ?? ''} onChange={set('honorarios_estimados')} placeholder="0,00" />
            <Input label="Data de distribuição" name="data_distribuicao" type="date" value={form.data_distribuicao ?? ''} onChange={set('data_distribuicao')} />

            <div className="sm:col-span-2 flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Observações</label>
              <textarea rows={3} value={form.observacoes ?? ''} onChange={set('observacoes')}
                placeholder="Cliente solicitou urgência na liminar…"
                className="rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 py-2 text-[var(--color-text)] placeholder:text-[var(--color-text-muted)] focus:border-[var(--color-primary)] focus:outline-none resize-none"
              />
            </div>
          </div>

          <div className="flex justify-end gap-3 pt-2 border-t border-[var(--color-border)]">
            <Button variant="secondary" onClick={() => setModalAberto(false)}>Cancelar</Button>
            <Button variant="primary" loading={salvando} onClick={salvar}>
              {editando ? 'Salvar alterações' : 'Cadastrar processo'}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

// Badge de status colorido
function StatusBadge({ status }: { status: string }) {
  const colors: Record<string, string> = {
    Ativo:     'bg-green-500/15 text-green-400',
    Suspenso:  'bg-yellow-500/15 text-yellow-400',
    Arquivado: 'bg-gray-500/15 text-gray-400',
    Encerrado: 'bg-red-500/15 text-red-400',
  };
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${colors[status] ?? 'bg-gray-500/15 text-gray-400'}`}>
      {status}
    </span>
  );
}
