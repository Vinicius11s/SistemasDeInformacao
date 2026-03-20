import { api, ApiResponse } from './client';

export interface StagingProcessoResponse {
  id: string;
  num_processo?: string;
  parte_contraria?: string;
  tribunal?: string;
  comarca_vara?: string;
  assunto?: string;
  valor_causa?: number;
  honorarios_estimados?: number;
  fase_processual?: string;
  status_processo?: string;
  data_distribuicao?: string; // "YYYY-MM-DD"
  cliente_nome?: string;
  observacoes?: string;
  origem_mensagem?: string;
  status: 'Pendente' | 'Confirmado' | 'Rejeitado';
  expires_at: string;
  created_at: string;
}

export interface StagingProcessoCountResponse {
  pendentes: number;
}

export interface UpdateStagingProcessoPayload {
  num_processo?: string;
  parte_contraria?: string;
  valor_causa?: number;
  tribunal?: string;
  comarca_vara?: string;
  assunto?: string;
}

export interface ConfirmarStagingProcessoPayload {
  id_cliente?: string;
}

export async function listStagingProcessos(
  token: string
): Promise<ApiResponse<StagingProcessoResponse[]>> {
  return api.get<StagingProcessoResponse[]>('/api/processo/staging', token);
}

export async function countStagingProcessos(
  token: string
): Promise<ApiResponse<StagingProcessoCountResponse>> {
  return api.get<StagingProcessoCountResponse>('/api/processo/staging/count', token);
}

export async function rejeitarStagingProcesso(
  id: string,
  token: string
): Promise<ApiResponse<unknown>> {
  return api.delete(`/api/processo/staging/${id}`, token);
}

export async function editarStagingProcesso(
  id: string,
  payload: UpdateStagingProcessoPayload,
  token: string
): Promise<ApiResponse<StagingProcessoResponse>> {
  return api.patch<StagingProcessoResponse>(`/api/processo/staging/${id}`, payload, token);
}

export async function confirmarStagingProcesso(
  id: string,
  payload: ConfirmarStagingProcessoPayload,
  token: string
): Promise<ApiResponse<unknown>> {
  return api.post(`/api/processo/staging/${id}/confirmar`, payload, token);
}

