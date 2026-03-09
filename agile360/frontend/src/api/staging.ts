import { api, ApiResponse } from './client';
import { TipoPessoa } from '../utils/clienteLabels';

export interface StagingClienteResponse {
  id: string;
  tipoPessoa: TipoPessoa;
  nome?: string;
  cpf?: string;
  rg?: string;
  orgaoExpedidor?: string;
  razaoSocial?: string;
  cnpj?: string;
  inscricaoEstadual?: string;
  email?: string;
  telefone?: string;
  whatsAppNumero?: string;
  dataReferencia?: string;
  areaAtuacao?: string;
  endereco?: string;
  observacoes?: string;
  origem: string;
  origemMensagem?: string;
  status: 'Pendente' | 'Confirmado' | 'Rejeitado';
  expiresAt: string;
  createdAt: string;
}

export interface StagingCountResponse {
  pendentes: number;
}

export async function listStagingPendentes(
  token: string
): Promise<ApiResponse<StagingClienteResponse[]>> {
  return api.get<StagingClienteResponse[]>('/api/clientes/staging', token);
}

export async function countStagingPendentes(
  token: string
): Promise<ApiResponse<StagingCountResponse>> {
  return api.get<StagingCountResponse>('/api/clientes/staging/count', token);
}

export async function confirmarStaging(
  id: string,
  token: string
): Promise<ApiResponse<unknown>> {
  return api.post<unknown>(`/api/clientes/staging/${id}/confirmar`, {}, token);
}

export async function rejeitarStaging(
  id: string,
  token: string
): Promise<ApiResponse<unknown>> {
  return api.delete<unknown>(`/api/clientes/staging/${id}`, token);
}
