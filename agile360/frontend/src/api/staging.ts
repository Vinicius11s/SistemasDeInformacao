import { api, ApiResponse } from './client';
import { TipoPessoa } from '../utils/clienteLabels';

export interface StagingClienteResponse {
  id: string;
  tipo_pessoa: TipoPessoa;
  nome?: string;
  cpf?: string;
  rg?: string;
  orgao_expedidor?: string;
  razao_social?: string;
  cnpj?: string;
  inscricao_estadual?: string;
  email?: string;
  telefone?: string;
  // Por convenção snake_case, pode vir como `whats_app_numero` (algoritmo split) ou
  // `whatsapp_numero` (quando o backend ajusta via JsonPropertyName).
  whats_app_numero?: string;
  whatsapp_numero?: string;
  data_referencia?: string;
  area_atuacao?: string;
  cep?: string;
  estado?: string;
  cidade?: string;
  endereco?: string;
  numero?: string;
  bairro?: string;
  complemento?: string;
  estado_civil?: string;
  numero_conta?: string;
  pix?: string;
  observacoes?: string;
  origem: string;
  origem_mensagem?: string;
  status: 'Pendente' | 'Confirmado' | 'Rejeitado';
  expires_at: string;
  created_at: string;
}

export interface StagingCountResponse {
  pendentes: number;
}

export interface UpdateStagingClientePayload {
  nome_completo?: string;
  cpf?: string;
  rg?: string;
  orgao_expedidor?: string;
  telefone?: string;
  razao_social?: string;
  cnpj?: string;
  inscricao_estadual?: string;
  email?: string;
  // backend aceita `whatsapp_numero`
  whatsapp_numero?: string;
  data_referencia?: string;
  estado_civil?: string;
  area_atuacao?: string;
  cep?: string;
  estado?: string;
  cidade?: string;
  endereco?: string;
  numero?: string;
  bairro?: string;
  complemento?: string;
  numero_conta?: string;
  pix?: string;
  observacoes?: string;
}

export async function listStagingPendentes(
  token: string
): Promise<ApiResponse<StagingClienteResponse[]>> {
  return api.get<StagingClienteResponse[]>('/api/cliente/staging', token);
}

export async function countStagingPendentes(
  token: string
): Promise<ApiResponse<StagingCountResponse>> {
  return api.get<StagingCountResponse>('/api/cliente/staging/count', token);
}

export async function confirmarStaging(
  id: string,
  token: string
): Promise<ApiResponse<unknown>> {
  return api.post<unknown>(`/api/cliente/staging/${id}/confirmar`, {}, token);
}

export async function rejeitarStaging(
  id: string,
  token: string
): Promise<ApiResponse<unknown>> {
  return api.delete<unknown>(`/api/cliente/staging/${id}`, token);
}

export async function editarStaging(
  id: string,
  payload: UpdateStagingClientePayload,
  token: string
): Promise<ApiResponse<StagingClienteResponse>> {
  return api.patch<StagingClienteResponse>(`/api/cliente/staging/${id}`, payload, token);
}
