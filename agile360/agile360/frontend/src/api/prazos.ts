import { api } from './client';

// ─── Types ────────────────────────────────────────────────────────────────────

export type PrioridadePrazo  = 'Baixa' | 'Normal' | 'Alta' | 'Fatal';
export type StatusPrazo      = 'Pendente' | 'Concluído' | 'Cancelado';
export type TipoContagemPrazo = 'Util' | 'Corrido';

export interface Prazo {
  id:               string;
  id_advogado:      string;
  id_processo?:     string;
  id_cliente:       string;
  titulo:           string;
  descricao?:       string;
  tipo_prazo?:      string;
  prioridade:       PrioridadePrazo;
  data_publicacao?: string;   // "yyyy-MM-dd"
  data_vencimento:  string;   // "yyyy-MM-dd"
  data_conclusao?:  string;   // ISO 8601
  status:           StatusPrazo;
  tipo_contagem:    TipoContagemPrazo;
  prazo_dias?:      number;
  suspensao_prazos: boolean;
  lembrete_enviado: boolean;
  criado_em?:       string;   // ISO 8601
}

export type CriarPrazoPayload = Omit<Prazo,
  'id' | 'id_advogado' | 'criado_em' | 'lembrete_enviado'>;

export type AtualizarPrazoPayload = CriarPrazoPayload & {
  data_conclusao?: string;
};

// ─── Funções ──────────────────────────────────────────────────────────────────

export const prazosApi = {
  listar: (token: string) =>
    api.get<Prazo[]>('/api/prazos', token),

  obter: (id: string, token: string) =>
    api.get<Prazo>(`/api/prazos/${id}`, token),

  proximos: (token: string, count = 5) =>
    api.get<Prazo[]>(`/api/prazos/proximos?count=${count}`, token),

  criar: (body: CriarPrazoPayload, token: string) =>
    api.post<Prazo>('/api/prazos', body, token),

  atualizar: (id: string, body: AtualizarPrazoPayload, token: string) =>
    api.put<void>(`/api/prazos/${id}`, body, token),

  excluir: (id: string, token: string) =>
    api.delete(`/api/prazos/${id}`, token),
};

// ─── Helpers de cálculo (lado cliente — espelha o RPC do Postgres) ───────────

/**
 * Calcula a data_vencimento com base em data_publicacao + prazo_dias.
 * - 'Corrido': soma os dias diretamente no calendário.
 * - 'Util'   : pula sábados e domingos; se o resultado cair em fim de semana,
 *              avança para a segunda-feira seguinte.
 */
export function calcularDataVencimento(
  dataPublicacao: string,
  prazoDias: number,
  tipoContagem: TipoContagemPrazo = 'Util',
): string {
  // Parsear como UTC para evitar off-by-one por fuso horário
  const [ano, mes, dia] = dataPublicacao.split('-').map(Number);
  const dt = new Date(Date.UTC(ano, mes - 1, dia));

  if (tipoContagem === 'Corrido') {
    dt.setUTCDate(dt.getUTCDate() + prazoDias);
  } else {
    let diasContados = 0;
    while (diasContados < prazoDias) {
      dt.setUTCDate(dt.getUTCDate() + 1);
      const dow = dt.getUTCDay(); // 0=Dom, 6=Sáb
      if (dow !== 0 && dow !== 6) diasContados++;
    }
  }

  // Ajuste final: se caiu em fim de semana, avança para segunda-feira
  const dow = dt.getUTCDay();
  if (dow === 6) dt.setUTCDate(dt.getUTCDate() + 2); // Sáb → Seg
  if (dow === 0) dt.setUTCDate(dt.getUTCDate() + 1); // Dom → Seg

  return dt.toISOString().slice(0, 10); // "yyyy-MM-dd"
}

/**
 * Retorna quantos dias faltam para o vencimento.
 * Valor negativo = vencido.
 */
export function diasParaVencimento(dataVencimento: string): number {
  const hoje = new Date();
  const [ano, mes, dia] = dataVencimento.split('-').map(Number);
  const venc = new Date(Date.UTC(ano, mes - 1, dia));
  const hojeUTC = new Date(Date.UTC(
    hoje.getFullYear(), hoje.getMonth(), hoje.getDate()));
  return Math.round((venc.getTime() - hojeUTC.getTime()) / (1000 * 60 * 60 * 24));
}

/** Formatação dd/MM/yyyy para exibição */
export function formatarData(iso: string): string {
  const [ano, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${ano}`;
}
