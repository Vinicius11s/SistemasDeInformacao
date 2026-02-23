import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Card } from '../components/Card';
import { useAuth } from '../context/AuthContext';

export function Register() {
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [oab, setOab] = useState('');
  const [telefone, setTelefone] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { register: doRegister } = useAuth();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    const result = await doRegister({ nome, email, password, oab: oab || undefined, telefone: telefone || undefined });
    setLoading(false);
    if (result.ok) {
      navigate('/app', { replace: true });
    } else {
      setError(result.error ?? 'Falha no cadastro.');
    }
  }

  return (
    <div className="mx-auto flex max-w-md flex-col items-center justify-center px-6 py-12">
      <Card className="w-full">
        <h1 className="mb-6 text-2xl font-semibold text-[var(--color-text)]">Cadastrar</h1>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <Input
            label="Nome"
            name="nome"
            autoComplete="name"
            value={nome}
            onChange={(e) => setNome(e.target.value)}
            required
          />
          <Input
            label="E-mail"
            type="email"
            name="email"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          <Input
            label="Senha"
            type="password"
            name="password"
            autoComplete="new-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          <Input label="OAB" name="oab" value={oab} onChange={(e) => setOab(e.target.value)} />
          <Input
            label="Telefone"
            type="tel"
            name="telefone"
            value={telefone}
            onChange={(e) => setTelefone(e.target.value)}
          />
          {error && (
            <p className="text-sm text-[var(--color-error)]" role="alert" aria-live="assertive">
              {error}
            </p>
          )}
          <Button type="submit" loading={loading} className="mt-2">
            Cadastrar
          </Button>
          <Link to="/login" className="text-center text-sm text-[var(--color-text-muted)] hover:text-[var(--color-text)]">
            Já tem conta? Entrar
          </Link>
        </form>
      </Card>
    </div>
  );
}
