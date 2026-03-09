import { useState } from 'react';
import { Input } from '../Input';
import { Button } from '../Button';
import { Card } from '../Card';
import { maskCpf, maskCnpj, maskRg, maskPhone, rawDigits } from '../../utils/masks';
import { getLabels, type TipoPessoa } from '../../utils/clienteLabels';

interface Props {
  onClose: () => void;
}

const emptyForm = {
  tipoPessoa: 'PessoaFisica' as TipoPessoa,
  // PF fields
  nome: '',
  cpf: '',
  rg: '',
  orgaoExpedidor: '',
  // PJ fields
  razaoSocial: '',
  cnpj: '',
  inscricaoEstadual: '',
  // Shared PF/PJ — labels swap based on tipoPessoa
  dataReferencia: '',   // date input value (yyyy-mm-dd)
  areaAtuacao: '',
  // Contact
  email: '',
  telefone: '',
  whatsApp: '',
  endereco: '',
  observacoes: '',
};

export function ClienteFormModal({ onClose }: Props) {
  const [form, setForm] = useState(emptyForm);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const isPF = form.tipoPessoa === 'PessoaFisica';
  // Labels update instantly when the user switches TipoPessoa
  const labels = getLabels(form.tipoPessoa);

  function set(field: keyof typeof emptyForm, value: string) {
    setForm((f) => ({ ...f, [field]: value }));
  }

  function switchTipo(tipo: TipoPessoa) {
    // Reset document fields when switching to avoid stale CPF in CNPJ slot, etc.
    setForm((f) => ({
      ...f,
      tipoPessoa: tipo,
      nome: '',
      cpf: '',
      rg: '',
      orgaoExpedidor: '',
      razaoSocial: '',
      cnpj: '',
      inscricaoEstadual: '',
    }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');

    if (isPF && !form.nome.trim()) { setError('Nome é obrigatório.'); return; }
    if (!isPF && !form.razaoSocial.trim()) { setError('Razão Social é obrigatória.'); return; }
    if (!isPF && rawDigits(form.cnpj)?.length !== 14) { setError('CNPJ inválido.'); return; }

    const payload = {
      tipoPessoa: form.tipoPessoa,
      nome:              form.nome        || undefined,
      cpf:               rawDigits(form.cpf),
      rg:                rawDigits(form.rg),
      orgaoExpedidor:    form.orgaoExpedidor || undefined,
      razaoSocial:       form.razaoSocial   || undefined,
      cnpj:              rawDigits(form.cnpj),
      inscricaoEstadual: rawDigits(form.inscricaoEstadual),
      dataReferencia:    form.dataReferencia || undefined,
      areaAtuacao:       form.areaAtuacao    || undefined,
      email:             form.email          || undefined,
      telefone:          rawDigits(form.telefone),
      whatsAppNumero:    rawDigits(form.whatsApp),
      endereco:          form.endereco        || undefined,
      observacoes:       form.observacoes     || undefined,
    };

    setLoading(true);
    try {
      // TODO: chamar API quando endpoint estiver disponível (Epic 2)
      console.debug('[ClienteFormModal] payload:', payload);
      await new Promise((r) => setTimeout(r, 600));
      onClose();
    } catch {
      setError('Erro ao salvar cliente. Tente novamente.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 px-4"
      role="dialog"
      aria-modal="true"
      aria-label="Cadastro de cliente"
    >
      <Card className="w-full max-w-xl max-h-[90vh] overflow-y-auto">
        <div className="mb-6 flex items-center justify-between">
          <h2 className="text-xl font-semibold text-[var(--color-text)]">Novo cliente</h2>
          <button
            type="button"
            onClick={onClose}
            className="text-[var(--color-text-muted)] hover:text-[var(--color-text)]"
            aria-label="Fechar"
          >
            ✕
          </button>
        </div>

        {/* ── Seletor PF / PJ ── */}
        <fieldset className="mb-6">
          <legend className="mb-2 text-sm font-medium text-[var(--color-text-muted)]">
            Tipo de pessoa
          </legend>
          <div className="flex gap-4">
            {(['PessoaFisica', 'PessoaJuridica'] as TipoPessoa[]).map((tipo) => (
              <label
                key={tipo}
                className={`flex flex-1 cursor-pointer items-center justify-center gap-2 rounded-[var(--radius)] border px-4 py-3 text-sm font-medium transition-colors ${
                  form.tipoPessoa === tipo
                    ? 'border-[var(--color-primary)] bg-[var(--color-surface)] text-[var(--color-primary)]'
                    : 'border-[var(--color-border)] text-[var(--color-text-muted)] hover:border-[var(--color-primary)]'
                }`}
              >
                <input
                  type="radio"
                  name="tipoPessoa"
                  value={tipo}
                  checked={form.tipoPessoa === tipo}
                  onChange={() => switchTipo(tipo)}
                  className="sr-only"
                />
                {tipo === 'PessoaFisica' ? '👤 Pessoa Física' : '🏢 Pessoa Jurídica'}
              </label>
            ))}
          </div>
        </fieldset>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          {/* ── Campos exclusivos Pessoa Física ── */}
          {isPF && (
            <>
              <Input
                label={labels.nomeLabel}
                placeholder={labels.nomePlaceholder}
                value={form.nome}
                onChange={(e) => set('nome', e.target.value)}
                required
              />
              <div className="grid grid-cols-2 gap-4">
                <Input
                  label="CPF"
                  value={form.cpf}
                  onChange={(e) => set('cpf', maskCpf(e.target.value))}
                  placeholder="000.000.000-00"
                  maxLength={14}
                  inputMode="numeric"
                />
                <Input
                  label="RG"
                  value={form.rg}
                  onChange={(e) => set('rg', maskRg(e.target.value))}
                  placeholder="00.000.000-0"
                  maxLength={12}
                  inputMode="numeric"
                />
              </div>
              <Input
                label="Órgão Expedidor"
                value={form.orgaoExpedidor}
                onChange={(e) => set('orgaoExpedidor', e.target.value)}
                maxLength={20}
                placeholder="ex.: SSP/SP"
              />
            </>
          )}

          {/* ── Campos exclusivos Pessoa Jurídica ── */}
          {!isPF && (
            <>
              <Input
                label={labels.nomeLabel}
                placeholder={labels.nomePlaceholder}
                value={form.razaoSocial}
                onChange={(e) => set('razaoSocial', e.target.value)}
                required
              />
              <div className="grid grid-cols-2 gap-4">
                <Input
                  label="CNPJ *"
                  value={form.cnpj}
                  onChange={(e) => set('cnpj', maskCnpj(e.target.value))}
                  placeholder="00.000.000/0000-00"
                  maxLength={18}
                  inputMode="numeric"
                  required
                />
                <Input
                  label="Inscrição Estadual"
                  value={form.inscricaoEstadual}
                  onChange={(e) => set('inscricaoEstadual', e.target.value)}
                  maxLength={20}
                />
              </div>
              <Input
                label="Nome do responsável"
                value={form.nome}
                onChange={(e) => set('nome', e.target.value)}
                placeholder="Contato principal"
              />
            </>
          )}

          {/* ── Campos compartilhados — labels dinâmicos ── */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              {/* Label troca instantaneamente ao alternar PF/PJ */}
              <label className="mb-1 block text-sm text-[var(--color-text-muted)]">
                {labels.dataLabel}
              </label>
              <input
                type="date"
                value={form.dataReferencia}
                onChange={(e) => set('dataReferencia', e.target.value)}
                placeholder={labels.dataPlaceholder}
                className="w-full rounded-[var(--radius)] border border-[var(--color-border)] bg-[var(--color-surface)] px-3 py-2 text-sm text-[var(--color-text)] outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-primary)]"
              />
            </div>
            <Input
              label={labels.areaAtuacaoLabel}
              placeholder={labels.areaAtuacaoPlaceholder}
              value={form.areaAtuacao}
              onChange={(e) => set('areaAtuacao', e.target.value)}
              maxLength={200}
            />
          </div>

          {/* ── Contato (ambos) ── */}
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Telefone"
              value={form.telefone}
              onChange={(e) => set('telefone', maskPhone(e.target.value))}
              placeholder="(00) 00000-0000"
              maxLength={15}
              inputMode="tel"
            />
            <Input
              label="WhatsApp"
              value={form.whatsApp}
              onChange={(e) => set('whatsApp', maskPhone(e.target.value))}
              placeholder="(00) 00000-0000"
              maxLength={15}
              inputMode="tel"
            />
          </div>
          <Input
            label="E-mail"
            type="email"
            value={form.email}
            onChange={(e) => set('email', e.target.value)}
          />
          <Input
            label="Endereço"
            value={form.endereco}
            onChange={(e) => set('endereco', e.target.value)}
          />
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-muted)]">Observações</label>
            <textarea
              value={form.observacoes}
              onChange={(e) => set('observacoes', e.target.value)}
              rows={3}
              className="w-full resize-none rounded-[var(--radius)] border border-[var(--color-border)] bg-[var(--color-surface)] px-3 py-2 text-sm text-[var(--color-text)] outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-primary)]"
            />
          </div>

          {error && (
            <p className="text-sm text-[var(--color-error)]" role="alert">{error}</p>
          )}

          <div className="mt-2 flex gap-3">
            <Button type="button" variant="secondary" onClick={onClose} className="flex-1">
              Cancelar
            </Button>
            <Button type="submit" loading={loading} className="flex-1">
              Salvar cliente
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
