import { useEffect, useMemo, useState } from 'react';
import {
  Clock,
  AlertTriangle,
  XCircle,
  CalendarDays,
  Info,
} from 'lucide-react';
import { Button }   from '../components/Button';
import { Input }    from '../components/Input';
import { Modal }    from '../components/Modal';
import { Combobox } from '../components/Combobox';
import { useToken } from '../context/AuthContext';
import {
  type Prazo,
  type CriarPrazoPayload,
  type PrioridadePrazo,
  type StatusPrazo,
  type TipoContagemPrazo,
  prazosApi,
  calcularDataVencimento,
  diasParaVencimento,
  formatarData,
} from '../api/prazos';
import { clientesApi, type Cliente }  from '../api/clientes';
import { processosApi, type Processo } from '../api/processos';

// ─── Constantes ───────────────────────────────────────────────────────────────

const PRIORIDADES: PrioridadePrazo[]   = ['Baixa', 'Normal', 'Alta', 'Fatal'];
const STATUS_OPT:  StatusPrazo[]       = ['Pendente', 'Concluído', 'Cancelado'];
const TIPO_CONT:   TipoContagemPrazo[] = ['Util', 'Corrido'];
const TIPOS_PRAZO  = [
  'Recursal', 'Contestação', 'Petição', 'Manifestação',
  'Embargos de Declaração', 'Agravo', 'Apelação', 'Outro',
];

// ─── Estado inicial do formulário ─────────────────────────────────────────────

type FormState = CriarPrazoPayload & { data_conclusao?: string };

const EMPTY_FORM: FormState = {
  id_processo:      undefined,
  id_cliente:       '',
  titulo:           '',
  descricao:        '',
  tipo_prazo:       '',
  prioridade:       'Normal',
  data_publicacao:  '',
  data_vencimento:  '',
  status:           'Pendente',
  tipo_contagem:    'Util',
  prazo_dias:       undefined,
  suspensao_prazos: false,
};

// ─── Helpers visuais ─────────────────────────────────────────────────────────

/**
 * Retorna o estilo de urgência com base nos dias restantes.
 * Usando bordas e texto — sem cores vibrantes.
 */
function urgencia(dataVencimento: string): {
  borderColor: string;
  labelColor:  string;
  label:       string;
  icon:        React.ReactNode;
} {
  const dias = diasParaVencimento(dataVencimento);
  if (dias < 0)
    return { borderColor: '#7f1d1d', labelColor: '#fca5a5', label: `Vencido (${Math.abs(dias)}d)`, icon: <XCircle size={13} /> };
  if (dias === 0)
    return { borderColor: '#b91c1c', labelColor: '#fca5a5', label: 'Vence hoje',  icon: <AlertTriangle size={13} /> };
  if (dias <= 3)
    return { borderColor: '#c2410c', labelColor: '#fdba74', label: `${dias}d`,    icon: <AlertTriangle size={13} /> };
  if (dias <= 10)
    return { borderColor: '#78350f', labelColor: '#fcd34d', label: `${dias}d`,    icon: <Clock size={13} /> };
  return   { borderColor: 'var(--color-border)', labelColor: 'var(--color-text-muted)', label: `${dias}d`, icon: <CalendarDays size={13} /> };
}

function PrioridadeBadge({ p }: { p: string }) {
  const styles: Record<string, string> = {
    Fatal:  'bg-red-500/10 text-red-400 border border-red-500/20',
    Alta:   'bg-orange-500/10 text-orange-400 border border-orange-500/20',
    Normal: 'bg-slate-500/10 text-slate-400 border border-slate-500/20',
    Baixa:  'bg-slate-600/10 text-slate-500 border border-slate-600/20',
  };
  return (
    <span className={`inline-block rounded-[var(--radius)] px-2 py-0.5 text-xs font-medium ${styles[p] ?? styles['Normal']}`}>
      {p}
    </span>
  );
}

function StatusBadge({ s }: { s: string }) {
  const styles: Record<string, string> = {
    Pendente:   'bg-orange-500/10 text-orange-400 border border-orange-500/20',
    'Concluído':'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20',
    Cancelado:  'bg-slate-500/10 text-slate-400 border border-slate-500/20',
  };
  return (
    <span className={`inline-block rounded-[var(--radius)] px-2 py-0.5 text-xs font-medium ${styles[s] ?? styles['Pendente']}`}>
      {s}
    </span>
  );
}

