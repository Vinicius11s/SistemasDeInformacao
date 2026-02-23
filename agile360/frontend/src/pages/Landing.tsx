import { Link } from 'react-router-dom';
import { Button } from '../components/Button';

const benefits = [
  {
    title: 'Gestão de clientes',
    description: 'Centralize dados, contatos e histórico em um só lugar.',
  },
  {
    title: 'Prazos e alertas',
    description: 'Nunca perca um prazo. Acompanhe prazos processuais e receba avisos.',
  },
  {
    title: 'Audiências e calendário',
    description: 'Organize audiências e compromissos com visão clara do que vem pela frente.',
  },
  {
    title: 'WhatsApp + IA',
    description: 'Capture informações de clientes via WhatsApp com extração inteligente.',
  },
  {
    title: 'Segurança jurídica',
    description: 'Dados isolados por advogado, auditoria e conformidade.',
  },
];

export function Landing() {
  return (
    <div>
      <section className="mx-auto max-w-4xl px-6 py-20 text-center">
        <h1
          className="mb-4 text-[var(--text-hero)] font-bold leading-[var(--line-height-tight)] text-[var(--color-text)]"
          style={{ fontFamily: 'var(--font-ui)' }}
        >
          Visão 360 e Esforço Zero
        </h1>
        <p className="mb-10 text-xl text-[var(--color-text-muted)]">
          CRM jurídico que centraliza clientes, processos e prazos. Para advogados que buscam
          agilidade e segurança.
        </p>
        <div className="flex flex-wrap justify-center gap-4">
          <Link to="/login">
            <Button variant="primary">Começar agora</Button>
          </Link>
          <Link to="/register">
            <Button variant="secondary">Cadastrar</Button>
          </Link>
        </div>
      </section>

      <section id="beneficios" className="border-t border-[var(--color-border)] bg-[var(--color-surface)] px-6 py-16">
        <h2 className="mb-10 text-center text-[var(--text-2xl)] font-semibold text-[var(--color-text)]">
          Benefícios
        </h2>
        <div className="mx-auto grid max-w-5xl grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {benefits.map((b) => (
            <div
              key={b.title}
              className="rounded-[var(--radius-lg)] border border-[var(--color-border)] bg-[var(--color-bg)] p-6"
            >
              <h3 className="mb-2 text-lg font-semibold text-[var(--color-text)]">{b.title}</h3>
              <p className="text-[var(--color-text-muted)]">{b.description}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
