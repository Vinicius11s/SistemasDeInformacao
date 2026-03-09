import { api } from './client';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface Cliente {
  id: string;
  id_advogado: string;
  tipo_cliente?: string;      // 'Pessoa Física' | 'Pessoa Jurídica'
  nome_completo: string;
  cpf?: string;
  rg?: string;
  orgao_expedidor?: string;
  data_nascimento?: string;   // "YYYY-MM-DD"
  estado_civil?: string;
  profissao?: string;
  telefone?: string;
  numero_conta?: string;
  pix?: string;
  cep?: string;
  endereco?: string;
  numero?: string;
  bairro?: string;
  complemento?: string;
  cidade?: string;
  estado?: string;            // 2 chars, ex.: "SP"
  is_active?: boolean;
  data_cadastro?: string;     // "YYYY-MM-DD"
}

export type CriarClientePayload = Omit<Cliente, 'id' | 'id_advogado' | 'data_cadastro'>;
export type AtualizarClientePayload = CriarClientePayload;

export interface ImportarClientesResult {
  total: number;
  sucesso: number;
  falhas: number;
  erros: { linha: number; nome_completo: string; motivo: string }[];
}

// ─── Funções ──────────────────────────────────────────────────────────────────

export const clientesApi = {
  listar: (token: string) =>
    api.get<Cliente[]>('/api/clientes', token),

  obter: (id: string, token: string) =>
    api.get<Cliente>(`/api/clientes/${id}`, token),

  criar: (body: CriarClientePayload, token: string) =>
    api.post<Cliente>('/api/clientes', body, token),

  atualizar: (id: string, body: AtualizarClientePayload, token: string) =>
    api.put<void>(`/api/clientes/${id}`, body, token),

  excluir: (id: string, token: string) =>
    api.delete(`/api/clientes/${id}`, token),

  /**
   * Baixa o arquivo modelo (.xlsx) para preenchimento.
   * Usa fetch com Authorization header (não window.open, que não envia tokens).
   * O download é disparado programaticamente via Blob URL.
   */
  downloadTemplate: async (token: string): Promise<void> => {
    const res = await fetch('/api/clientes/template', {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!res.ok) throw new Error(`Erro ${res.status} ao baixar o modelo.`);
    const blob = await res.blob();
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href     = url;
    a.download = 'modelo_clientes.xlsx';
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
  },

  /** Faz upload de planilha .xlsx para importação em massa */
  importar: (file: File, token: string) => {
    const form = new FormData();
    form.append('planilha', file);
    return api.postForm<ImportarClientesResult>('/api/clientes/importar', form, token);
  },
};
