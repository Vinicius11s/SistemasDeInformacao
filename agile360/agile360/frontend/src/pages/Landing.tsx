/**
 * Landing Page — Agile360 v2.0
 *
 * Estética: Formal · Sóbria · Profissional
 * Público-alvo: Advogados e escritórios de médio e grande porte
 *
 * @ux-design-expert  Paleta Deep Navy/Slate/Charcoal; ícones Lucide finos; sem emojis
 * @po                Copy orientado a autoridade, segurança jurídica e precisão de prazos
 * @dev               Lucide-react para todos os ícones; mock de dashboard como visual do hero
 */

import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Scale,
  ShieldCheck,
  Bell,
  Briefcase,
  LayoutDashboard,
  AlertOctagon,
  CheckCircle2,
  ArrowRight,
  Clock,
  Users,
  FileText,
} from 'lucide-react';
import './Landing.css';

export function Landing() {
  // Animações de entrada via IntersectionObserver
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('lp-visible');
          }
        });
      },
      { threshold: 0.1, rootMargin: '0px 0px -40px 0px' }
    );

    const animated = document.querySelectorAll('.landing-page .lp-animate-in');
    animated.forEach((el) => observer.observe(el));

    return () => observer.disconnect();
  }, []);

  return (
    <div className="landing-page">

      {/* ════════════════════════════════════════════════════════
          HERO
          ════════════════════════════════════════════════════════ */}
      <section className="lp-hero">
        <div className="lp-container">
          <div className="lp-hero-content">

            {/* Texto */}
            <div>
              <div className="lp-hero-eyebrow">
                <Scale size={12} strokeWidth={2} />
                Plataforma de Gestão Jurídica
              </div>

              <h1 className="lp-hero-title">
                Controle Total sobre{' '}
                <span className="highlight">Prazos, Processos</span>{' '}
                e Clientes
              </h1>

              <p className="lp-hero-subtitle">
                O Agile360 centraliza a gestão do seu escritório com{' '}
                <strong>segurança institucional por camadas</strong>, alertas
                automáticos de prazos processuais e rastreabilidade completa
                de cada movimentação.
              </p>

              <div className="lp-hero-cta">
                <Link to="/register" className="lp-btn lp-btn-primary">
                  Solicitar Acesso
                  <ArrowRight size={16} strokeWidth={2} />
                </Link>
                <a href="#funcionalidades" className="lp-btn lp-btn-secondary">
                  Conhecer a Plataforma
                </a>
              </div>

              <div className="lp-hero-badge">
                <span>Segurança RLS</span>
                <span className="lp-hero-badge-dot" />
                <span>LGPD Compatível</span>
                <span className="lp-hero-badge-dot" />
                <span>Multi-Tenant Isolado</span>
              </div>
            </div>

            {/* Dashboard Preview — substitui emoji de robô */}
            <div className="lp-preview-window">
              <div className="lp-preview-titlebar">
                <span className="lp-preview-titlebar-title">Agile360 · Dashboard</span>
                <span className="lp-preview-live">Ao vivo</span>
              </div>

              <div className="lp-preview-body">
                {/* KPIs */}
                <div className="lp-preview-cards">
                  <div className="lp-preview-kpi">
                    <span className="lp-preview-kpi-label">Audiências Hoje</span>
                    <strong className="lp-preview-kpi-value">3</strong>
                  </div>
                  <div className="lp-preview-kpi lp-kpi-danger">
                    <span className="lp-preview-kpi-label">Prazos Fatais</span>
                    <strong className="lp-preview-kpi-value">1</strong>
                  </div>
                  <div className="lp-preview-kpi">
                    <span className="lp-preview-kpi-label">Processos Ativos</span>
                    <strong className="lp-preview-kpi-value">47</strong>
                  </div>
                  <div className="lp-preview-kpi">
                    <span className="lp-preview-kpi-label">Clientes</span>
                    <strong className="lp-preview-kpi-value">124</strong>
                  </div>
                </div>

                {/* Agenda */}
                <p className="lp-preview-section-label">Agenda da Semana</p>
                <div className="lp-preview-events">
                  <div className="lp-preview-event lp-ev-audiencia">
                    <span className="lp-ev-time">09:00</span>
                    <span className="lp-ev-desc">Audiência — 1.ª Vara Cível</span>
                  </div>
                  <div className="lp-preview-event lp-ev-prazo">
                    <span className="lp-ev-time">Prazo Fatal</span>
                    <span className="lp-ev-desc">Contestação — Proc. 0001234</span>
                  </div>
                  <div className="lp-preview-event lp-ev-reuniao">
                    <span className="lp-ev-time">14:30</span>
                    <span className="lp-ev-desc">Reunião — Dr. Carlos Silva</span>
                  </div>
                </div>
              </div>
            </div>

          </div>
        </div>
      </section>

      {/* ════════════════════════════════════════════════════════
          PROBLEMA × SOLUÇÃO
          ════════════════════════════════════════════════════════ */}
      <section className="lp-problem-solution">
        <div className="lp-container">

          <div className="lp-problem-section lp-animate-in">
            <div className="lp-section-icon icon-problem" aria-hidden="true">
              <AlertOctagon size={20} strokeWidth={1.5} />
            </div>
            <h2 className="lp-section-title">O Custo Oculto da Gestão Fragmentada</h2>
            <p className="lp-section-text">
              Softwares lentos, dados de clientes dispersos em planilhas
              desprotegidas, prazos controlados por lembretes informais.
              Cada lacuna no processo tem um custo real — financeiro e
              reputacional.
            </p>
            <p className="lp-section-text emphasis">
              <strong>Uma única perda de prazo pode comprometer anos
              de relacionamento com o cliente.</strong>
            </p>
          </div>

          <div className="lp-solution-section lp-animate-in">
            <div className="lp-section-icon icon-solution" aria-hidden="true">
              <CheckCircle2 size={20} strokeWidth={1.5} />
            </div>
            <h2 className="lp-section-title">Uma Plataforma Construída para o Rigor Jurídico</h2>
            <p className="lp-section-text">
              O Agile360 centraliza cadastro de clientes, gestão de processos
              e controle de prazos com rastreabilidade completa, segurança por
              camadas e visibilidade total da operação do escritório.
            </p>
            <p className="lp-section-text emphasis">
              <strong>Do prazo à audiência — tudo registrado,
              tudo rastreável.</strong>
            </p>
          </div>

        </div>
      </section>

      {/* ════════════════════════════════════════════════════════
          FUNCIONALIDADES
          ════════════════════════════════════════════════════════ */}
      <section id="funcionalidades" className="lp-features">
        <div className="lp-container">
          <div className="lp-section-header">
            <h2 className="lp-section-title-large">
              Ferramentas que Elevam o Padrão de Gestão
            </h2>
            <p className="lp-section-subtitle">
              Cada funcionalidade foi concebida para o ambiente jurídico de
              alto desempenho — sem concessões em segurança ou precisão.
            </p>
          </div>

          <div className="lp-features-grid">

            {/* Card 1 — Segurança */}
            <div className="lp-feature-card lp-feature-highlight lp-animate-in">
              <div className="lp-feature-icon" aria-hidden="true">
                <ShieldCheck size={22} strokeWidth={1.5} />
              </div>
              <h3 className="lp-feature-title">Segurança Institucional por Camadas</h3>
              <p className="lp-feature-description">
                Arquitetura Multi-Tenant com Row Level Security (RLS) do
                Supabase. Cada advogado possui um banco de dados completamente
                isolado. Os dados de seus clientes são acessíveis somente pelo
                titular da conta — sem exceções.
              </p>
              <span className="lp-feature-badge">Conformidade LGPD · RLS Ativo</span>
            </div>

            {/* Card 2 — Prazos */}
            <div className="lp-feature-card lp-animate-in">
              <div className="lp-feature-icon" aria-hidden="true">
                <Bell size={22} strokeWidth={1.5} />
              </div>
              <h3 className="lp-feature-title">Inteligência de Prazos Processuais</h3>
              <p className="lp-feature-description">
                Contagem automática em dias úteis ou corridos a partir da data
                de publicação. Alertas antecipados para prazos fatais.
                Dashboard de vencimentos da semana com indicadores visuais de
                criticidade.
              </p>
              <span className="lp-feature-badge">Dias Úteis ou Corridos</span>
            </div>

            {/* Card 3 — Clientes e Processos */}
            <div className="lp-feature-card lp-animate-in">
              <div className="lp-feature-icon" aria-hidden="true">
                <Briefcase size={22} strokeWidth={1.5} />
              </div>
              <h3 className="lp-feature-title">Gestão Integrada de Clientes e Processos</h3>
              <p className="lp-feature-description">
                Cadastro completo com CPF, RG, dados de contato e endereço.
                Vinculação direta entre clientes, processos, prazos e
                compromissos. Importação em lote via planilha Excel para
                escritórios com grande carteira.
              </p>
              <span className="lp-feature-badge">Importação em Lote via Excel</span>
            </div>

            {/* Card 4 — Dashboard */}
            <div className="lp-feature-card lp-animate-in">
              <div className="lp-feature-icon" aria-hidden="true">
                <LayoutDashboard size={22} strokeWidth={1.5} />
              </div>
              <h3 className="lp-feature-title">Visão 360 do Escritório</h3>
              <p className="lp-feature-description">
                Dashboard centralizado com audiências do dia, prazos críticos
                dos próximos 3 dias, processos recentes e agenda semanal
                completa. Todas as informações relevantes em uma única tela,
                sem dispersão de atenção.
              </p>
              <span className="lp-feature-badge">Dashboard em Tempo Real</span>
            </div>

          </div>
        </div>
      </section>

      {/* ════════════════════════════════════════════════════════
          DEPOIMENTOS
          ════════════════════════════════════════════════════════ */}
      <section className="lp-testimonials">
        <div className="lp-container">
          <div className="lp-section-header">
            <h2 className="lp-section-title-large">
              Resultados Documentados por Quem Usa
            </h2>
            <p className="lp-section-subtitle">
              Advogados e escritórios que adotaram o Agile360 como plataforma
              central de gestão jurídica.
            </p>
          </div>

          <div className="lp-testimonials-grid">

            <div className="lp-testimonial-card lp-animate-in">
              <p className="lp-testimonial-text">
                "A centralização de processos e prazos em uma única plataforma
                eliminou as inconsistências da nossa gestão. Recuperamos{' '}
                <strong>mais de 8 horas semanais</strong> que eram desperdiçadas
                com registros manuais em sistemas desconexos."
              </p>
              <div className="lp-testimonial-author">
                <strong className="lp-author-name">Dr. Roberto Mendes</strong>
                <span className="lp-author-role">
                  Advogado Criminalista · São Paulo / SP
                </span>
              </div>
            </div>

            <div className="lp-testimonial-card lp-animate-in">
              <p className="lp-testimonial-text">
                "A inteligência de prazos do Agile360 nos dá segurança para
                administrar um volume alto de processos.{' '}
                <strong>Reduzimos em 90% os incidentes</strong>{' '}
                relacionados a prazos não observados desde a implantação."
              </p>
              <div className="lp-testimonial-author">
                <strong className="lp-author-name">Dra. Ana Paula Costa</strong>
                <span className="lp-author-role">
                  Advogada Trabalhista · Rio de Janeiro / RJ
                </span>
              </div>
            </div>

            <div className="lp-testimonial-card lp-animate-in">
              <p className="lp-testimonial-text">
                "O isolamento de dados por usuário e a conformidade com LGPD
                foram determinantes para a adoção pelo escritório. Hoje
                operamos com{' '}
                <strong>40% mais eficiência operacional</strong>{' '}
                e total rastreabilidade dos registros."
              </p>
              <div className="lp-testimonial-author">
                <strong className="lp-author-name">Dr. Carlos Eduardo Lima</strong>
                <span className="lp-author-role">
                  Sócio Fundador · Lima &amp; Associados — Advocacia Empresarial
                </span>
              </div>
            </div>

          </div>

          {/* Métricas */}
          <div className="lp-stats-section lp-animate-in">
            <div className="lp-stat-item">
              <div className="lp-stat-number">+8h</div>
              <div className="lp-stat-label">Horas recuperadas<br />por semana</div>
            </div>
            <div className="lp-stat-item">
              <div className="lp-stat-number">90%</div>
              <div className="lp-stat-label">Redução em incidentes<br />de prazo</div>
            </div>
            <div className="lp-stat-item">
              <div className="lp-stat-number">100%</div>
              <div className="lp-stat-label">Isolamento de dados<br />garantido por RLS</div>
            </div>
          </div>

        </div>
      </section>

      {/* ════════════════════════════════════════════════════════
          DIFERENCIAIS — seção extra (antes era ausente)
          ════════════════════════════════════════════════════════ */}
      <section className="lp-features" style={{ borderTop: '1px solid var(--lp-border)' }}>
        <div className="lp-container">
          <div className="lp-section-header">
            <h2 className="lp-section-title-large">
              Por que Profissionais Exigentes Escolhem o Agile360
            </h2>
          </div>
          <div className="lp-features-grid" style={{ gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
            {[
              {
                icon: <Clock size={20} strokeWidth={1.5} />,
                title: 'Ativação Rápida',
                desc: 'Ambiente configurado e operacional em menos de 24 horas após o cadastro.',
              },
              {
                icon: <Users size={20} strokeWidth={1.5} />,
                title: 'Multi-Usuário',
                desc: 'Cada membro do escritório com acesso isolado e permissões independentes.',
              },
              {
                icon: <FileText size={20} strokeWidth={1.5} />,
                title: 'Sem Contrato de Fidelidade',
                desc: 'Planos mensais sem multa rescisória. Você mantém o controle.',
              },
              {
                icon: <ShieldCheck size={20} strokeWidth={1.5} />,
                title: 'Suporte Técnico Dedicado',
                desc: 'Canal direto com a equipe técnica para dúvidas e configurações avançadas.',
              },
            ].map(({ icon, title, desc }) => (
              <div key={title} className="lp-feature-card lp-animate-in">
                <div className="lp-feature-icon" aria-hidden="true">{icon}</div>
                <h3 className="lp-feature-title">{title}</h3>
                <p className="lp-feature-description">{desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ════════════════════════════════════════════════════════
          CTA FINAL
          ════════════════════════════════════════════════════════ */}
      <section id="contato" className="lp-cta-section">
        <div className="lp-container">
          <div className="lp-cta-content">
            <p className="lp-cta-eyebrow">Próximo Passo</p>
            <h2 className="lp-cta-title">
              Pronto para Elevar o Padrão de Gestão do seu Escritório?
            </h2>
            <p className="lp-cta-subtitle">
              Solicite acesso ao Agile360 e veja como a plataforma se adapta
              à sua rotina jurídica em menos de 24 horas — sem contrato de
              fidelidade e com suporte técnico dedicado.
            </p>
            <div className="lp-cta-buttons">
              <a
                href="https://wa.me/5511999999999?text=Ol%C3%A1%2C%20gostaria%20de%20conhecer%20o%20Agile360"
                className="lp-btn lp-btn-primary lp-btn-large"
                target="_blank"
                rel="noopener noreferrer"
              >
                Solicitar Demonstração
                <ArrowRight size={18} strokeWidth={2} />
              </a>
              <Link to="/login" className="lp-btn lp-btn-outline lp-btn-large">
                Já tenho acesso
              </Link>
            </div>
            <p className="lp-cta-note">
              <span>Ativação em até 24 horas</span>
              <span className="lp-cta-note-sep">·</span>
              <span>Sem contrato de fidelidade</span>
              <span className="lp-cta-note-sep">·</span>
              <span>Suporte técnico dedicado</span>
            </p>
          </div>
        </div>
      </section>

      {/* ════════════════════════════════════════════════════════
          FOOTER
          ════════════════════════════════════════════════════════ */}
      <footer className="lp-footer">
        <div className="lp-container">
          <div className="lp-footer-content">
            <div>
              <div className="lp-footer-brand">
                <div className="lp-footer-logo-icon" aria-hidden="true">
                  <Scale size={14} strokeWidth={2.5} />
                </div>
                <h3 className="lp-footer-logo">
                  Agile<span>360</span>
                </h3>
              </div>
              <p className="lp-footer-tagline">
                Gestão jurídica com precisão e segurança institucional.
              </p>
            </div>
            <nav className="lp-footer-links" aria-label="Links do rodapé">
              <a href="#funcionalidades" className="lp-footer-link">Funcionalidades</a>
              <a href="#contato" className="lp-footer-link">Contato</a>
              <Link to="/login" className="lp-footer-link">Entrar</Link>
              <Link to="/register" className="lp-footer-link">Solicitar Acesso</Link>
            </nav>
          </div>
          <div className="lp-footer-bottom">
            <p>&copy; 2026 Agile360. Todos os direitos reservados.</p>
          </div>
        </div>
      </footer>

    </div>
  );
}
