import { api } from './client';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface DashboardContadores {
  audiencias_hoje:     number;
  atendimentos_hoje:   number;
  prazos_fatais:       number;   // prazos pendentes nos próximos 3 dias
  novos_processos_mes: number;
}

export interface CompromissoDashboard {
  id:           string;
  tipo:         string;   // 'Audiência' | 'Atendimento' | 'Reunião' | 'Prazo'
  is_active:    boolean;
  data:         string;   // "yyyy-MM-dd"
  hora:         string;   // "HH:mm"
  local?:       string;
  id_processo?: string;
}

export interface ProcessoDashboard {
  id:           string;
  num_processo: string;
  status:       string;
  assunto?:     string;
  tribunal?:    string;
  criado_em?:   string;
}

export interface PrazoDashboard {
  id:            string;
  titulo:        string;
  status:        string;   // 'Pendente' | 'Concluído' | 'Cancelado'
  prioridade:    string;   // 'Baixa' | 'Normal' | 'Alta' | 'Fatal'
  data_vencimento: string; // "yyyy-MM-dd"
  id_processo?:  string;
  id_cliente?:   string;
}

export interface DashboardResumo {
  contadores:           DashboardContadores;
  compromissos_semana:  CompromissoDashboard[];
  processos_recentes:   ProcessoDashboard[];
  prazos_proximos:      PrazoDashboard[];
}

// ─── Funções ──────────────────────────────────────────────────────────────────

export const dashboardApi = {
  resumo: (token: string) =>
    api.get<DashboardResumo>('/api/dashboard/resumo', token),
};
