import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Card } from '../components/Card';
import { forgotPassword } from '../api/auth';

export function ForgotPassword() {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setSent(false);
    await forgotPassword(email);
    setLoading(false);
    setSent(true);
  }

  return (
    <div className="mx-auto flex max-w-md flex-col items-center justify-center px-6 py-12">
      <Card className="w-full">
        <h1 className="mb-6 text-2xl font-semibold text-[var(--color-text)]">Esqueci a senha</h1>
        {sent ? (
          <p className="text-[var(--color-text-muted)]" role="status">
            Se o e-mail existir, você receberá um link para redefinir a senha.
          </p>
        ) : (
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <Input
              label="E-mail"
              type="email"
              name="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
            <Button type="submit" loading={loading} className="mt-2">
              Enviar link
            </Button>
          </form>
        )}
        <Link
          to="/login"
          className="mt-4 block text-center text-sm text-[var(--color-primary)] hover:underline"
        >
          Voltar ao login
        </Link>
      </Card>
    </div>
  );
}
