import { api, ApiResponse } from './client';

export interface AuthResponse {
  access_token: string;
  refresh_token: string;
  expires_at: string;
  advogado?: { id: string; nome: string; email: string; numero_oab: string };
}

export interface Profile {
  id: string;
  nome: string;
  email: string;
  oab: string;
  telefone?: string;
}

export async function login(email: string, password: string): Promise<ApiResponse<AuthResponse>> {
  return api.post<AuthResponse>('/api/auth/login', { email, password });
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
  return api.post<AuthResponse>('/api/auth/refresh', { refreshToken });
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
  return api.post('/api/auth/reset-password', { token, newPassword });
}
