import { api, ApiResponse } from './client';

export type TipoCompromisso =
  | 'Audiência'
  | 'Atendimento'
  | 'Reunião'
  | 'Prazo';

export interface StagingCompromissoResponse {
  id: string;
  tipo_compromisso?: TipoCompromisso | string;
  tipo_audiencia?: string;
  data?: string; // "YYYY-MM-DD"
  hora?: string; // "HH:mm:ss" (pode vir com timezone dependendo do backend)
  local?: string;
  cliente_nome?: string;
  num_processo?: string;
  observacoes?: string;
  lembrete_minutos?: number;
  origem_mensagem?: string;
  status: 'Pendente' | 'Confirmado' | 'Rejeitado';
  expires_at: string;
  created_at: string;
}

export interface StagingCompromissoCountResponse {
  pendentes: number;
}

export interface UpdateStagingCompromissoPayload {
  tipo_compromisso?: string;
  data?: string; // "YYYY-MM-DD"
  hora?: string; // "HH:mm:ss"
  local?: string;
  lembrete_minutos?: number;
}

export interface ConfirmarStagingCompromissoPayload {
  id_cliente?: string;
  id_processo?: string;
}

export async function listStagingCompromissos(
  token: string
): Promise<ApiResponse<StagingCompromissoResponse[]>> {
  return api.get<StagingCompromissoResponse[]>('/api/compromisso/staging', token);
}

export async function countStagingCompromissos(
  token: string
): Promise<ApiResponse<StagingCompromissoCountResponse>> {
  return api.get<StagingCompromissoCountResponse>('/api/compromisso/staging/count', token);
}

export async function rejeitarStagingCompromisso(
  id: string,
  token: string
): Promise<ApiResponse<unknown>> {
  return api.delete(`/api/compromisso/staging/${id}`, token);
}

export async function editarStagingCompromisso(
  id: string,
  payload: UpdateStagingCompromissoPayload,
  token: string
): Promise<ApiResponse<StagingCompromissoResponse>> {
  return api.patch<StagingCompromissoResponse>(`/api/compromisso/staging/${id}`, payload, token);
}

export async function confirmarStagingCompromisso(
  id: string,
  payload: ConfirmarStagingCompromissoPayload,
  token: string
): Promise<ApiResponse<unknown>> {
  return api.post<unknown>(`/api/compromisso/staging/${id}/confirmar`, payload, token);
}

