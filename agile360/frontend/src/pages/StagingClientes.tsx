import { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import {
  listStagingPendentes,
  confirmarStaging,
  rejeitarStaging,
  editarStaging,
  type StagingClienteResponse,
} from '../api/staging';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { formatCpf, formatCnpj, formatPhone, formatRg, maskCnpj, maskCpf, maskPhone, maskRg, rawDigits } from '../utils/masks';
import { getLabels } from '../utils/clienteLabels';
import { Modal } from '../components/Modal';

function formatDoc(s: StagingClienteResponse) {
  const tipoPessoa =
    (s as any).tipo_pessoa ?? (s as any).tipoPessoa ?? 'PessoaFisica';
  if (tipoPessoa === 'PessoaJuridica') return formatCnpj(s.cnpj);
  return formatCpf(s.cpf);
}

function formatDate(raw?: string) {
  if (!raw) return '—';
  // DateOnly do backend costuma vir como "YYYY-MM-DD" (sem offset).
  // Nesse caso, não devemos usar `new Date()` para evitar deslocamento de fuso.
  if (/^\d{4}-\d{2}-\d{2}$/.test(raw)) {
    const [y, m, d] = raw.split('-');
    return `${d}/${m}/${y}`;
  }
  // Backend envia timestamptz via JSON — pode vir como ISO com offset.
  // Usamos conversão para data no fuso local do navegador.
  const normalized = raw.includes(' ') && !raw.includes('T')
    ? raw.replace(' ', 'T')
    : raw;
  const dt = new Date(normalized);
  if (Number.isNaN(dt.getTime())) return '—';
  return dt.toLocaleDateString('pt-BR');
}

function getTipoPessoa(item: StagingClienteResponse): 'PessoaFisica' | 'PessoaJuridica' {
  const tipoPessoa =
    (item as any).tipo_pessoa ?? (item as any).tipoPessoa ?? 'PessoaFisica';
  return tipoPessoa === 'PessoaJuridica' ? 'PessoaJuridica' : 'PessoaFisica';
}

export function StagingClientes() {
  const { state } = useAuth();
  const navigate = useNavigate();

  const [items, setItems] = useState<StagingClienteResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [processing, setProcessing] = useState<string | null>(null);

  const [messageModal, setMessageModal] = useState<StagingClienteResponse | null>(null);

  const [editId, setEditId] = useState<string | null>(null);
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);
  const [buscandoCep, setBuscandoCep] = useState(false);
  const [erroCep, setErroCep] = useState<string | null>(null);
  const lastCepQueryRef = useRef<string>('');
  const [draft, setDraft] = useState({
    // Documento / identificação
    nome_completo: '',
    cpf: '',
    rg: '',
    orgao_expedidor: '',
    razao_social: '',
    cnpj: '',
    inscricao_estadual: '',

    // Dados complementares / contato
    data_referencia: '',
    area_atuacao: '',
    estado_civil: '',
    email: '',
    telefone: '',
    whatsapp_numero: '',

    // Endereço
    cep: '',
    estado: '',
    cidade: '',
    endereco: '',
    numero: '',
    bairro: '',
    complemento: '',

    // Financeiro
    numero_conta: '',
    pix: '',

    observacoes: '',
  });

  const load = useCallback(async () => {
    if (!state.token) return;
    setLoading(true);
    const res = await listStagingPendentes(state.token);
    if (res.success && Array.isArray(res.data)) {
      setItems(res.data);
      setError(null);
    } else {
      setError(res.error?.message ?? 'Erro ao carregar registros pendentes.');
    }
    setLoading(false);
  }, [state.token]);

  useEffect(() => { load(); }, [load]);

  async function handleConfirmar(id: string) {
    if (!state.token) return;
    setProcessing(id);
    const res = await confirmarStaging(id, state.token);
    setProcessing(null);
    if (res.success) {
      setItems(prev => prev.filter(i => i.id !== id));
      navigate('/app/clientes');
    } else {
      alert(res.error?.message ?? 'Erro ao confirmar cadastro.');
    }
  }

  async function handleRejeitar(id: string) {
    if (!state.token) return;
    if (!window.confirm('Rejeitar este cadastro? O registro será descartado.')) return;
    setProcessing(id);
    const res = await rejeitarStaging(id, state.token);
    setProcessing(null);
    if (res.success) {
      setItems(prev => prev.filter(i => i.id !== id));
    } else {
      alert(res.error?.message ?? 'Erro ao rejeitar cadastro.');
    }
  }

  function handleVerMensagem(item: StagingClienteResponse) {
    setMessageModal(item);
  }

  function handleEditar(item: StagingClienteResponse) {
    setEditError(null);
    setEditSaving(false);
    setEditId(item.id);

    const tipoPessoa = getTipoPessoa(item);
    const whatsAppNumero =
      item.whatsapp_numero ??
      item.whats_app_numero ??
      (item as any).whatsAppNumero ??
      '';

    setDraft({
      // Documento / identificação
      nome_completo: tipoPessoa === 'PessoaJuridica' ? '' : (item.nome ?? ''),
      cpf: tipoPessoa === 'PessoaJuridica' ? '' : (item.cpf ? formatCpf(item.cpf) : ''),
      rg: tipoPessoa === 'PessoaJuridica' ? '' : (item.rg ? formatRg(item.rg) : ''),
      orgao_expedidor: tipoPessoa === 'PessoaJuridica' ? '' : (item.orgao_expedidor ?? ''),
      razao_social: tipoPessoa === 'PessoaJuridica' ? (item.razao_social ?? '') : '',
      cnpj: tipoPessoa === 'PessoaJuridica' ? (item.cnpj ? formatCnpj(item.cnpj) : '') : '',
      inscricao_estadual: tipoPessoa === 'PessoaJuridica' ? (item.inscricao_estadual ?? '') : '',

      // Dados complementares / contato
      data_referencia: item.data_referencia ?? '',
      area_atuacao: item.area_atuacao ?? '',
      estado_civil: (item as any).estado_civil ?? '',
      email: item.email ?? '',
      telefone: item.telefone ? formatPhone(item.telefone) : '',
      whatsapp_numero: whatsAppNumero ? formatPhone(whatsAppNumero) : '',

      // Endereço
      cep: (item as any).cep ?? '',
      estado: (item as any).estado ?? '',
      cidade: (item as any).cidade ?? '',
      endereco: item.endereco ?? '',
      numero: (item as any).numero ?? '',
      bairro: (item as any).bairro ?? '',
      complemento: (item as any).complemento ?? '',

      // Financeiro
      numero_conta: (item as any).numero_conta ?? '',
      pix: (item as any).pix ?? '',

      observacoes: item.observacoes ?? '',
    });
  }

  function handleCancelarEdicao() {
    setEditError(null);
    setEditSaving(false);
    setEditId(null);
  }

  async function handleSalvarEdicao() {
    if (!state.token) return;
    if (!editId) return;

    const item = items.find(i => i.id === editId);
    if (!item) return;

    setEditSaving(true);
    setEditError(null);

    const tipoPessoa = getTipoPessoa(item);

    const payload: any = {};

    if (tipoPessoa === 'PessoaFisica') {
      const nome = draft.nome_completo.trim();
      if (nome.length > 0) payload.nome_completo = nome;

      const cpfDigits = rawDigits(draft.cpf);
      if (cpfDigits) payload.cpf = cpfDigits;

      const rgDigits = rawDigits(draft.rg);
      if (rgDigits) payload.rg = rgDigits;

      const orgao = draft.orgao_expedidor.trim();
      if (orgao.length > 0) payload.orgao_expedidor = orgao;
    } else {
      const razao = draft.razao_social.trim();
      if (razao.length > 0) payload.razao_social = razao;

      const cnpjDigits = rawDigits(draft.cnpj);
      if (cnpjDigits) payload.cnpj = cnpjDigits;

      const ie = draft.inscricao_estadual.trim();
      if (ie.length > 0) payload.inscricao_estadual = ie;
    }

    // Campos compartilhados
    if (draft.telefone) {
      const telDigits = rawDigits(draft.telefone);
      if (telDigits) payload.telefone = telDigits;
    }
    if (draft.whatsapp_numero) {
      const waDigits = rawDigits(draft.whatsapp_numero);
      if (waDigits) payload.whatsapp_numero = waDigits;
    }
    if (draft.data_referencia) payload.data_referencia = draft.data_referencia;
    if (draft.area_atuacao.trim()) payload.area_atuacao = draft.area_atuacao.trim();
    if (draft.estado_civil.trim()) payload.estado_civil = draft.estado_civil.trim();
    if (draft.email.trim()) payload.email = draft.email.trim();

    // Endereço
    if (draft.cep) {
      const cepDigits = rawDigits(draft.cep);
      if (cepDigits) payload.cep = cepDigits;
    }
    if (draft.estado.trim()) payload.estado = draft.estado.trim();
    if (draft.cidade.trim()) payload.cidade = draft.cidade.trim();
    if (draft.endereco.trim()) payload.endereco = draft.endereco.trim();
    if (draft.numero.trim()) payload.numero = draft.numero.trim();
    if (draft.bairro.trim()) payload.bairro = draft.bairro.trim();
    if (draft.complemento.trim()) payload.complemento = draft.complemento.trim();

    // Financeiro
    if (draft.numero_conta.trim()) {
      payload.numero_conta = draft.numero_conta.trim();
    }
    if (draft.pix.trim()) payload.pix = draft.pix.trim();

    if (draft.observacoes.trim()) payload.observacoes = draft.observacoes.trim();

    const res = await editarStaging(editId, payload, state.token);

    setEditSaving(false);

    if (res.success && res.data) {
      const updated = res.data;
      setItems(prev => prev.map(i => (i.id === editId ? updated : i)));
      setEditId(null);
    } else {
      setEditError(res.error?.message ?? 'Erro ao salvar alterações.');
    }
  }

  const editingItem = editId ? items.find(i => i.id === editId) : null;
  const editingTipoPessoa = editingItem ? getTipoPessoa(editingItem) : 'PessoaFisica';
  const labels = getLabels(editingTipoPessoa);

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-[var(--color-text)]">
            Ações Pendentes do Bot
          </h1>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Cadastros enviados via WhatsApp aguardando sua revisão. Confirme para adicionar à base ou rejeite para descartar.
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="secondary"
            type="button"
            onClick={() => navigate('/app/staging/processos')}
            className="min-h-[44px]"
          >
            Triagem Processos
          </Button>
          <Button
            variant="secondary"
            type="button"
            onClick={() => navigate('/app/staging/compromissos')}
            className="min-h-[44px]"
          >
            Triagem Compromissos
          </Button>
          <Button
            variant="secondary"
            type="button"
            onClick={() => navigate('/app/staging/prazos')}
            className="min-h-[44px]"
          >
            Triagem Prazos
          </Button>
        </div>
      </div>

      {loading && (
        <p className="text-[var(--color-text-muted)]">Carregando...</p>
      )}

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {!loading && !error && items.length === 0 && (
        <div className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-6 py-10 text-center">
          <p className="text-[var(--color-text-muted)]">Nenhum cadastro pendente no momento.</p>
        </div>
      )}

      {!loading && items.length > 0 && (
        <div className="space-y-4">
          {items.map(item => {
            const tipoPessoa =
              (item as any).tipo_pessoa ?? (item as any).tipoPessoa ?? 'PessoaFisica';
            const labels = getLabels(
              tipoPessoa === 'PessoaJuridica' ? 'PessoaJuridica' : 'PessoaFisica',
            );
            const isPending = processing === item.id;
            const isThisEditing = editId === item.id;
            const isOtherEditing = editId !== null && !isThisEditing;

            const expiresAtRaw =
              item.expires_at ?? (item as any).expiresAt ?? '';
            const createdAtRaw =
              item.created_at ?? (item as any).createdAt ?? '';
            const expiresAt = expiresAtRaw ? new Date(expiresAtRaw) : null;
            const isExpiringSoon = expiresAt
              ? (expiresAt.getTime() - Date.now()) < 1000 * 60 * 60 * 3 // 3h
              : false;

            const whatsAppNumero =
              item.whatsapp_numero ??
              item.whats_app_numero ??
              (item as any).whatsAppNumero;

            return (
              <div
                key={item.id}
                className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-5 shadow-sm"
              >
                {/* Header row */}
                <div className="mb-3 flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <span className="inline-block rounded-full bg-amber-100 px-2.5 py-0.5 text-xs font-medium text-amber-800">
                      {tipoPessoa === 'PessoaJuridica' ? 'Pessoa Jurídica' : 'Pessoa Física'}
                    </span>
                    <h2 className="mt-1 text-base font-semibold text-[var(--color-text)]">
                      {tipoPessoa === 'PessoaJuridica'
                        ? (item.razao_social || '—')
                        : (item.nome || '—')}
                    </h2>
                  </div>
                  <div className="text-right text-xs text-[var(--color-text-muted)]">
                    <div>
                      Recebido: {createdAtRaw ? formatDate(createdAtRaw) : '—'}
                    </div>
                    <div className={isExpiringSoon ? 'font-medium text-amber-600' : ''}>
                      Expira: {expiresAtRaw ? formatDate(String(expiresAtRaw)) : '—'}
                    </div>
                  </div>
                </div>

                {/* Data grid */}
                <dl className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm sm:grid-cols-3 lg:grid-cols-4">
                  <DataItem label={labels.documentoHeader} value={formatDoc(item)} />
                  {tipoPessoa === 'PessoaFisica' && item.rg && (
                    <DataItem label="RG" value={formatRg(item.rg)} />
                  )}
                  {item.email && <DataItem label="E-mail" value={item.email} />}
                  {item.telefone && (
                    <DataItem label="Telefone" value={formatPhone(item.telefone)} />
                  )}
                  {whatsAppNumero && <DataItem label="WhatsApp" value={formatPhone(whatsAppNumero)} />}
                  {item.data_referencia && (
                    <DataItem label={labels.dataHeader} value={formatDate(item.data_referencia)} />
                  )}
                  {item.area_atuacao && (
                    <DataItem label={labels.areaAtuacaoHeader} value={item.area_atuacao} />
                  )}
                  {item.endereco && <DataItem label="Endereço" value={item.endereco} />}
                </dl>

                {/* Original WhatsApp message */}
                {item.origem_mensagem && (
                  <button
                    type="button"
                    onClick={() => handleVerMensagem(item)}
                    className="mt-3 w-full rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-3 py-2 text-xs font-semibold text-[var(--color-text)] hover:bg-[var(--color-surface-elevated)]"
                    style={{ textAlign: 'left' }}
                  >
                    Ver mensagem original do WhatsApp
                  </button>
                )}

                {/* Observações */}
                {item.observacoes && (
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    <span className="font-medium">Obs:</span> {item.observacoes}
                  </p>
                )}

                {/* Action buttons */}
                <div className="mt-4 flex gap-3">
                  {isThisEditing ? (
                    <button
                      type="button"
                      onClick={handleCancelarEdicao}
                      disabled={editSaving}
                      className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-4 py-2 text-sm font-medium text-[var(--color-text)] hover:bg-[var(--color-surface-elevated)] disabled:opacity-50"
                    >
                      Cancelar edição
                    </button>
                  ) : (
                    <>
                      <button
                        onClick={() => handleConfirmar(item.id)}
                        disabled={isPending || isOtherEditing}
                        className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
                      >
                        {isPending ? 'Processando...' : 'Confirmar Cadastro'}
                      </button>
                      <button
                        onClick={() => handleEditar(item)}
                        disabled={isPending || isOtherEditing}
                        className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-4 py-2 text-sm font-medium text-[var(--color-text)] hover:bg-[var(--color-surface-elevated)] disabled:opacity-50"
                      >
                        Editar Cadastro
                      </button>
                      <button
                        onClick={() => handleRejeitar(item.id)}
                        disabled={isPending || isOtherEditing}
                        className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
                      >
                        Rejeitar
                      </button>
                    </>
                  )}
                </div>

                {isThisEditing && editError && (
                  <p className="mt-3 text-xs text-[var(--color-error)]">{editError}</p>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* ───────────────────────────────────────────────────────────────────
          Modal de Edição Completa (Triagem / staging)
         ─────────────────────────────────────────────────────────────────── */}
      <Modal
        open={!!editId}
        onClose={handleCancelarEdicao}
        title="Triagem — Editar Cadastro (staging)"
        size="max-w-5xl"
      >
        <form
          className="space-y-5"
          onSubmit={e => {
            e.preventDefault();
            handleSalvarEdicao();
          }}
        >
          <div className="flex items-center justify-between gap-4">
            <span className="inline-flex items-center rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold text-amber-800">
              {editingTipoPessoa === 'PessoaJuridica' ? 'Pessoa Jurídica' : 'Pessoa Física'}
            </span>
            <button
              type="button"
              onClick={handleCancelarEdicao}
              disabled={editSaving}
              className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-3 py-2 text-sm font-medium text-[var(--color-text)] hover:bg-[var(--color-surface-elevated)] disabled:opacity-50"
            >
              Fechar
            </button>
          </div>

          {/* Documento */}
          {editingTipoPessoa === 'PessoaFisica' ? (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label={labels.nomeLabel}
                value={draft.nome_completo}
                onChange={e => setDraft(d => ({ ...d, nome_completo: e.target.value }))}
                placeholder={labels.nomePlaceholder}
              />
              <Input
                label="CPF *"
                value={draft.cpf}
                onChange={e => setDraft(d => ({ ...d, cpf: maskCpf(e.target.value) }))}
                placeholder="000.000.000-00"
                inputMode="numeric"
              />
              <Input
                label="RG"
                value={draft.rg}
                onChange={e => setDraft(d => ({ ...d, rg: maskRg(e.target.value) }))}
                placeholder="00.000.000-0"
                inputMode="numeric"
              />
              <Input
                label="Orgão Expedidor"
                value={draft.orgao_expedidor}
                onChange={e => setDraft(d => ({ ...d, orgao_expedidor: e.target.value }))}
                placeholder="Ex.: SSP/SP"
              />
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label={labels.nomeLabel}
                value={draft.razao_social}
                onChange={e => setDraft(d => ({ ...d, razao_social: e.target.value }))}
                placeholder={labels.nomePlaceholder}
              />
              <Input
                label="CNPJ *"
                value={draft.cnpj}
                onChange={e => setDraft(d => ({ ...d, cnpj: maskCnpj(e.target.value) }))}
                placeholder="00.000.000/0000-00"
                inputMode="numeric"
              />
              <Input
                label="Inscrição Estadual"
                value={draft.inscricao_estadual}
              onChange={e => setDraft(d => ({ ...d, inscricao_estadual: e.target.value.slice(0, 20) }))}
                placeholder="Ex.: 123.456.789.000"
              />
            </div>
          )}

          {/* Complementos + Contato */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label={labels.dataLabel}
              value={draft.data_referencia}
              onChange={e => setDraft(d => ({ ...d, data_referencia: e.target.value }))}
              type="date"
            />
            <Input
              label={labels.areaAtuacaoLabel}
              value={draft.area_atuacao}
              onChange={e => setDraft(d => ({ ...d, area_atuacao: e.target.value }))}
              placeholder={labels.areaAtuacaoPlaceholder}
              maxLength={200}
            />
            <div className="flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Estado civil</label>
              <select
                value={draft.estado_civil}
                onChange={e => setDraft(d => ({ ...d, estado_civil: e.target.value }))}
                className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
              >
                <option value="">Selecione…</option>
                {['Solteiro(a)', 'Casado(a)', 'Divorciado(a)', 'Viúvo(a)', 'União Estável'].map(v => (
                  <option key={v} value={v}>
                    {v}
                  </option>
                ))}
              </select>
            </div>
            <Input
              label="E-mail"
              value={draft.email}
              onChange={e => setDraft(d => ({ ...d, email: e.target.value }))}
              type="email"
              placeholder="seuemail@dominio.com"
            />
            <Input
              label="Telefone"
              value={draft.telefone}
              onChange={e => setDraft(d => ({ ...d, telefone: maskPhone(e.target.value) }))}
              placeholder="(00) 00000-0000"
              inputMode="tel"
            />
            <Input
              label="WhatsApp"
              value={draft.whatsapp_numero}
              onChange={e => setDraft(d => ({ ...d, whatsapp_numero: maskPhone(e.target.value) }))}
              placeholder="(00) 00000-0000"
              inputMode="tel"
            />
          </div>

          {/* Endereço completo */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Input
              label="CEP"
              value={draft.cep}
              onChange={async e => {
                const cepLimpo = e.target.value.replace(/\D/g, '').slice(0, 8);
                setDraft(d => ({ ...d, cep: cepLimpo }));
                setErroCep(null);

                // Só consulta com 8 dígitos (mesma regra do cadastro manual)
                if (cepLimpo.length !== 8) return;
                if (cepLimpo === lastCepQueryRef.current) return;
                lastCepQueryRef.current = cepLimpo;

                setBuscandoCep(true);
                try {
                  const res = await fetch(`https://viacep.com.br/ws/${cepLimpo}/json/`);
                  if (!res.ok) throw new Error('Falha na requisição ViaCEP.');
                  const data = await res.json();
                  if (data.erro) {
                    setErroCep('CEP não encontrado.');
                    return;
                  }

                  setDraft(f => ({
                    ...f,
                    endereco: data.logradouro ?? f.endereco,
                    bairro: data.bairro ?? f.bairro,
                    cidade: data.localidade ?? f.cidade,
                    estado: data.uf ?? f.estado,
                    complemento: f.complemento || (data.complemento ?? ''),
                  }));
                } catch {
                  setErroCep('Erro ao consultar o CEP. Verifique e tente novamente.');
                } finally {
                  setBuscandoCep(false);
                }
              }}
              placeholder="01310-100"
              inputMode="numeric"
              disabled={buscandoCep}
              error={erroCep ?? undefined}
            />
            <div className="flex flex-col gap-1">
              <label className="text-sm text-[var(--color-text-muted)]">Estado (UF)</label>
              <select
                value={draft.estado}
                onChange={e => setDraft(d => ({ ...d, estado: e.target.value }))}
                className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
              >
                <option value="">Selecione…</option>
                {['AC','AL','AM','AP','BA','CE','DF','ES','GO','MA','MG','MS','MT',
                  'PA','PB','PE','PI','PR','RJ','RN','RO','RR','RS','SC','SE','SP','TO'].map(uf => (
                    <option key={uf} value={uf}>
                      {uf}
                    </option>
                  ))}
              </select>
            </div>
            <Input
              label="Cidade"
              value={draft.cidade}
              onChange={e => setDraft(d => ({ ...d, cidade: e.target.value }))}
              placeholder="São Paulo"
            />
            <Input
              label="Logradouro (Endereço)"
              value={draft.endereco}
              onChange={e => setDraft(d => ({ ...d, endereco: e.target.value }))}
              placeholder="Avenida Paulista"
            />
            <Input
              label="Número"
              value={draft.numero}
              onChange={e => setDraft(d => ({ ...d, numero: e.target.value }))}
              placeholder="1000"
            />
            <Input
              label="Bairro"
              value={draft.bairro}
              onChange={e => setDraft(d => ({ ...d, bairro: e.target.value }))}
              placeholder="Bela Vista"
            />
            <Input
              label="Complemento"
              value={draft.complemento}
              onChange={e => setDraft(d => ({ ...d, complemento: e.target.value }))}
              placeholder="Ap. 42"
            />
          </div>

          {/* Financeiro */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label="Número da Conta"
              value={draft.numero_conta}
              onChange={e => setDraft(d => ({ ...d, numero_conta: e.target.value.slice(0, 50) }))}
              placeholder="0001 / 00012345-6"
            />
            <Input
              label="PIX"
              value={draft.pix}
              onChange={e => setDraft(d => ({ ...d, pix: e.target.value }))}
              placeholder="chave@pix.com ou CPF"
            />
          </div>

          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-muted)]">Observações</label>
            <textarea
              value={draft.observacoes}
              onChange={e => setDraft(d => ({ ...d, observacoes: e.target.value }))}
              rows={4}
              className="w-full resize-none rounded-[var(--radius)] border border-[var(--color-border)] bg-[var(--color-surface)] px-3 py-2 text-sm text-[var(--color-text)] outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-primary)]"
              placeholder="Qualquer detalhe adicional relevante..."
            />
          </div>

          {editError && <p className="text-sm text-[var(--color-error)]" role="alert">{editError}</p>}

          <div className="flex justify-end gap-3 pt-2 border-t border-[var(--color-border)]">
            <button
              type="button"
              onClick={handleCancelarEdicao}
              disabled={editSaving}
              className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-4 py-2 text-sm font-medium text-[var(--color-text)] hover:bg-[var(--color-surface-elevated)] disabled:opacity-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={editSaving}
              className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
            >
              {editSaving ? 'Salvando...' : 'Salvar alterações'}
            </button>
          </div>
        </form>
      </Modal>

      <Modal
        open={!!messageModal}
        onClose={() => setMessageModal(null)}
        title="Mensagem original do WhatsApp"
        size="max-w-3xl"
      >
        <div className="whitespace-pre-wrap rounded-lg bg-[var(--color-surface)] border border-[var(--color-border)] px-3 py-3 text-sm text-[var(--color-text)]">
          {messageModal?.origem_mensagem ?? '—'}
        </div>
      </Modal>
    </div>
  );
}

function DataItem({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[var(--color-text-muted)]">{label}</dt>
      <dd className="font-medium text-[var(--color-text)]">{value || '—'}</dd>
    </div>
  );
}
