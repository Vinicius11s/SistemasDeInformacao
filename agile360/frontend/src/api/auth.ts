import { api, ApiResponse } from './client';

export interface AuthResponse {
  access_token: string;
  refresh_token: string;
  expires_at: string;
  advogado?: { id: string; nome: string; email: string; numero_oab: string };
}

/**
 * Retornado pelo backend com status 202 quando o advogado tem MFA ativo.
 * O frontend deve redirecionar para /mfa-challenge com este token temporário.
 */
export interface MfaRequiredResponse {
  mfa_temp_token: string;
  expires_in_seconds: number;
}

/** Type guard: discrimina entre resposta completa (200) e desafio MFA (202). */
export function isMfaRequired(d: AuthResponse | MfaRequiredResponse): d is MfaRequiredResponse {
  return 'mfa_temp_token' in d;
}

/**
 * Resposta retornada pelo endpoint POST /api/auth/mfa/challenge.
 * O refreshToken é enviado via HttpOnly cookie — não aparece neste objeto.
 */
export interface SecureAuthResponse {
  accessToken: string;
  expiresInSeconds: number;
  advogado?: { id: string; nome: string; email: string; oab?: string; fotoUrl?: string };
}

export interface Profile {
  id: string;
  nome: string;
  email: string;
  oab: string;
  telefone?: string;
}

export async function login(
  email: string,
  password: string
): Promise<ApiResponse<AuthResponse | MfaRequiredResponse>> {
  return api.post<AuthResponse | MfaRequiredResponse>('/api/auth/login', { email, password });
}

export async function register(payload: {
  nome: string;
  email: string;
  password: string;
  oab?: string;
  telefone?: string;
}): Promise<ApiResponse<AuthResponse>> {
  return api.post<AuthResponse>('/api/auth/register', payload);
}

export async function refresh(refreshToken: string): Promise<ApiResponse<AuthResponse>> {
  // Backend espera "refresh_token" (snake_case via JsonNamingPolicy.SnakeCaseLower)
  return api.post<AuthResponse>('/api/auth/refresh', { refresh_token: refreshToken });
}

export async function getMe(token: string): Promise<ApiResponse<Profile>> {
  return api.get<Profile>('/api/auth/me', token);
}

export async function forgotPassword(email: string): Promise<ApiResponse<unknown>> {
  return api.post('/api/auth/forgot-password', { email });
}

export async function resetPassword(
  token: string,
  newPassword: string
): Promise<ApiResponse<unknown>> {
  // Backend espera "new_password" (snake_case via JsonNamingPolicy.SnakeCaseLower)
  return api.post('/api/auth/reset-password', { token, new_password: newPassword });
}
