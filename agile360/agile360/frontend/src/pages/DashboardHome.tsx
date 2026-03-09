import { useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CalendarDays,
  Users,
  AlertTriangle,
  FolderOpen,
  RefreshCw,
  Clock,
  MapPin,
  Gavel,
  Briefcase,
  ArrowRight,
  Scale,
  RotateCcw,
  CheckCircle2,
  XCircle,
} from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import {
  dashboardApi,
  type DashboardResumo,
  type CompromissoDashboard,
  type PrazoDashboard,
} from '../api/dashboard';
import { diasParaVencimento, formatarData } from '../api/prazos';

// ═══════════════════════════════════════════════════════════════════════════
//  Design System v2.0 — Deep Navy · Charcoal · Slate
//  Filosofia: hierarquia por tipografia e peso, não por cor.
//  Cor é usada com parcimônia — apenas para comunicar urgência real.
// ═══════════════════════════════════════════════════════════════════════════

/** Indicadores por tipo de compromisso — paleta sóbria */
const TIPO_CONFIG: Record<string, {
  acento:     string;   // cor da borda esquerda
  textoCor:   string;   // cor do texto do tipo
  horaCor:    string;   // cor da hora
  cardBg:     string;   // fundo do card no calendário
  urgente:    boolean;  // somente Prazo = true
}> = {
  'Audiência':   { acento: '#D95F00', textoCor: '#fdba74', horaCor: '#fb923c', cardBg: 'rgba(217,95,0,.14)',    urgente: false },
  'Atendimento': { acento: '#64748b', textoCor: '#cbd5e1', horaCor: '#94a3b8', cardBg: 'rgba(100,116,139,.16)', urgente: false },
  'Reunião':     { acento: '#7c8fa8', textoCor: '#e2e8f0', horaCor: '#cbd5e1', cardBg: 'rgba(148,163,184,.13)', urgente: false },
  'Prazo':       { acento: '#f87171', textoCor: '#fca5a5', horaCor: '#f87171', cardBg: 'rgba(248,113,113,.14)', urgente: true  },
};

/** Configuração visual por prioridade de prazo */
const PRAZO_PRIORIDADE: Record<string, { borderColor: string; textCor: string }> = {
  Fatal:  { borderColor: '#7f1d1d', textCor: '#fca5a5' },
  Alta:   { borderColor: '#c2410c', textCor: '#fdba74' },
  Normal: { borderColor: 'var(--color-border)', textCor: 'var(--color-text-muted)' },
  Baixa:  { borderColor: 'var(--color-border)', textCor: 'var(--color-text-muted)' },
};

/** Indicadores de status de processo — pontos discretos */
const PROCESSO_STATUS: Record<string, { dot: string; label: string }> = {
  Ativo:     { dot: '#D95F00', label: 'Ativo'     },
  Suspenso:  { dot: '#fbbf24', label: 'Suspenso'  },
  Arquivado: { dot: '#475569', label: 'Arquivado' },
  Encerrado: { dot: '#64748b', label: 'Encerrado' },
};

// ─── Helpers ─────────────────────────────────────────────────────────────────
function getInicioSemana(): Date {
  const hoje = new Date();
  const dia  = hoje.getDay();
  const diff = dia === 0 ? -6 : 1 - dia;
  const seg  = new Date(hoje);
  seg.setDate(hoje.getDate() + diff);
  seg.setHours(0, 0, 0, 0);
  return seg;
}

function formatarDiaSemana(d: Date): { abrev: string; num: number; isHoje: boolean } {
  const hoje = new Date();
  return {
    abrev:  d.toLocaleDateString('pt-BR', { weekday: 'short' }).replace('.', '').toUpperCase(),
    num:    d.getDate(),
    isHoje: d.toDateString() === hoje.toDateString(),
  };
}

function formatarHora(h: string) { return h.slice(0, 5); }

function dataHoje() {
  return new Date().toLocaleDateString('pt-BR', {
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric',
  });
}

// ─── Skeleton ────────────────────────────────────────────────────────────────
function Skeleton({ className = '' }: { className?: string }) {
  return (
    <div
      className={`animate-pulse rounded-[var(--radius)] ${className}`}
      style={{ backgroundColor: 'var(--color-border-strong)' }}
      aria-hidden="true"
    />
  );
}

