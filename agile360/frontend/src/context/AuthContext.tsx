import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import * as authApi from '../api/auth';
import type { Profile } from '../api/auth';
import { mfaChallenge, mfaChallengeWithRecovery } from '../api/mfa';

const TOKEN_KEY = 'agile360_token';
const REFRESH_KEY = 'agile360_refresh';

type AuthState = {
  token: string | null;
  refreshToken: string | null;
  user: Profile | null;
  loading: boolean;
};

const defaultState: AuthState = {
  token: null,
  refreshToken: null,
  user: null,
  loading: true,
};

const AuthContext = createContext<{
  state: AuthState;
  login: (email: string, password: string) => Promise<{ ok: boolean; error?: string }>;
  register: (payload: Parameters<typeof authApi.register>[0]) => Promise<{ ok: boolean; error?: string }>;
  logout: () => void;
  setTokens: (access: string, refresh: string) => void;
  completeMfaChallenge: (mfaTempToken: string, code: string) => Promise<{ ok: boolean; error?: string }>;
  completeMfaChallengeWithRecovery: (mfaTempToken: string, code: string) => Promise<{ ok: boolean; error?: string }>;
} | null>(null);

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) throw new Error('O useAuth precisa ser usado dentro de um AuthProvider');
    return context;
}
export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(defaultState);

  const setTokens = useCallback((access: string, refresh: string) => {
    localStorage.setItem(TOKEN_KEY, access);
    localStorage.setItem(REFRESH_KEY, refresh);
    setState((s) => ({ ...s, token: access, refreshToken: refresh, loading: false }));
  }, []);

  const loadUser = useCallback(async (token: string) => {
    const res = await authApi.getMe(token);
    if (res.success && res.data) {
      setState((s) => ({ ...s, user: res.data!, loading: false }));
    } else {
      setState((s) => ({ ...s, user: null, loading: false }));
    }
  }, []);

  useEffect(() => {
    const token = localStorage.getItem(TOKEN_KEY);
    const refresh = localStorage.getItem(REFRESH_KEY);
    if (!token) {
      setState((s) => ({ ...s, loading: false }));
      return;
    }
    setState((s) => ({ ...s, token, refreshToken: refresh }));
    loadUser(token).catch(() => setState((s) => ({ ...s, loading: false })));
  }, [loadUser]);

  const login = useCallback(
    async (email: string, password: string) => {
      const res = await authApi.login(email, password);
      if (!res.success) {
        return { ok: false, error: res.error?.message ?? 'E-mail ou senha inválidos.' };
      }
      const d = res.data!;
      setTokens(d.access_token, d.refresh_token ?? '');
      if (d.advogado) setState((s) => ({ ...s, user: d.advogado as unknown as Profile }));
      else await loadUser(d.access_token);
      return { ok: true };
    },
    [setTokens, loadUser]
  );

  const register = useCallback(
    async (payload: Parameters<typeof authApi.register>[0]) => {
      const res = await authApi.register(payload);
      if (!res.success) {
        return { ok: false, error: res.error?.message ?? 'Falha no cadastro.' };
      }
      const d = res.data!;
      setTokens(d.access_token, d.refresh_token ?? '');
      if (d.advogado) setState((s) => ({ ...s, user: d.advogado as unknown as Profile }));
      else await loadUser(d.access_token);
      return { ok: true };
    },
    [setTokens, loadUser]
  );

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    setState({ ...defaultState, loading: false });
  }, []);

  /**
   * Conclui o challenge MFA: valida o código TOTP, recebe o accessToken
   * e atualiza o estado de autenticação. O refreshToken chega via HttpOnly cookie.
   */
  const completeMfaChallenge = useCallback(
    async (mfaTempToken: string, code: string) => {
      const res = await mfaChallenge(mfaTempToken, code);
      if (!res.success) {
        return { ok: false, error: res.error?.message ?? 'Código inválido.' };
      }
      const d = res.data!;
      // Refresh token está no HttpOnly cookie — não precisa guardar no localStorage
      setTokens(d.accessToken, '');
      if (d.advogado) {
        setState((s) => ({
          ...s,
          user: { id: d.advogado!.id, nome: d.advogado!.nome, email: d.advogado!.email, oab: d.advogado!.oab ?? '' },
          loading: false,
        }));
      } else {
        await loadUser(d.accessToken);
      }
      return { ok: true };
    },
    [setTokens, loadUser]
  );

  /**
   * F7 — Conclui o challenge MFA com um Recovery Code de emergência.
   * Chama POST /api/auth/mfa/challenge/recovery.
   */
  const completeMfaChallengeWithRecovery = useCallback(
    async (mfaTempToken: string, code: string) => {
      const res = await mfaChallengeWithRecovery(mfaTempToken, code);
      if (!res.success) {
        return { ok: false, error: res.error?.message ?? 'Código de recuperação inválido ou já utilizado.' };
      }
      const d = res.data!;
      setTokens(d.accessToken, '');
      if (d.advogado) {
        setState((s) => ({
          ...s,
          user: { id: d.advogado!.id, nome: d.advogado!.nome, email: d.advogado!.email, oab: d.advogado!.oab ?? '' },
          loading: false,
        }));
      } else {
        await loadUser(d.accessToken);
      }
      return { ok: true };
    },
    [setTokens, loadUser]
  );

  const value = useMemo(
    () => ({ state, login, register, logout, setTokens, completeMfaChallenge, completeMfaChallengeWithRecovery }),
    [state, login, register, logout, setTokens, completeMfaChallenge, completeMfaChallengeWithRecovery]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useToken() {
  const { state } = useAuth();
  return state.token;
}
