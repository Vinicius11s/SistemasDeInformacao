import { useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Card } from '../components/Card';
import { resetPassword } from '../api/auth';

export function ResetPassword() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';
  const [newPassword, setNewPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (newPassword !== confirm) {
      setError('As senhas não coincidem.');
      return;
    }
    setError('');
    setLoading(true);
    const res = await resetPassword(token, newPassword);
    setLoading(false);
    if (res.success) {
      setSuccess(true);
    } else {
      setError(res.error?.message ?? 'Token inválido ou expirado.');
    }
  }

  if (success) {
    return (
      <div className="mx-auto max-w-md px-6 py-12">
        <Card>
          <p className="text-[var(--color-text)]">Senha alterada com sucesso.</p>
          <Link to="/login" className="mt-4 inline-block text-[var(--color-primary)] hover:underline">
            Ir para o login
          </Link>
        </Card>
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-md flex-col items-center justify-center px-6 py-12">
      <Card className="w-full">
        <h1 className="mb-6 text-2xl font-semibold text-[var(--color-text)]">Nova senha</h1>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <Input
            label="Nova senha"
            type="password"
            name="newPassword"
            autoComplete="new-password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            required
          />
          <Input
            label="Confirmar senha"
            type="password"
            name="confirm"
            autoComplete="new-password"
            value={confirm}
            onChange={(e) => setConfirm(e.target.value)}
            required
          />
          {error && (
            <p className="text-sm text-[var(--color-error)]" role="alert">
              {error}
            </p>
          )}
          <Button type="submit" loading={loading} disabled={!token} className="mt-2">
            Alterar senha
          </Button>
        </form>
        <Link to="/login" className="mt-4 block text-center text-sm text-[var(--color-primary)] hover:underline">
          Voltar ao login
        </Link>
      </Card>
    </div>
  );
}