// ─── Divider ─────────────────────────────────────────────────────────────────
function SectionLabel({ children }: { children: React.ReactNode }) {
  return (
    <p className="label-uppercase mb-3 flex items-center gap-2">
      <span className="inline-block h-px flex-1" style={{ backgroundColor: 'var(--color-border)' }} />
      {children}
      <span className="inline-block h-px flex-1" style={{ backgroundColor: 'var(--color-border)' }} />
    </p>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
//  SummaryCard — card de contador no topo do dashboard
// ═══════════════════════════════════════════════════════════════════════════
interface SummaryCardProps {
  label:      string;
  value:      number;
  icon:       React.ReactNode;
  acento:     string;   // cor da borda esquerda e do ícone
  sublabel?:  string;
  urgente?:   boolean;  // exibe alerta tipográfico quando value > 0
  loading?:   boolean;
}

function SummaryCard({ label, value, icon, acento, sublabel, urgente, loading }: SummaryCardProps) {
  const isAlert = urgente && value > 0;

  if (loading) {
    return (
      <div
        className="rounded-[var(--radius-lg)] p-5"
        style={{
          background:      'var(--color-surface)',
          border:          '1px solid var(--color-border)',
          borderLeft:      `2px solid var(--color-border-strong)`,
          minHeight:       116,
        }}
      >
        <Skeleton className="mb-4 h-7 w-7" />
        <Skeleton className="mb-2 h-8 w-1/3" />
        <Skeleton className="h-3.5 w-2/3" />
      </div>
    );
  }

  return (
    <div
      className="rounded-[var(--radius-lg)] p-5 transition-colors"
      style={{
        background:   'var(--color-surface)',
        border:       `1px solid var(--color-border)`,
        borderLeft:   `2px solid ${isAlert ? '#f87171' : acento}`,
      }}
    >
      {/* Ícone + rótulo de alerta */}
      <div className="mb-4 flex items-center justify-between">
        <span style={{ color: isAlert ? '#f87171' : acento, opacity: 0.9 }}>
          {icon}
        </span>
        {isAlert && (
          <span
            className="flex items-center gap-1 rounded px-1.5 py-0.5 text-[10px] font-semibold"
            style={{
              backgroundColor: 'var(--color-error-bg)',
              color: 'var(--color-error)',
              letterSpacing: '0.04em',
            }}
          >
            <AlertTriangle size={9} />
            ATENÇÃO
          </span>
        )}
      </div>

      {/* Número — hierarquia tipográfica principal */}
      <p
        className="font-bold leading-none"
        style={{
          fontSize:    '2rem',
          color:       isAlert ? 'var(--color-error)' : 'var(--color-text-heading)',
          fontVariantNumeric: 'tabular-nums',
        }}
      >
        {value}
      </p>

      {/* Rótulo */}
      <p
        className="mt-1.5 text-sm font-medium"
        style={{ color: 'var(--color-text)' }}
      >
        {label}
      </p>
      {sublabel && (
        <p className="mt-0.5 text-xs" style={{ color: 'var(--color-text-muted)' }}>
          {sublabel}
        </p>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
//  WeeklyCalendar — calendário Seg–Sex
// ═══════════════════════════════════════════════════════════════════════════
function WeeklyCalendar({
  compromissos,
  loading,
  onClickCompromisso,
}: {
  compromissos: CompromissoDashboard[];
  loading:      boolean;
  onClickCompromisso: (c: CompromissoDashboard) => void;
}) {
  const inicioSemana = getInicioSemana();
  const dias = Array.from({ length: 5 }, (_, i) => {
    const d = new Date(inicioSemana);
    d.setDate(inicioSemana.getDate() + i);
    return d;
  });

  const porDia = (dia: Date): CompromissoDashboard[] => {
    const iso = `${dia.getFullYear()}-${String(dia.getMonth() + 1).padStart(2, '0')}-${String(dia.getDate()).padStart(2, '0')}`;
    return compromissos.filter(c => c.data === iso);
  };

  return (
    <section
      className="rounded-[var(--radius-lg)] overflow-hidden"
      style={{ border: '1px solid var(--color-border)', background: 'var(--color-surface)' }}
      aria-label="Calendário semanal de compromissos"
    >
      {/* Header da seção */}
      <div
        className="flex items-center justify-between px-5 py-3"
        style={{ borderBottom: '1px solid var(--color-border)' }}
      >
        <div className="flex items-center gap-2">
          <CalendarDays size={15} style={{ color: 'var(--color-text-muted)' }} />
          <h2
            className="text-sm font-semibold"
            style={{ color: 'var(--color-text-heading)', letterSpacing: '-0.01em' }}
          >
            Agenda da Semana
          </h2>
        </div>
        <span className="label-uppercase" style={{ color: 'var(--color-text-muted)' }}>
          {inicioSemana.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' })}
          {' – '}
          {dias[4].toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' })}
        </span>
      </div>

      {/* Grid Seg → Sex */}
      <div className="grid grid-cols-5" style={{ borderTop: '1px solid transparent' }}>
        {dias.map((dia, idx) => {
          const { abrev, num, isHoje } = formatarDiaSemana(dia);
          const eventos = porDia(dia);

          return (
            <div
              key={dia.toISOString()}
              className="flex min-h-[190px] flex-col"
              style={{
                borderRight: idx < 4 ? '1px solid var(--color-border)' : undefined,
              }}
            >
              {/* Cabeçalho do dia */}
              <div
                className="flex flex-col items-center py-3"
                style={{
                  borderBottom:    '1px solid var(--color-border)',
                  backgroundColor: isHoje ? 'rgba(37,99,235,.05)' : undefined,
                }}
              >
                <span
                  className="label-uppercase"
                  style={{ color: isHoje ? 'var(--color-primary)' : 'var(--color-text-muted)' }}
                >
                  {abrev}
                </span>
                <span
                  className="mt-1 flex h-6 w-6 items-center justify-center rounded-sm text-sm font-bold"
                  style={
                    isHoje
                      ? { background: 'var(--color-primary)', color: '#fff' }
                      : { color: 'var(--color-text-heading)' }
                  }
                >
                  {num}
                </span>
              </div>

              {/* Eventos */}
              <div className="flex flex-1 flex-col gap-1 p-2">
                {loading ? (
                  <>
                    <Skeleton className="h-11" />
                    <Skeleton className="h-11 opacity-60" />
                  </>
                ) : eventos.length === 0 ? (
                  <p
                    className="mt-3 text-center text-[11px]"
                    style={{ color: 'var(--color-text-muted)' }}
                  >
                    —
                  </p>
                ) : (
                  eventos.map(ev => {
                    const cfg = TIPO_CONFIG[ev.tipo] ?? TIPO_CONFIG['Reunião'];
                    return (
                      <button
                        key={ev.id}
                        onClick={() => onClickCompromisso(ev)}
                        type="button"
                        title={`${ev.tipo} — ${formatarHora(ev.hora)}${ev.local ? ` · ${ev.local}` : ''}`}
                        className="w-full cursor-pointer rounded-[var(--radius)] px-2 py-1.5 text-left transition-colors"
                        style={{
                          background:  cfg.cardBg,
                          borderLeft:  `2px solid ${cfg.acento}`,
                        }}
                        onMouseEnter={e => {
                          (e.currentTarget as HTMLElement).style.filter = 'brightness(1.15)';
                        }}
                        onMouseLeave={e => {
                          (e.currentTarget as HTMLElement).style.filter = '';
                        }}
                      >
                        {/* Hora */}
                        <p
                          className="text-[10px] font-semibold"
                          style={{ color: cfg.horaCor, fontVariantNumeric: 'tabular-nums' }}
                        >
                          {formatarHora(ev.hora)}
                        </p>
                        {/* Tipo */}
                        <p
                          className="mt-0.5 truncate text-[11px]"
                          style={{
                            color:      cfg.textoCor,
                            fontWeight: cfg.urgente ? 600 : 400,
                          }}
                        >
                          {ev.tipo}
                        </p>
                        {/* Local */}
                        {ev.local && (
                          <p
                            className="mt-0.5 flex items-center gap-0.5 truncate text-[10px]"
                            style={{ color: 'var(--color-text-muted)' }}
                          >
                            <MapPin size={7} />
                            {ev.local}
                          </p>
                        )}
                      </button>
                    );
                  })
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* Legenda inline — rodapé do calendário */}
      <div
        className="flex flex-wrap items-center gap-5 px-5 py-2"
        style={{ borderTop: '1px solid var(--color-border)' }}
      >
        <span className="label-uppercase">Tipos:</span>
        {Object.entries(TIPO_CONFIG).map(([tipo, cfg]) => (
          <span
            key={tipo}
            className="flex items-center gap-1.5 text-xs"
            style={{ color: cfg.textoCor }}
          >
            <span
              className="inline-block h-2.5 rounded-sm"
              style={{ width: 2, backgroundColor: cfg.acento }}
            />
            {tipo}
          </span>
        ))}
      </div>
    </section>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
//  RecentProcesses — lista dos processos mais recentes
// ═══════════════════════════════════════════════════════════════════════════
function RecentProcesses({
  processos,
  loading,
  onVerTodos,
  onClickProcesso,
}: {
  processos:       DashboardResumo['processos_recentes'];
  loading:         boolean;
  onVerTodos:      () => void;
  onClickProcesso: (id: string) => void;
}) {
  return (
    <section
      className="rounded-[var(--radius-lg)] overflow-hidden"
      style={{ border: '1px solid var(--color-border)', background: 'var(--color-surface)' }}
      aria-label="Processos recentes"
    >
      {/* Header */}
      <div
        className="flex items-center justify-between px-5 py-3"
        style={{ borderBottom: '1px solid var(--color-border)' }}
      >
        <div className="flex items-center gap-2">
          <Scale size={15} style={{ color: 'var(--color-text-muted)' }} />
          <h2
            className="text-sm font-semibold"
            style={{ color: 'var(--color-text-heading)', letterSpacing: '-0.01em' }}
          >
            Processos Recentes
          </h2>
        </div>
        <button
          onClick={onVerTodos}
          type="button"
          className="flex items-center gap-1 text-xs font-medium transition-colors hover:underline"
          style={{ color: 'var(--color-primary)' }}
        >
          Ver todos
          <ArrowRight size={11} />
        </button>
      </div>

      {/* Linhas */}
      <div>
        {loading
          ? Array.from({ length: 4 }).map((_, i) => (
              <div
                key={i}
                className="flex items-center gap-4 px-5 py-3"
                style={{ borderBottom: '1px solid var(--color-border)' }}
              >
                <Skeleton className="h-2 w-2 shrink-0 rounded-full" />
                <div className="flex-1 space-y-1.5">
                  <Skeleton className="h-3.5 w-1/2" />
                  <Skeleton className="h-3 w-1/3" />
                </div>
                <Skeleton className="h-3 w-14" />
              </div>
            ))
          : processos.length === 0
          ? (
            <div
              className="flex flex-col items-center gap-2 py-12"
              style={{ color: 'var(--color-text-muted)' }}
            >
              <FolderOpen size={28} strokeWidth={1.5} />
              <p className="text-sm">Nenhum processo cadastrado.</p>
            </div>
          )
          : processos.map((p, idx) => {
              const st = PROCESSO_STATUS[p.status] ?? PROCESSO_STATUS.Ativo;
              const isLast = idx === processos.length - 1;
              return (
                <button
                  key={p.id}
                  onClick={() => onClickProcesso(p.id)}
                  type="button"
                  className="flex w-full items-center gap-4 px-5 py-3 text-left transition-colors"
                  style={{
                    borderBottom: isLast ? undefined : '1px solid var(--color-border)',
                  }}
                  onMouseEnter={e => {
                    (e.currentTarget as HTMLElement).style.backgroundColor = 'var(--color-surface-hover)';
                  }}
                  onMouseLeave={e => {
                    (e.currentTarget as HTMLElement).style.backgroundColor = '';
                  }}
                >
                  {/* Ponto de status */}
                  <span
                    className="h-2 w-2 shrink-0 rounded-full"
                    style={{ backgroundColor: st.dot }}
                    title={st.label}
                  />

                  {/* Número e assunto */}
                  <div className="min-w-0 flex-1">
                    <p
                      className="truncate text-sm font-medium"
                      style={{ color: 'var(--color-text-heading)', fontVariantNumeric: 'tabular-nums' }}
                    >
                      {p.num_processo}
                    </p>
                    <p
                      className="mt-0.5 truncate text-xs"
                      style={{ color: 'var(--color-text-muted)' }}
                    >
                      {[p.assunto, p.tribunal].filter(Boolean).join(' · ') || '—'}
                    </p>
                  </div>

                  {/* Status — apenas texto, sem badge colorido */}
                  <span
                    className="shrink-0 text-xs font-medium"
                    style={{ color: 'var(--color-text-secondary)' }}
                  >
                    {st.label}
                  </span>
                </button>
              );
            })}
      </div>
    </section>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
//  UpcomingPrazos — card de prazos urgentes
// ═══════════════════════════════════════════════════════════════════════════
function UpcomingPrazos({
  prazos,
  loading,
  onVerTodos,
}: {
  prazos:    PrazoDashboard[];
  loading:   boolean;
  onVerTodos: () => void;
}) {
  return (
    <section
      className="rounded-[var(--radius-lg)] overflow-hidden"
      style={{ border: '1px solid var(--color-border)', background: 'var(--color-surface)' }}
      aria-label="Próximos prazos"
    >
      {/* Cabeçalho */}
      <div
        className="flex items-center justify-between px-5 py-3"
        style={{ borderBottom: '1px solid var(--color-border)' }}
      >
        <div className="flex items-center gap-2">
          <Clock size={15} style={{ color: 'var(--color-text-muted)' }} strokeWidth={1.5} />
          <h2
            className="text-sm font-semibold"
            style={{ color: 'var(--color-text-heading)', letterSpacing: '-0.01em' }}
          >
            Próximos Prazos
          </h2>
        </div>
        <button
          type="button"
          onClick={onVerTodos}
          className="flex items-center gap-1 text-xs font-medium transition-colors hover:underline"
          style={{ color: 'var(--color-primary)' }}
        >
          Ver todos
          <ArrowRight size={11} />
        </button>
      </div>

      {/* Linhas */}
      <div>
        {loading
          ? Array.from({ length: 4 }).map((_, i) => (
              <div
                key={i}
                className="flex items-center gap-4 px-5 py-3"
                style={{ borderBottom: '1px solid var(--color-border)', borderLeft: '3px solid var(--color-border)' }}
              >
                <div className="flex-1 space-y-1.5">
                  <Skeleton className="h-3.5 w-1/2" />
                  <Skeleton className="h-3 w-1/4" />
                </div>
                <Skeleton className="h-3 w-14" />
              </div>
            ))
          : prazos.length === 0
          ? (
            <div
              className="flex flex-col items-center gap-2 py-10"
              style={{ color: 'var(--color-text-muted)' }}
            >
              <CheckCircle2 size={28} strokeWidth={1.5} />
              <p className="text-sm">Nenhum prazo pendente.</p>
            </div>
          )
          : prazos.map((p, idx) => {
              const dias    = diasParaVencimento(p.data_vencimento);
              const pCfg    = PRAZO_PRIORIDADE[p.prioridade] ?? PRAZO_PRIORIDADE.Normal;
              const isLast  = idx === prazos.length - 1;

              // Label e cor dos dias restantes
              let diasLabel = `${dias}d`;
              let diasCor   = 'var(--color-text-muted)';
              if (dias < 0)  { diasLabel = `Vencido (${Math.abs(dias)}d)`; diasCor = '#fca5a5'; }
              if (dias === 0){ diasLabel = 'Hoje';  diasCor = '#fca5a5'; }
              if (dias === 1){ diasLabel = 'Amanhã'; diasCor = '#fdba74'; }
              if (dias > 1 && dias <= 3) diasCor = '#fdba74';

              return (
                <div
                  key={p.id}
                  className="flex items-center gap-4 px-5 py-3"
                  style={{
                    borderBottom:     isLast ? undefined : '1px solid var(--color-border)',
                    borderLeft:       `3px solid ${pCfg.borderColor}`,
                  }}
                >
                  {/* Ícone de urgência */}
                  <div style={{ color: diasCor, flexShrink: 0 }}>
                    {dias <= 0
                      ? <XCircle size={14} strokeWidth={1.5} />
                      : dias <= 3
                        ? <AlertTriangle size={14} strokeWidth={1.5} />
                        : <Clock size={14} strokeWidth={1.5} />
                    }
                  </div>

                  {/* Título e data */}
                  <div className="min-w-0 flex-1">
                    <p
                      className="truncate text-sm font-medium"
                      style={{ color: 'var(--color-text-heading)' }}
                    >
                      {p.titulo}
                    </p>
                    <p
                      className="mt-0.5 text-xs"
                      style={{ color: 'var(--color-text-muted)' }}
                    >
                      {formatarData(p.data_vencimento)} · {p.prioridade}
                    </p>
                  </div>

                  {/* Dias restantes */}
                  <span
                    className="shrink-0 text-xs font-semibold tabular-nums"
                    style={{ color: diasCor }}
                  >
                    {diasLabel}
                  </span>
                </div>
              );
            })}
      </div>
    </section>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
//  DashboardHome — página principal (v2.0)
// ═══════════════════════════════════════════════════════════════════════════
const AUTO_REFRESH_MS = 60_000;

export function DashboardHome() {
  const { state }  = useAuth();
  const token      = state.token;
  const navigate   = useNavigate();

  const [dados,        setDados]        = useState<DashboardResumo | null>(null);
  const [loading,      setLoading]      = useState(true);
  const [erro,         setErro]         = useState<string | null>(null);
  const [atualizadoEm, setAtualizadoEm] = useState<Date | null>(null);
  const [refreshing,   setRefreshing]   = useState(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const carregar = useCallback(async (silencioso = false) => {
    if (!token) return;
    if (!silencioso) setLoading(true);
    else             setRefreshing(true);
    setErro(null);

    const res = await dashboardApi.resumo(token);
    if (res.success && res.data) {
      setDados(res.data);
      setAtualizadoEm(new Date());
    } else {
      setErro(res.error?.message ?? 'Erro ao carregar o painel.');
    }
    setLoading(false);
    setRefreshing(false);
  }, [token]);

  useEffect(() => { carregar(); }, [carregar]);

  useEffect(() => {
    intervalRef.current = setInterval(() => carregar(true), AUTO_REFRESH_MS);
    return () => { if (intervalRef.current) clearInterval(intervalRef.current); };
  }, [carregar]);

  const irParaCompromisso = (c: CompromissoDashboard) =>
    navigate(c.id_processo ? '/app/processos' : '/app/audiencias');

  // Saudação institucional — sem emojis
  const h = new Date().getHours();
  const saudacao = h < 12 ? 'Bom dia' : h < 18 ? 'Boa tarde' : 'Boa noite';
  const nomeExibido = state.user?.nome ?? 'Advogado';

  const contadores = dados?.contadores;
  const prazosAtivos = (contadores?.prazos_fatais ?? 0) > 0;

  return (
    <div className="mx-auto max-w-[1280px] space-y-6">

      {/* ── Cabeçalho institucional ──────────────────────────── */}
      <div
        className="flex flex-wrap items-start justify-between gap-4 pb-5"
        style={{ borderBottom: '1px solid var(--color-border)' }}
      >
        <div>
          {/* Saudação sem emoji, tipografia com autoridade */}
          <h1
            className="text-xl font-semibold"
            style={{ color: 'var(--color-text-heading)', letterSpacing: '-0.02em' }}
          >
            {saudacao}, {nomeExibido}.
          </h1>
          <p
            className="mt-1 text-sm"
            style={{ color: 'var(--color-text-muted)', textTransform: 'capitalize' }}
          >
            {dataHoje()}
          </p>
        </div>

        {/* Controles de atualização */}
        <div className="flex items-center gap-3">
          {atualizadoEm && (
            <span
              className="flex items-center gap-1.5 text-xs"
              style={{ color: 'var(--color-text-muted)' }}
            >
              <Clock size={11} />
              Atualizado {atualizadoEm.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
            </span>
          )}
          <button
            onClick={() => carregar(true)}
            disabled={refreshing}
            type="button"
            title="Atualizar painel"
            className="flex items-center gap-1.5 rounded-[var(--radius)] px-3 py-1.5 text-xs font-medium transition-colors disabled:opacity-40"
            style={{
              border:     '1px solid var(--color-border-strong)',
              background: 'var(--color-surface)',
              color:      'var(--color-text-secondary)',
            }}
            onMouseEnter={e => {
              (e.currentTarget as HTMLElement).style.borderColor = 'var(--color-primary)';
              (e.currentTarget as HTMLElement).style.color       = 'var(--color-primary)';
            }}
            onMouseLeave={e => {
              (e.currentTarget as HTMLElement).style.borderColor = 'var(--color-border-strong)';
              (e.currentTarget as HTMLElement).style.color       = 'var(--color-text-secondary)';
            }}
          >
            {refreshing
              ? <RotateCcw size={12} className="animate-spin" />
              : <RefreshCw size={12} />
            }
            {refreshing ? 'Atualizando' : 'Atualizar'}
          </button>
        </div>
      </div>

      {/* ── Banner de alerta — Prazos Fatais ─────────────────── */}
      {!loading && prazosAtivos && (
        <div
          className="flex items-start gap-3 rounded-[var(--radius-lg)] px-4 py-3"
          style={{
            border:     '1px solid rgba(248,113,113,.20)',
            background: 'var(--color-error-bg)',
          }}
          role="alert"
        >
          <AlertTriangle
            size={16}
            style={{ color: 'var(--color-error)', marginTop: 1, flexShrink: 0 }}
          />
          <div>
            <p
              className="text-sm font-semibold"
              style={{ color: 'var(--color-error)' }}
            >
              {contadores!.prazos_fatais === 1
                ? '1 prazo fatal nos próximos 3 dias'
                : `${contadores!.prazos_fatais} prazos fatais nos próximos 3 dias`}
            </p>
            <p className="mt-0.5 text-xs" style={{ color: 'var(--color-text-secondary)' }}>
              Verifique a agenda e tome as providências necessárias.
            </p>
          </div>
        </div>
      )}

      {/* ── Erro de carregamento ─────────────────────────────── */}
      {erro && (
        <div
          className="rounded-[var(--radius-lg)] px-4 py-3 text-sm"
          style={{
            border:     '1px solid rgba(248,113,113,.20)',
            background: 'var(--color-error-bg)',
            color:      'var(--color-error)',
          }}
        >
          {erro}
        </div>
      )}

      {/* ── Grid de indicadores ───────────────────────────────── */}
      <div>
        <SectionLabel>Resumo de Hoje</SectionLabel>
        <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
          <SummaryCard
            label="Audiências"
            value={contadores?.audiencias_hoje ?? 0}
            icon={<Gavel size={18} strokeWidth={1.5} />}
            acento="var(--color-accent-audiencia)"
            sublabel="agendadas para hoje"
            loading={loading}
          />
          <SummaryCard
            label="Atendimentos"
            value={contadores?.atendimentos_hoje ?? 0}
            icon={<Users size={18} strokeWidth={1.5} />}
            acento="var(--color-accent-atend)"
            sublabel="agendados para hoje"
            loading={loading}
          />
          <SummaryCard
            label="Prazos Fatais"
            value={contadores?.prazos_fatais ?? 0}
            icon={<AlertTriangle size={18} strokeWidth={1.5} />}
            acento="var(--color-accent-prazo)"
            sublabel="vencimento em até 3 dias"
            urgente
            loading={loading}
          />
          <SummaryCard
            label="Novos Processos"
            value={contadores?.novos_processos_mes ?? 0}
            icon={<Briefcase size={18} strokeWidth={1.5} />}
            acento="var(--color-primary)"
            sublabel="abertos neste mês"
            loading={loading}
          />
        </div>
      </div>

      {/* ── Calendário semanal ────────────────────────────────── */}
      <div>
        <SectionLabel>Agenda Semanal</SectionLabel>
        <WeeklyCalendar
          compromissos={dados?.compromissos_semana ?? []}
          loading={loading}
          onClickCompromisso={irParaCompromisso}
        />
      </div>

      {/* ── Grid inferior: Processos + Prazos ──────────────────── */}
      <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">

        {/* Processos recentes */}
        <div>
          <SectionLabel>Processos</SectionLabel>
          <RecentProcesses
            processos={dados?.processos_recentes ?? []}
            loading={loading}
            onVerTodos={() => navigate('/app/processos')}
            onClickProcesso={() => navigate('/app/processos')}
          />
        </div>

        {/* Próximos prazos */}
        <div>
          <SectionLabel>Prazos Urgentes</SectionLabel>
          <UpcomingPrazos
            prazos={dados?.prazos_proximos ?? []}
            loading={loading}
            onVerTodos={() => navigate('/app/prazos')}
          />
        </div>

      </div>
    </div>
  );
}
