import { api } from './client';

// ─── Types ────────────────────────────────────────────────────────────────────

export type StatusProcesso = 'Ativo' | 'Suspenso' | 'Arquivado' | 'Encerrado';
export type FaseProcessual  = 'Conhecimento' | 'Recursal' | 'Execução';

export interface Processo {
  id: string;
  id_advogado: string;
  id_cliente: string;
  num_processo: string;
  status: StatusProcesso;
  parte_contraria?: string;
  tribunal?: string;
  comarca_vara?: string;
  assunto?: string;
  valor_causa?: number;
  honorarios_estimados?: number;
  fase_processual?: FaseProcessual;
  data_distribuicao?: string;   // "YYYY-MM-DD"
  observacoes?: string;
  criado_em?: string;           // "YYYY-MM-DD"
}

export type CriarProcessoPayload = Omit<Processo, 'id' | 'id_advogado' | 'criado_em'>;
export type AtualizarProcessoPayload = CriarProcessoPayload;

// ─── Funções ──────────────────────────────────────────────────────────────────

export const processosApi = {
  listar: (token: string) =>
    api.get<Processo[]>('/api/processos', token),

  obter: (id: string, token: string) =>
    api.get<Processo>(`/api/processos/${id}`, token),

  criar: (body: CriarProcessoPayload, token: string) =>
    api.post<Processo>('/api/processos', body, token),

  atualizar: (id: string, body: AtualizarProcessoPayload, token: string) =>
    api.put<void>(`/api/processos/${id}`, body, token),

  excluir: (id: string, token: string) =>
    api.delete(`/api/processos/${id}`, token),
};
