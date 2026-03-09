import { useEffect, useRef, useState } from 'react';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Modal } from '../components/Modal';
import { useToken } from '../context/AuthContext';
import {
  type Cliente,
  type CriarClientePayload,
  type ImportarClientesResult,
  clientesApi,
} from '../api/clientes';

// ─── Icones SVG (inline — sem dependencia externa) ───────────────────────────

function IconImport() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
      <polyline points="7 10 12 15 17 10" />
      <line x1="12" y1="15" x2="12" y2="3" />
    </svg>
  );
}

function IconDownload() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
      <polyline points="7 10 12 15 17 10" />
      <line x1="12" y1="15" x2="12" y2="3" />
    </svg>
  );
}

function IconAttach() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66L9.41 17.41a2 2 0 0 1-2.83-2.83l8.49-8.48" />
    </svg>
  );
}

// ─── Máscaras ─────────────────────────────────────────────────────────────────

const onlyDigits = (v = '') => v.replace(/\D/g, '');

/** 000.000.000-00 */
const maskCpf = (v: string) => {
  const d = onlyDigits(v).slice(0, 11);
  if (d.length <=  3) return d;
  if (d.length <=  6) return `${d.slice(0,3)}.${d.slice(3)}`;
  if (d.length <=  9) return `${d.slice(0,3)}.${d.slice(3,6)}.${d.slice(6)}`;
  return `${d.slice(0,3)}.${d.slice(3,6)}.${d.slice(6,9)}-${d.slice(9)}`;
};

/** 00.000.000/0001-00 */
const maskCnpj = (v: string) => {
  const d = onlyDigits(v).slice(0, 14);
  if (d.length <=  2) return d;
  if (d.length <=  5) return `${d.slice(0,2)}.${d.slice(2)}`;
  if (d.length <=  8) return `${d.slice(0,2)}.${d.slice(2,5)}.${d.slice(5)}`;
  if (d.length <= 12) return `${d.slice(0,2)}.${d.slice(2,5)}.${d.slice(5,8)}/${d.slice(8)}`;
  return `${d.slice(0,2)}.${d.slice(2,5)}.${d.slice(5,8)}/${d.slice(8,12)}-${d.slice(12)}`;
};

/** (11) 98765-4321  ou  (11) 3456-7890 */
const maskTelefone = (v: string) => {
  const d = onlyDigits(v).slice(0, 11);
  if (d.length <=  2) return d.length ? `(${d}` : '';
  if (d.length <=  6) return `(${d.slice(0,2)}) ${d.slice(2)}`;
  if (d.length <= 10) return `(${d.slice(0,2)}) ${d.slice(2,6)}-${d.slice(6)}`;
  // 11 dígitos → celular com 9
  return `(${d.slice(0,2)}) ${d.slice(2,7)}-${d.slice(7)}`;
};

/** 00.000.000-0  (formato SP/mais comum) */
const maskRg = (v: string) => {
  const d = onlyDigits(v).slice(0, 9);
  if (d.length <=  2) return d;
  if (d.length <=  5) return `${d.slice(0,2)}.${d.slice(2)}`;
  if (d.length <=  8) return `${d.slice(0,2)}.${d.slice(2,5)}.${d.slice(5)}`;
  return `${d.slice(0,2)}.${d.slice(2,5)}.${d.slice(5,8)}-${d.slice(8)}`;
};

// ─── Tipos de estado ──────────────────────────────────────────────────────────

type FormState = CriarClientePayload;

const EMPTY_FORM: FormState = {
  tipo_cliente: 'Pessoa Física',
  nome_completo: '',
  cpf: '',
  rg: '',
  orgao_expedidor: '',
  data_nascimento: '',
  estado_civil: '',
  profissao: '',
  telefone: '',
  numero_conta: '',
  pix: '',
  cep: '',
  endereco: '',
  numero: '',
  bairro: '',
  complemento: '',
  cidade: '',
  estado: '',
};

