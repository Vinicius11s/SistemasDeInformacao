import { api, ApiResponse } from './client';

export interface StagingPrazoResponse {
  id: string;
  processo_id?: string;
  cliente_id?: string;
  titulo?: string;
  descricao?: string;
  tipo_prazo?: string;
  prioridade?: string;
  data_vencimento?: string; // YYYY-MM-DD
  data_publicacao?: string; // YYYY-MM-DD
  tipo_contagem?: string;
  prazo_dias?: number;
  suspensao_prazos?: boolean;
  status: 'Pendente' | 'Confirmado' | 'Rejeitado' | string;
  expires_at: string;
  created_at: string;
}

export interface StagingPrazoCountResponse {
  pendentes: number;
}

export interface UpdateStagingPrazoPayload {
  titulo?: string;
  data_vencimento?: string; // YYYY-MM-DD
  prioridade?: string; // Normal | Urgente (triagem)
  tipo_contagem?: string; // Util | Corrido
}

export interface ConfirmarStagingPrazoPayload {
  id_cliente?: string;
  id_processo?: string;
}

export async function listStagingPrazos(
  token: string,
): Promise<ApiResponse<StagingPrazoResponse[]>> {
  return api.get<StagingPrazoResponse[]>('/api/prazo/staging', token);
}

export async function countStagingPrazos(
  token: string,
): Promise<ApiResponse<StagingPrazoCountResponse>> {
  return api.get<StagingPrazoCountResponse>('/api/prazo/staging/count', token);
}

export async function editarStagingPrazo(
  id: string,
  payload: UpdateStagingPrazoPayload,
  token: string,
): Promise<ApiResponse<StagingPrazoResponse>> {
  return api.patch<StagingPrazoResponse>(`/api/prazo/staging/${id}`, payload, token);
}

export async function confirmarStagingPrazo(
  id: string,
  payload: ConfirmarStagingPrazoPayload,
  token: string,
): Promise<ApiResponse<unknown>> {
  return api.post<unknown>(`/api/prazo/staging/${id}/confirmar`, payload, token);
}

export async function rejeitarStagingPrazo(
  id: string,
  token: string,
): Promise<ApiResponse<unknown>> {
  return api.delete(`/api/prazo/staging/${id}`, token);
}