// ─── Componente principal ────────────────────────────────────────────────────

export function Prazos() {
  const token = useToken();

  const [lista,        setLista]        = useState<Prazo[]>([]);
  const [clientes,     setClientes]     = useState<Cliente[]>([]);
  const [processos,    setProcessos]    = useState<Processo[]>([]);
  const [carregando,   setCarregando]   = useState(true);
  const [modalAberto,  setModalAberto]  = useState(false);
  const [editando,     setEditando]     = useState<Prazo | null>(null);
  const [form,         setForm]         = useState<FormState>(EMPTY_FORM);
  const [salvando,     setSalvando]     = useState(false);
  const [erroForm,     setErroForm]     = useState<string | null>(null);
  const [filtroStatus, setFiltroStatus] = useState<StatusPrazo | 'Todos'>('Todos');

  // ── Opções dos Comboboxes ────────────────────────────────────────────────
  const opcoesClientes = useMemo(() =>
    clientes.map(c => ({
      value:    c.id,
      label:    c.nome_completo,
      sublabel: c.cpf ?? undefined,
    })), [clientes]);

  const opcoesProcessos = useMemo(() =>
    processos.map(p => ({
      value:    p.id,
      label:    p.num_processo,
      sublabel: p.assunto ?? undefined,
    })), [processos]);

  // Cliente é readonly quando um processo está selecionado (inteligência relacional)
  const clienteReadonly = !!form.id_processo;

  // ── Carregar dados ───────────────────────────────────────────────────────
  async function carregar() {
    if (!token) return;
    setCarregando(true);
    const [rPrazos, rClientes, rProcessos] = await Promise.all([
      prazosApi.listar(token),
      clientesApi.listar(token),
      processosApi.listar(token),
    ]);
    if (rPrazos.success)    setLista(rPrazos.data     ?? []);
    if (rClientes.success)  setClientes(rClientes.data ?? []);
    if (rProcessos.success) setProcessos(rProcessos.data ?? []);
    setCarregando(false);
  }

  useEffect(() => { void carregar(); }, []);   // eslint-disable-line react-hooks/exhaustive-deps

  // ── Helpers de formulário ────────────────────────────────────────────────
  function set<K extends keyof FormState>(key: K) {
    return (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
      // Inputs numéricos são sempre convertidos para number (evita string-concat no cálculo)
      const value =
        e.target.type === 'checkbox'
          ? (e.target as HTMLInputElement).checked
          : e.target.type === 'number'
          ? (e.target.value === '' ? undefined : Number(e.target.value))
          : (e.target.value === '' ? undefined : e.target.value);

      setForm(prev => {
        const next = { ...prev, [key]: value };

        // ── Auto-calcular data_vencimento ────────────────────────────────
        // Dispara sempre que qualquer uma das três entradas muda.
        // Garante que prazo_dias seja number (Number coerce string ou undefined→NaN).
        if (key === 'data_publicacao' || key === 'prazo_dias' || key === 'tipo_contagem') {
          const pub  = key === 'data_publicacao' ? (value as string | undefined) : prev.data_publicacao;
          const dias = Number(key === 'prazo_dias' ? value : prev.prazo_dias);   // ← sempre number
          const tipo = (key === 'tipo_contagem' ? value : prev.tipo_contagem) as TipoContagemPrazo;

          if (pub && Number.isFinite(dias) && dias > 0) {
            next.data_vencimento = calcularDataVencimento(pub, dias, tipo);
          }
        }
        return next;
      });
    };
  }

  // ── Handler de processo: preenche cliente automaticamente ────────────────
  /** @architect — regra: processo selecionado → cliente vinculado é obrigatório */
  function handleProcessoChange(processoId: string) {
    setForm(prev => {
      const next = { ...prev, id_processo: processoId || undefined };
      if (processoId) {
        const proc = processos.find(p => p.id === processoId);
        if (proc?.id_cliente) {
          next.id_cliente = proc.id_cliente;   // ← auto-fill
        }
      }
      // Ao limpar o processo, o cliente volta a ser editável (mantém o valor)
      return next;
    });
  }

  // ── Abrir modal ──────────────────────────────────────────────────────────
  function abrirCriar() {
    setEditando(null);
    setForm(EMPTY_FORM);
    setErroForm(null);
    setModalAberto(true);
  }

  function abrirEditar(p: Prazo) {
    setEditando(p);
    setForm({
      id_processo:      p.id_processo,
      id_cliente:       p.id_cliente,
      titulo:           p.titulo,
      descricao:        p.descricao ?? '',
      tipo_prazo:       p.tipo_prazo ?? '',
      prioridade:       p.prioridade,
      data_publicacao:  p.data_publicacao ?? '',
      data_vencimento:  p.data_vencimento,
      status:           p.status,
      tipo_contagem:    p.tipo_contagem,
      prazo_dias:       p.prazo_dias,
      suspensao_prazos: p.suspensao_prazos,
      data_conclusao:   p.data_conclusao ?? '',
    });
    setErroForm(null);
    setModalAberto(true);
  }

  // ── Salvar ───────────────────────────────────────────────────────────────
  async function salvar() {
    if (!token) return;
    if (!form.titulo.trim()) { setErroForm('O título é obrigatório.'); return; }
    if (!form.id_cliente)    { setErroForm('Selecione um cliente.');   return; }
    if (!form.data_vencimento) { setErroForm('A data de vencimento é obrigatória.'); return; }

    setSalvando(true);
    setErroForm(null);

    const payload: CriarPrazoPayload = {
      id_processo:      form.id_processo || undefined,
      id_cliente:       form.id_cliente!,
      titulo:           form.titulo,
      descricao:        form.descricao || undefined,
      tipo_prazo:       form.tipo_prazo || undefined,
      prioridade:       form.prioridade,
      data_publicacao:  form.data_publicacao || undefined,
      data_vencimento:  form.data_vencimento,
      status:           form.status,
      tipo_contagem:    form.tipo_contagem,
      prazo_dias:       form.prazo_dias ? Number(form.prazo_dias) : undefined,
      suspensao_prazos: form.suspensao_prazos,
    };

    let res;
    if (editando) {
      res = await prazosApi.atualizar(editando.id, {
        ...payload,
        data_conclusao: form.data_conclusao || undefined,
      }, token);
    } else {
      res = await prazosApi.criar(payload, token);
    }

    if (!res.success) {
      setErroForm(res.error?.message ?? 'Erro ao salvar.');
    } else {
      setModalAberto(false);
      await carregar();
    }
    setSalvando(false);
  }

  // ── Excluir ──────────────────────────────────────────────────────────────
  async function excluir(p: Prazo) {
    if (!token) return;
    if (!confirm(`Excluir o prazo "${p.titulo}"? Esta ação não pode ser desfeita.`)) return;
    const res = await prazosApi.excluir(p.id, token);
    if (!res.success) {
      alert(res.error?.message ?? 'Erro ao excluir.');
    } else {
      await carregar();
    }
  }

  // ── Lista filtrada ───────────────────────────────────────────────────────
  const listaFiltrada = filtroStatus === 'Todos'
    ? lista
    : lista.filter(p => p.status === filtroStatus);

  // Ordena: Fatais/Alta primeiro, depois por vencimento
  const listaOrdenada = [...listaFiltrada].sort((a, b) => {
    const peso = (p: string) => p === 'Fatal' ? 0 : p === 'Alta' ? 1 : p === 'Normal' ? 2 : 3;
    if (peso(a.prioridade) !== peso(b.prioridade))
      return peso(a.prioridade) - peso(b.prioridade);
    return a.data_vencimento.localeCompare(b.data_vencimento);
  });

  // ── Render ───────────────────────────────────────────────────────────────
  return (
    <div className="mx-auto max-w-7xl space-y-5">

      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
    <div>
          <h1
            className="text-xl font-bold tracking-tight"
            style={{ color: 'var(--color-text-heading)' }}
          >
            Prazos
          </h1>
          <p className="text-sm" style={{ color: 'var(--color-text-muted)' }}>
            {lista.filter(p => p.status === 'Pendente').length} pendentes ·{' '}
            {lista.filter(p => diasParaVencimento(p.data_vencimento) <= 3 && p.status === 'Pendente').length} vencem em até 3 dias
          </p>
        </div>
        <Button variant="primary" onClick={abrirCriar}>+ Novo prazo</Button>
      </div>

      {/* Filtros de status */}
      <div className="flex flex-wrap gap-2">
        {(['Todos', 'Pendente', 'Concluído', 'Cancelado'] as const).map(s => (
          <button
            key={s}
            type="button"
            onClick={() => setFiltroStatus(s)}
            className="rounded-[var(--radius)] border px-3 py-1.5 text-xs font-medium transition-colors"
            style={filtroStatus === s
              ? { background: 'var(--color-primary)', borderColor: 'var(--color-primary)', color: '#fff' }
              : { background: 'transparent', borderColor: 'var(--color-border)', color: 'var(--color-text-secondary)' }
            }
          >
            {s === 'Todos' ? `Todos (${lista.length})` : `${s} (${lista.filter(p => p.status === s).length})`}
          </button>
        ))}
      </div>

      {/* Tabela */}
      {carregando ? (
        <div className="py-16 text-center text-sm" style={{ color: 'var(--color-text-muted)' }}>
          Carregando prazos…
        </div>
      ) : listaOrdenada.length === 0 ? (
        <div
          className="rounded-[var(--radius-lg)] border py-16 text-center text-sm"
          style={{ borderColor: 'var(--color-border)', color: 'var(--color-text-muted)' }}
        >
          Nenhum prazo encontrado. Cadastre o primeiro prazo clicando em "+ Novo prazo".
        </div>
      ) : (
        <div
          className="overflow-x-auto rounded-[var(--radius-lg)] border"
          style={{ borderColor: 'var(--color-border)', background: 'var(--color-surface)' }}
        >
          <table className="w-full text-sm">
            <thead>
              <tr style={{ borderBottom: '1px solid var(--color-border)' }}>
                {['Título', 'Cliente', 'Vencimento', 'Dias', 'Prioridade', 'Status', 'Ações'].map(h => (
                  <th
                    key={h}
                    className="label-uppercase px-4 py-3 text-left"
                    style={{ color: 'var(--color-text-muted)' }}
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {listaOrdenada.map((p, i) => {
                const urg = urgencia(p.data_vencimento);
                const cliente = clientes.find(c => c.id === p.id_cliente);
                return (
                  <tr
                    key={p.id}
                    style={{
                      borderTop:       i > 0 ? '1px solid var(--color-border)' : undefined,
                      borderLeft:      `3px solid ${urg.borderColor}`,
                    }}
                  >
                    {/* Título */}
                    <td className="px-4 py-3">
                      <div className="font-medium" style={{ color: 'var(--color-text-heading)' }}>
                        {p.titulo}
                      </div>
                      {p.tipo_prazo && (
                        <div className="text-xs" style={{ color: 'var(--color-text-muted)' }}>
                          {p.tipo_prazo}
                        </div>
                      )}
                    </td>

                    {/* Cliente */}
                    <td className="px-4 py-3" style={{ color: 'var(--color-text-secondary)' }}>
                      {cliente?.nome_completo ?? p.id_cliente.slice(0, 8) + '…'}
                    </td>

                    {/* Vencimento */}
                    <td className="px-4 py-3">
                      <div className="font-medium" style={{ color: 'var(--color-text)' }}>
                        {formatarData(p.data_vencimento)}
                      </div>
                    </td>

                    {/* Dias restantes */}
                    <td className="px-4 py-3">
                      <span className="flex items-center gap-1 text-xs" style={{ color: urg.labelColor }}>
                        {urg.icon}
                        {urg.label}
                      </span>
                    </td>

                    {/* Prioridade */}
                    <td className="px-4 py-3">
                      <PrioridadeBadge p={p.prioridade} />
                    </td>

                    {/* Status */}
                    <td className="px-4 py-3">
                      <StatusBadge s={p.status} />
                    </td>

                    {/* Ações */}
                    <td className="px-4 py-3">
                      <div className="flex gap-3 text-xs">
                        <button
                          type="button"
                          onClick={() => abrirEditar(p)}
                          style={{ color: 'var(--color-primary)' }}
                          className="hover:underline"
                        >
                          Editar
                        </button>
                        <button
                          type="button"
                          onClick={() => excluir(p)}
                          style={{ color: 'var(--color-error)' }}
                          className="hover:underline"
                        >
                          Excluir
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* ── Modal de cadastro/edição ─────────────────────────────────────── */}
      <Modal
        open={modalAberto}
        onClose={() => setModalAberto(false)}
        title={editando ? 'Editar prazo' : 'Novo prazo'}
        size="max-w-2xl"
      >
        <div className="flex flex-col gap-4">
          {erroForm && (
            <p
              className="rounded-[var(--radius)] border px-3 py-2 text-sm"
              style={{
                background:   'var(--color-error-bg)',
                borderColor:  'var(--color-error)',
                color:        'var(--color-error)',
              }}
            >
              {erroForm}
            </p>
          )}

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">

            {/* Título */}
            <div className="sm:col-span-2">
              <Input
                label="Título *"
                name="titulo"
                value={form.titulo}
                onChange={set('titulo')}
                placeholder="Ex.: Contestação — Processo 0001234-10.2026.8.26.0100"
              />
            </div>

            {/* Processo (opcional — preenche o Cliente automaticamente) */}
            <Combobox
              label="Processo (opcional)"
              options={opcoesProcessos}
              value={form.id_processo ?? ''}
              onChange={handleProcessoChange}
              placeholder="Busque pelo número do processo…"
            />

            {/* Cliente — readonly quando processo selecionado */}
            <Combobox
              label="Cliente *"
              options={opcoesClientes}
              value={form.id_cliente ?? ''}
              onChange={val => setForm(f => ({ ...f, id_cliente: val }))}
              placeholder="Busque pelo nome ou CPF…"
              readonly={clienteReadonly}
              readonlyHint={clienteReadonly ? '— preenchido automaticamente' : undefined}
              error={erroForm === 'Selecione um cliente.' ? erroForm : undefined}
            />

            {/* Tipo do prazo */}
            <div className="flex flex-col gap-1">
              <label className="text-sm" style={{ color: 'var(--color-text-muted)' }}>
                Tipo do prazo
              </label>
              <select
                value={form.tipo_prazo ?? ''}
                onChange={set('tipo_prazo')}
                className="min-h-[44px] rounded-[var(--radius)] border px-3 text-sm focus:outline-none"
                style={{
                  background:  'var(--color-surface-elevated)',
                  borderColor: 'var(--color-border)',
                  color:       'var(--color-text)',
                }}
              >
                <option value="">Selecione…</option>
                {TIPOS_PRAZO.map(t => <option key={t} value={t}>{t}</option>)}
              </select>
            </div>

            {/* Prioridade */}
            <div className="flex flex-col gap-1">
              <label className="text-sm" style={{ color: 'var(--color-text-muted)' }}>
                Prioridade
              </label>
              <select
                value={form.prioridade}
                onChange={set('prioridade')}
                className="min-h-[44px] rounded-[var(--radius)] border px-3 text-sm focus:outline-none"
                style={{
                  background:  'var(--color-surface-elevated)',
                  borderColor: 'var(--color-border)',
                  color:       'var(--color-text)',
                }}
              >
                {PRIORIDADES.map(p => <option key={p} value={p}>{p}</option>)}
              </select>
            </div>

            {/* Separador — Cálculo automático de data */}
            <div className="sm:col-span-2">
              <div
                className="flex items-center gap-2 rounded-[var(--radius)] border px-3 py-2 text-xs"
                style={{
                  borderColor: 'var(--color-border)',
                  color:       'var(--color-text-muted)',
                  background:  'var(--color-surface-elevated)',
                }}
              >
                <Info size={12} />
                Defina o <strong>Tipo de contagem</strong>, depois preencha{' '}
                <strong>Data de publicação</strong> e <strong>Prazo (dias)</strong> para calcular a
                data de vencimento automaticamente.
              </div>
            </div>

            {/* 1. Tipo de contagem — deve ser definido ANTES da publicação/dias */}
            <div className="flex flex-col gap-1">
              <label className="text-sm" style={{ color: 'var(--color-text-muted)' }}>
                Tipo de contagem
              </label>
              <select
                value={form.tipo_contagem}
                onChange={set('tipo_contagem')}
                className="min-h-[44px] rounded-[var(--radius)] border px-3 text-sm focus:outline-none"
                style={{
                  background:  'var(--color-surface-elevated)',
                  borderColor: 'var(--color-border)',
                  color:       'var(--color-text)',
                }}
              >
                {TIPO_CONT.map(t => (
                  <option key={t} value={t}>{t === 'Util' ? 'Dias úteis' : 'Dias corridos'}</option>
                ))}
              </select>
            </div>

            {/* 2. Data publicação */}
            <Input
              label="Data de publicação"
              name="data_publicacao"
              type="date"
              value={form.data_publicacao ?? ''}
              onChange={set('data_publicacao')}
            />

            {/* 3. Prazo em dias */}
            <Input
              label="Prazo (dias)"
              name="prazo_dias"
              type="number"
              min="1"
              value={form.prazo_dias ?? ''}
              onChange={set('prazo_dias')}
              placeholder="Ex.: 15"
            />

            {/* 4. Data vencimento — calculada automaticamente ou preenchível manualmente */}
            <div>
              <Input
                label="Data de vencimento *"
                name="data_vencimento"
                type="date"
                value={form.data_vencimento ?? ''}
                onChange={set('data_vencimento')}
              />
              {form.data_vencimento && (
                <p className="mt-1 text-xs" style={{ color: 'var(--color-text-muted)' }}>
                  {formatarData(form.data_vencimento)}
                  {' · '}
                  {(() => {
                    const d = diasParaVencimento(form.data_vencimento);
                    if (d < 0)   return `vencido há ${Math.abs(d)} dia${Math.abs(d) !== 1 ? 's' : ''}`;
                    if (d === 0) return 'vence hoje';
                    return `faltam ${d} dia${d !== 1 ? 's' : ''}`;
                  })()}
                </p>
              )}
            </div>

            {/* Status */}
            <div className="flex flex-col gap-1">
              <label className="text-sm" style={{ color: 'var(--color-text-muted)' }}>
                Status
              </label>
              <select
                value={form.status}
                onChange={set('status')}
                className="min-h-[44px] rounded-[var(--radius)] border px-3 text-sm focus:outline-none"
                style={{
                  background:  'var(--color-surface-elevated)',
                  borderColor: 'var(--color-border)',
                  color:       'var(--color-text)',
                }}
              >
                {STATUS_OPT.map(s => <option key={s} value={s}>{s}</option>)}
              </select>
            </div>

            {/* Data conclusão — só aparece ao concluir */}
            {form.status === 'Concluído' && (
              <Input
                label="Data de conclusão"
                name="data_conclusao"
                type="date"
                value={form.data_conclusao ?? ''}
                onChange={set('data_conclusao')}
              />
            )}

            {/* Suspensão de prazos */}
            <div className="flex items-center gap-2 sm:col-span-2">
              <input
                id="suspensao"
                type="checkbox"
                checked={form.suspensao_prazos}
                onChange={set('suspensao_prazos')}
                className="h-4 w-4 rounded accent-[var(--color-primary)]"
              />
              <label htmlFor="suspensao" className="text-sm" style={{ color: 'var(--color-text)' }}>
                Prazo suspenso (ex.: recesso forense, férias coletivas)
              </label>
            </div>

            {/* Descrição */}
            <div className="flex flex-col gap-1 sm:col-span-2">
              <label className="text-sm" style={{ color: 'var(--color-text-muted)' }}>
                Descrição / Observações
              </label>
              <textarea
                rows={3}
                value={form.descricao ?? ''}
                onChange={set('descricao')}
                placeholder="Detalhes sobre o prazo, intimação, etc."
                className="rounded-[var(--radius)] border px-3 py-2 text-sm focus:outline-none resize-none"
                style={{
                  background:  'var(--color-surface-elevated)',
                  borderColor: 'var(--color-border)',
                  color:       'var(--color-text)',
                }}
              />
            </div>
          </div>

          {/* Rodapé do modal */}
          <div
            className="flex justify-end gap-3 border-t pt-3"
            style={{ borderColor: 'var(--color-border)' }}
          >
            <Button variant="secondary" onClick={() => setModalAberto(false)}>
              Cancelar
            </Button>
            <Button variant="primary" loading={salvando} onClick={salvar}>
              {editando ? 'Salvar alterações' : 'Cadastrar prazo'}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