// ─── Componente ───────────────────────────────────────────────────────────────

export function Clientes() {
  const token = useToken();

  // ── Lista ─────────────────────────────────────────────────────────────────
  const [clientes, setClientes]       = useState<Cliente[]>([]);
  const [carregando, setCarregando]   = useState(true);
  const [erroLista, setErroLista]     = useState<string | null>(null);

  // ── Modal novo/editar ─────────────────────────────────────────────────────
  const [modalAberto, setModalAberto]   = useState(false);
  const [editando, setEditando]         = useState<Cliente | null>(null);
  const [form, setForm]                 = useState<FormState>(EMPTY_FORM);
  const [salvando, setSalvando]         = useState(false);
  const [erroForm, setErroForm]         = useState<string | null>(null);

  // ── Modal importar ────────────────────────────────────────────────────────
  const [modalImportar, setModalImportar]     = useState(false);
  const [arquivoSelecionado, setArquivo]      = useState<File | null>(null);
  const [importando, setImportando]           = useState(false);
  const [resultadoImport, setResultado]       = useState<ImportarClientesResult | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  // ─── Carregar lista ────────────────────────────────────────────────────────
  const carregar = async () => {
    if (!token) return;
    setCarregando(true);
    setErroLista(null);
    const res = await clientesApi.listar(token);
    if (res.success) setClientes(res.data ?? []);
    else setErroLista(res.error?.message ?? 'Erro ao carregar clientes.');
    setCarregando(false);
  };

  useEffect(() => { carregar(); }, [token]);

  // ─── Abrir modal criar ─────────────────────────────────────────────────────
  const abrirCriar = () => {
    setEditando(null);
    setForm(EMPTY_FORM);
    setErroForm(null);
    setModalAberto(true);
  };

  // ─── Abrir modal editar ────────────────────────────────────────────────────
  const isFisica = (f: FormState) => f.tipo_cliente !== 'Pessoa Jurídica';

  const abrirEditar = (c: Cliente) => {
    setEditando(c);
    const tipo = c.tipo_cliente ?? 'Pessoa Física';
    // re-aplica a máscara correta ao abrir (dados vêm sem máscara do banco)
    const cpfMasked = tipo === 'Pessoa Jurídica'
      ? maskCnpj(c.cpf ?? '')
      : maskCpf(c.cpf ?? '');
    setForm({
      tipo_cliente:    tipo,
      nome_completo:   c.nome_completo,
      cpf:             cpfMasked,
      rg:              maskRg(c.rg ?? ''),
      orgao_expedidor: c.orgao_expedidor ?? '',
      data_nascimento: c.data_nascimento ?? '',
      estado_civil:    c.estado_civil ?? '',
      profissao:       c.profissao ?? '',
      telefone:        c.telefone ?? '',
      numero_conta:    c.numero_conta ?? '',
      pix:             c.pix ?? '',
      cep:             c.cep ?? '',
      endereco:        c.endereco ?? '',
      numero:          c.numero ?? '',
      bairro:          c.bairro ?? '',
      complemento:     c.complemento ?? '',
      cidade:          c.cidade ?? '',
      estado:          c.estado ?? '',
    });
    setErroForm(null);
    setModalAberto(true);
  };

  // ─── Salvar (criar ou atualizar) — envia apenas dígitos em CPF/CNPJ/RG ────
  const salvar = async () => {
    if (!token) return;
    if (!form.nome_completo.trim()) { setErroForm('Nome completo é obrigatório.'); return; }
    setSalvando(true);
    setErroForm(null);

    // Remove máscaras antes de enviar
    const payload: CriarClientePayload = {
      ...form,
      cpf:     onlyDigits(form.cpf)      || undefined,
      rg:      onlyDigits(form.rg)       || undefined,
      telefone: onlyDigits(form.telefone) || undefined,
    };

    try {
      if (editando) {
        const res = await clientesApi.atualizar(editando.id, payload, token);
        if (!res.success) { setErroForm(res.error?.message ?? 'Erro ao salvar.'); return; }
      } else {
        const res = await clientesApi.criar(payload, token);
        if (!res.success) { setErroForm(res.error?.message ?? 'Erro ao criar.'); return; }
      }
      setModalAberto(false);
      carregar();
    } finally {
      setSalvando(false);
    }
  };

  // ─── Excluir ───────────────────────────────────────────────────────────────
  const excluir = async (id: string) => {
    if (!token) return;
    if (!confirm('Deseja excluir este cliente? Esta ação não pode ser desfeita.')) return;
    await clientesApi.excluir(id, token);
    carregar();
  };

  // ─── Download do modelo ────────────────────────────────────────────────────
  const [erroDownload, setErroDownload]     = useState<string | null>(null);
  const [baixandoModelo, setBaixandoModelo] = useState(false);

  const baixarModelo = async () => {
    if (!token) return;
    setBaixandoModelo(true);
    setErroDownload(null);
    try {
      await clientesApi.downloadTemplate(token);
    } catch {
      setErroDownload('Não foi possível baixar o modelo. Tente novamente.');
    } finally {
      setBaixandoModelo(false);
    }
  };

  // ─── Importar ──────────────────────────────────────────────────────────────
  const abrirImportar = () => {
    setArquivo(null);
    setResultado(null);
    setModalImportar(true);
  };

  const enviarPlanilha = async () => {
    if (!token || !arquivoSelecionado) return;
    setImportando(true);
    const res = await clientesApi.importar(arquivoSelecionado, token);
    if (res.success && res.data) {
      setResultado(res.data);
      carregar();
    } else {
      setResultado({ total: 0, sucesso: 0, falhas: 1, erros: [{ linha: 0, nome_completo: '', motivo: res.error?.message ?? 'Erro desconhecido.' }] });
    }
    setImportando(false);
  };

  // ─── ViaCEP ────────────────────────────────────────────────────────────────
  const [buscandoCep, setBuscandoCep] = useState(false);
  const [erroCep, setErroCep]         = useState<string | null>(null);

  const buscarCep = async () => {
    const cepLimpo = (form.cep ?? '').replace(/\D/g, '');
    if (cepLimpo.length !== 8) return;          // só dispara com 8 dígitos
    setBuscandoCep(true);
    setErroCep(null);
    try {
      const res = await fetch(`https://viacep.com.br/ws/${cepLimpo}/json/`);
      if (!res.ok) throw new Error('Falha na requisição ViaCEP.');
      const data = await res.json();
      if (data.erro) {
        setErroCep('CEP não encontrado.');
        return;
      }
      setForm(f => ({
        ...f,
        endereco:    data.logradouro  ?? f.endereco,
        bairro:      data.bairro      ?? f.bairro,
        cidade:      data.localidade  ?? f.cidade,
        estado:      data.uf          ?? f.estado,
        // complemento: preenche só se vier valor e o campo estiver vazio
        complemento: f.complemento || (data.complemento ?? ''),
      }));
    } catch {
      setErroCep('Erro ao consultar o CEP. Verifique e tente novamente.');
    } finally {
      setBuscandoCep(false);
    }
  };

  // ─── Helpers de formulário ─────────────────────────────────────────────────
  const set = (field: keyof FormState) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setForm(f => ({ ...f, [field]: e.target.value }));

  /** Aplica máscara ao digitar e salva o valor já formatado no estado */
  const setMasked = (field: keyof FormState, fn: (v: string) => string) =>
    (e: React.ChangeEvent<HTMLInputElement>) =>
      setForm(f => ({ ...f, [field]: fn(e.target.value) }));

  // ─── Render ────────────────────────────────────────────────────────────────
  return (
    <div>
      {/* Cabeçalho — empilha no mobile, botões touch 44px */}
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-center sm:justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-text)] sm:text-2xl">Clientes</h1>
        <div className="flex flex-col gap-2 sm:flex-row sm:gap-2">
          <Button variant="secondary" onClick={abrirImportar} className="min-h-[44px] w-full sm:w-auto flex items-center justify-center gap-2">
            <IconImport />
            Cadastro em massa
          </Button>
          <Button variant="primary" onClick={abrirCriar} className="min-h-[44px] w-full sm:w-auto">
            + Novo cliente
          </Button>
        </div>
      </div>

      {/* Lista */}
      {carregando ? (
        <p className="text-[var(--color-text-muted)]">Carregando…</p>
      ) : erroLista ? (
        <p className="text-[var(--color-error)]">{erroLista}</p>
      ) : clientes.length === 0 ? (
        <p className="text-[var(--color-text-muted)]">Nenhum cliente cadastrado ainda.</p>
      ) : (
        <>
          {/* Mobile: cards roláveis — uso com uma mão */}
          <div className="flex flex-col gap-3 md:hidden">
            {clientes.map(c => (
              <article
                key={c.id}
                className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-4 shadow-sm"
              >
                <p className="font-semibold text-[var(--color-text-heading)]">{c.nome_completo}</p>
                <p className="text-xs text-[var(--color-text-muted)]">
                  {c.tipo_cliente === 'Pessoa Jurídica' ? 'PJ' : 'PF'}
                  {c.cpf && ` · ${c.tipo_cliente === 'Pessoa Jurídica' ? maskCnpj(c.cpf) : maskCpf(c.cpf)}`}
                </p>
                {c.telefone && (
                  <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{maskTelefone(c.telefone)}</p>
                )}
                {[c.cidade, c.estado].filter(Boolean).length > 0 && (
                  <p className="text-sm text-[var(--color-text-muted)]">{[c.cidade, c.estado].filter(Boolean).join(' / ')}</p>
                )}
                <div className="mt-3 flex gap-2">
                  <button
                    type="button"
                    onClick={() => abrirEditar(c)}
                    className="min-h-[44px] min-w-[44px] flex-1 rounded-[var(--radius)] border border-[var(--color-border)] px-4 py-2 text-sm font-medium text-[var(--color-primary)] touch-manipulation"
                  >
                    Editar
                  </button>
                  <button
                    type="button"
                    onClick={() => excluir(c.id)}
                    className="min-h-[44px] min-w-[44px] flex-1 rounded-[var(--radius)] border border-[var(--color-border)] px-4 py-2 text-sm font-medium text-[var(--color-error)] touch-manipulation"
                  >
                    Excluir
                  </button>
                </div>
              </article>
            ))}
          </div>

          {/* Desktop: tabela */}
          <div className="hidden overflow-x-auto rounded-xl border border-[var(--color-border)] md:block">
            <table className="min-w-full text-sm text-[var(--color-text)]">
              <thead className="bg-[var(--color-surface)] text-[var(--color-text-muted)] text-xs uppercase tracking-wider">
                <tr>
                  {['Nome', 'Tipo', 'CPF / CNPJ', 'Telefone', 'Cidade / UF', 'Ações'].map(h => (
                    <th key={h} className="px-4 py-3 text-left">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--color-border)]">
                {clientes.map(c => (
                  <tr key={c.id} className="hover:bg-[var(--color-surface)] transition-colors">
                    <td className="px-4 py-3 font-medium">{c.nome_completo}</td>
                    <td className="px-4 py-3 text-xs text-[var(--color-text-muted)]">
                      {c.tipo_cliente === 'Pessoa Jurídica' ? 'PJ' : 'PF'}
                    </td>
                    <td className="px-4 py-3">
                      {c.cpf
                        ? c.tipo_cliente === 'Pessoa Jurídica'
                          ? maskCnpj(c.cpf)
                          : maskCpf(c.cpf)
                        : '—'}
                    </td>
                    <td className="px-4 py-3">
                      {c.telefone ? maskTelefone(c.telefone) : '—'}
                    </td>
                    <td className="px-4 py-3">
                      {[c.cidade, c.estado].filter(Boolean).join(' / ') || '—'}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        <button
                          onClick={() => abrirEditar(c)}
                          className="text-[var(--color-primary)] hover:underline text-xs"
                        >Editar</button>
                        <button
                          onClick={() => excluir(c.id)}
                          className="text-[var(--color-error)] hover:underline text-xs"
                        >Excluir</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {/* ═══ Modal Novo / Editar ════════════════════════════════════════════════ */}
      <Modal
        open={modalAberto}
        onClose={() => setModalAberto(false)}
        title={editando ? 'Editar cliente' : 'Novo cliente'}
        size="max-w-3xl"
      >
        <div className="flex flex-col gap-4">
          {erroForm && (
            <p className="rounded-lg bg-red-500/10 border border-red-500/30 px-3 py-2 text-sm text-red-400">
              {erroForm}
            </p>
          )}

          {/* Identificação */}
          <section>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-[var(--color-text-muted)] mb-3">
              Identificação
            </h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">

              {/* Tipo de cliente */}
              <div className="sm:col-span-2 flex flex-col gap-1">
                <label className="text-sm text-[var(--color-text-muted)]">Tipo de cliente</label>
                <div className="flex gap-3">
                  {(['Pessoa Física', 'Pessoa Jurídica'] as const).map(tipo => (
                    <label
                      key={tipo}
                      className={`flex min-h-[44px] min-w-[44px] flex-1 items-center justify-center gap-2 cursor-pointer rounded-lg border px-4 py-3 text-sm transition-colors touch-manipulation
                        ${form.tipo_cliente === tipo
                          ? 'border-[var(--color-primary)] bg-[var(--color-primary)]/10 text-[var(--color-primary)] font-medium'
                          : 'border-[var(--color-border)] text-[var(--color-text-muted)] hover:border-[var(--color-primary)]/50'}`}
                    >
                      <input
                        type="radio"
                        name="tipo_cliente"
                        value={tipo}
                        checked={form.tipo_cliente === tipo}
                        onChange={() => setForm(f => ({ ...f, tipo_cliente: tipo, cpf: '' }))}
                        className="accent-[var(--color-primary)]"
                      />
                      {tipo}
                    </label>
                  ))}
                </div>
              </div>

              {/* Nome */}
              <div className="sm:col-span-2">
                <Input label="Nome completo *" name="nome_completo" value={form.nome_completo} onChange={set('nome_completo')} placeholder="Maria da Silva" />
              </div>

              {/* CPF ou CNPJ conforme tipo */}
              {isFisica(form) ? (
                <Input
                  label="CPF"
                  name="cpf"
                  value={form.cpf ?? ''}
                  onChange={setMasked('cpf', maskCpf)}
                  placeholder="000.000.000-00"
                  inputMode="numeric"
                  maxLength={14}
                />
              ) : (
                <Input
                  label="CNPJ"
                  name="cpf"
                  value={form.cpf ?? ''}
                  onChange={setMasked('cpf', maskCnpj)}
                  placeholder="00.000.000/0001-00"
                  inputMode="numeric"
                  maxLength={18}
                />
              )}

              {/* RG — só Pessoa Física */}
              {isFisica(form) && (
                <Input
                  label="RG"
                  name="rg"
                  value={form.rg ?? ''}
                  onChange={setMasked('rg', maskRg)}
                  placeholder="00.000.000-0"
                  inputMode="numeric"
                  maxLength={12}
                />
              )}

              {isFisica(form) && (
                <Input label="Órgão Expedidor" name="orgao_expedidor" value={form.orgao_expedidor} onChange={set('orgao_expedidor')} placeholder="SSP/SP" />
              )}
              <Input label="Data de Nascimento" name="data_nascimento" type="date" value={form.data_nascimento} onChange={set('data_nascimento')} />
              <div className="flex flex-col gap-1">
                <label className="text-sm text-[var(--color-text-muted)]">Estado Civil</label>
                <select
                  value={form.estado_civil}
                  onChange={set('estado_civil')}
                  className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
                >
                  <option value="">Selecione…</option>
                  {['Solteiro(a)', 'Casado(a)', 'Divorciado(a)', 'Viúvo(a)', 'União Estável'].map(v => (
                    <option key={v}>{v}</option>
                  ))}
                </select>
              </div>
              <Input label="Profissão" name="profissao" value={form.profissao} onChange={set('profissao')} placeholder="Professora" />
            </div>
          </section>

          {/* Contato */}
          <section>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-[var(--color-text-muted)] mb-3">
              Contato
            </h3>
            <Input
              label="Telefone"
              name="telefone"
              value={form.telefone ?? ''}
              onChange={setMasked('telefone', maskTelefone)}
              placeholder="(11) 98765-4321"
              inputMode="numeric"
              maxLength={15}
            />
          </section>

          {/* Financeiro */}
          <section>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-[var(--color-text-muted)] mb-3">
              Dados Financeiros
            </h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <Input label="Número da Conta" name="numero_conta" value={form.numero_conta} onChange={set('numero_conta')} placeholder="0001 / 00012345-6" />
              <Input label="PIX" name="pix" value={form.pix} onChange={set('pix')} placeholder="chave@pix.com ou CPF" />
            </div>
          </section>

          {/* Endereço */}
          <section>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-[var(--color-text-muted)] mb-3">
              Endereço
            </h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div className="flex flex-col gap-1">
                <Input
                  label={buscandoCep ? 'CEP — consultando…' : 'CEP'}
                  name="cep"
                  value={form.cep}
                  onChange={set('cep')}
                  onBlur={buscarCep}
                  placeholder="01310-100"
                  error={erroCep ?? undefined}
                  disabled={buscandoCep}
                />
              </div>
              {/* Número vem logo após CEP no tab order — campos auto-preenchidos pelo ViaCEP ficam fora do tab */}
              <Input label="Número" name="numero" value={form.numero} onChange={set('numero')} placeholder="1000" />
              <Input label="Complemento" name="complemento" value={form.complemento} onChange={set('complemento')} placeholder="Ap. 42" />
              <div className="sm:col-span-2">
                <Input label="Logradouro" name="endereco" value={form.endereco} onChange={set('endereco')} placeholder="Avenida Paulista" tabIndex={-1} />
              </div>
              <Input label="Bairro" name="bairro" value={form.bairro} onChange={set('bairro')} placeholder="Bela Vista" tabIndex={-1} />
              <Input label="Cidade" name="cidade" value={form.cidade} onChange={set('cidade')} placeholder="São Paulo" tabIndex={-1} />
              <div className="flex flex-col gap-1">
                <label className="text-sm text-[var(--color-text-muted)]">Estado (UF)</label>
                <select
                  value={form.estado}
                  onChange={set('estado')}
                  tabIndex={-1}
                  className="min-h-[44px] rounded-[var(--radius)] bg-[var(--color-surface)] border border-[var(--color-border)] px-3 text-[var(--color-text)] focus:border-[var(--color-primary)] focus:outline-none"
                >
                  <option value="">Selecione…</option>
                  {['AC','AL','AM','AP','BA','CE','DF','ES','GO','MA','MG','MS','MT',
                    'PA','PB','PE','PI','PR','RJ','RN','RO','RR','RS','SC','SE','SP','TO'].map(uf => (
                    <option key={uf}>{uf}</option>
                  ))}
                </select>
              </div>
            </div>
          </section>

          {/* Ações */}
          <div className="flex justify-end gap-3 pt-2 border-t border-[var(--color-border)]">
            <Button variant="secondary" onClick={() => setModalAberto(false)}>Cancelar</Button>
            <Button variant="primary" loading={salvando} onClick={salvar}>
              {editando ? 'Salvar alterações' : 'Cadastrar cliente'}
            </Button>
          </div>
        </div>
      </Modal>

      {/* ═══ Modal Importar em Massa ═══════════════════════════════════════════ */}
      <Modal
        open={modalImportar}
        onClose={() => setModalImportar(false)}
        title="Cadastro em massa — Importar planilha"
        size="max-w-lg"
      >
        <div className="flex flex-col gap-5">
          {/* Passo 1 — Baixar modelo */}
          <div className="rounded-lg border border-[var(--color-border)] p-4 flex flex-col gap-2">
            <p className="text-sm font-semibold text-[var(--color-text)]">
              Passo 1 — Baixe o modelo de planilha
            </p>
            <p className="text-xs text-[var(--color-text-muted)]">
              O arquivo já vem com as colunas corretas e uma aba de instruções. Não altere o cabeçalho.
            </p>
            <Button
              variant="secondary"
              loading={baixandoModelo}
              onClick={baixarModelo}
              className="flex items-center gap-2"
            >
              <IconDownload />
              {baixandoModelo ? 'Aguarde…' : 'Baixar modelo .xlsx'}
            </Button>
            {erroDownload && (
              <p className="text-xs text-[var(--color-error)]">{erroDownload}</p>
            )}
          </div>

          {/* Passo 2 — Upload */}
          <div className="rounded-lg border border-[var(--color-border)] p-4 flex flex-col gap-3">
            <p className="text-sm font-semibold text-[var(--color-text)]">
              Passo 2 — Envie a planilha preenchida
            </p>
            <input
              ref={fileRef}
              type="file"
              accept=".xlsx"
              className="hidden"
              onChange={e => setArquivo(e.target.files?.[0] ?? null)}
            />
            <div className="flex items-center gap-3">
              <Button variant="secondary" onClick={() => fileRef.current?.click()} className="flex items-center gap-2">
                <IconAttach />
                Selecionar arquivo
              </Button>
              {arquivoSelecionado && (
                <span className="text-sm text-[var(--color-text-muted)]">
                  {arquivoSelecionado.name}
                </span>
              )}
            </div>
            <Button
              variant="primary"
              loading={importando}
              disabled={!arquivoSelecionado}
              onClick={enviarPlanilha}
            >
              Enviar e importar
            </Button>
          </div>

          {/* Resultado */}
          {resultadoImport && (
            <div className="rounded-lg border border-[var(--color-border)] p-4 flex flex-col gap-2">
              <p className="text-sm font-semibold text-[var(--color-text)]">Resultado da importacao</p>
              <div className="flex gap-4 text-sm">
                <span className="text-green-400">{resultadoImport.sucesso} cadastrados com sucesso</span>
                {resultadoImport.falhas > 0 && (
                  <span className="text-red-400">{resultadoImport.falhas} com erro</span>
                )}
              </div>
              {resultadoImport.erros.length > 0 && (
                <div className="max-h-40 overflow-y-auto rounded border border-[var(--color-border)] bg-[var(--color-surface)] p-2">
                  {resultadoImport.erros.map((e, i) => (
                    <p key={i} className="text-xs text-red-400">
                      Linha {e.linha}{e.nome_completo ? ` (${e.nome_completo})` : ''}: {e.motivo}
                    </p>
                  ))}
                </div>
              )}
              {resultadoImport.sucesso > 0 && (
                <Button variant="secondary" onClick={() => setModalImportar(false)}>
                  Fechar
                </Button>
              )}
            </div>
          )}
        </div>
      </Modal>
    </div>
  );
}
