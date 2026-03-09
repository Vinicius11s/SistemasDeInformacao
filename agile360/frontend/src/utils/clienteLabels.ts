/**
 * clienteLabels.ts
 *
 * Single source of truth for all labels and placeholders that change
 * based on TipoPessoa. The UI reads from this map — never hardcodes strings.
 *
 * Rationale: the database stores both PF and PJ data in the same two columns
 * (data_referencia, atividade). TipoPessoa is the semantic key that tells the
 * interface — and reports — how to label and interpret each value.
 */

export type TipoPessoa = 'PessoaFisica' | 'PessoaJuridica';

export interface ClienteFieldLabels {
  /** Main identifier field label */
  nomeLabel: string;
  nomePlaceholder: string;
  /** Date field */
  dataLabel: string;
  dataPlaceholder: string;
  /** Occupation / sector field */
  areaAtuacaoLabel: string;
  areaAtuacaoPlaceholder: string;
  /** Column header used in tables / dashboards */
  documentoHeader: string;
  dataHeader: string;
  areaAtuacaoHeader: string;
}

const LABELS: Record<TipoPessoa, ClienteFieldLabels> = {
  PessoaFisica: {
    nomeLabel:               'Nome completo *',
    nomePlaceholder:         'Ex.: João da Silva',
    dataLabel:               'Data de Nascimento',
    dataPlaceholder:         'DD/MM/AAAA',
    areaAtuacaoLabel:        'Profissão',
    areaAtuacaoPlaceholder:  'Ex.: Engenheiro Civil',
    documentoHeader:         'CPF',
    dataHeader:              'Nascimento',
    areaAtuacaoHeader:       'Profissão',
  },
  PessoaJuridica: {
    nomeLabel:               'Razão Social *',
    nomePlaceholder:         'Ex.: Empresa XYZ Ltda',
    dataLabel:               'Data de Abertura / Fundação',
    dataPlaceholder:         'DD/MM/AAAA',
    areaAtuacaoLabel:        'Ramo de Atividade',
    areaAtuacaoPlaceholder:  'Ex.: Comércio varejista de vestuário',
    documentoHeader:         'CNPJ',
    dataHeader:              'Abertura',
    areaAtuacaoHeader:       'Ramo de Atividade',
  },
};

/** Returns the full label set for the given TipoPessoa. */
export function getLabels(tipo: TipoPessoa): ClienteFieldLabels {
  return LABELS[tipo];
}
