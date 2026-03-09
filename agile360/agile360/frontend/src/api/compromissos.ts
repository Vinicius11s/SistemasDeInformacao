import { api } from './client';

// ─── Types ────────────────────────────────────────────────────────────────────

export type TipoCompromisso  = 'Audiência' | 'Atendimento' | 'Reunião' | 'Prazo';
export type StatusCompromisso = 'Agendado' | 'Concluído' | 'Cancelado';

export interface Compromisso {
  id: string;
  id_advogado: string;
  tipo_compromisso: TipoCompromisso;
  tipo_audiencia?: string;
  status: StatusCompromisso;
  data: string;              // "YYYY-MM-DD"
  hora: string;              // "HH:mm:ss"
  local?: string;
  id_cliente?: string;
  id_processo?: string;
  observacoes?: string;
  lembrete_minutos?: number;
  criado_em?: string;        // "YYYY-MM-DD"
}

export type CriarCompromissoPayload = Omit<Compromisso, 'id' | 'id_advogado' | 'criado_em'>;
export type AtualizarCompromissoPayload = CriarCompromissoPayload;

// ─── Funções ──────────────────────────────────────────────────────────────────

export const compromissosApi = {
  listar: (token: string) =>
    api.get<Compromisso[]>('/api/compromissos', token),

  obter: (id: string, token: string) =>
    api.get<Compromisso>(`/api/compromissos/${id}`, token),

  criar: (body: CriarCompromissoPayload, token: string) =>
    api.post<Compromisso>('/api/compromissos', body, token),

  atualizar: (id: string, body: AtualizarCompromissoPayload, token: string) =>
    api.put<void>(`/api/compromissos/${id}`, body, token),

  excluir: (id: string, token: string) =>
    api.delete(`/api/compromissos/${id}`, token),
};
