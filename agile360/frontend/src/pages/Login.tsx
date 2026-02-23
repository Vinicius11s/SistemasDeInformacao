import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Card } from '../components/Card';
import { useAuth } from '../context/AuthContext';

export function Login() {
    const [email, setEmail] = useState(''); // Guarda o que o usuário digita no e-mail
    const [password, setPassword] = useState(''); // Guarda a senha
    const [error, setError] = useState(''); // Guarda mensagens de erro, se houver
    const [loading, setLoading] = useState(false); // Diz se o sistema está "carregando" o login

    const [searchParams] = useSearchParams(); // Pega os parâmetros da URL, como ?returnUrl=/app, para saber para onde ir depois do login
    const returnUrl = searchParams.get('returnUrl') ?? '/app'; // Se não tiver returnUrl, vai para /app por padrão
    const navigate = useNavigate(); // Para navegar para outra página depois do login bem-sucedido
    const { login } = useAuth(); // Pega a função de login do contexto de autenticação, que é onde a lógica de login realmente acontece

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setError('');
        setLoading(true);

        const result = await login(email, password);

        setLoading(false);
        if (result.ok) {
            navigate(returnUrl, { replace: true });
        }
        else {
            setError(result.error ?? 'E-mail ou senha inválidos.');
        }
    }

    return (
        <div className="mx-auto flex max-w-md flex-col items-center justify-center px-6 py-12">
            <Card className="w-full">
            <h1 className="mb-6 text-2xl font-semibold text-[var(--color-text)]">Entrar</h1>
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
                <Input
                label="Senha"
                type="password"
                name="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                />
                {error && (
                <p className="text-sm text-[var(--color-error)]" role="alert" aria-live="assertive">
                    {error}
                </p>
                )}
                <Button type="submit" loading={loading} className="mt-2">
                Entrar
                </Button>
                <Link
                to="/forgot-password"
                className="text-center text-sm text-[var(--color-primary)] hover:underline"
                >
                Esqueci a senha
                </Link>
            </form>
            </Card>
        </div>
    );
}
