import { api, ApiResponse } from './client';
import { SecureAuthResponse } from './auth';

export interface MfaStatusResponse {
  mfaEnabled: boolean;
}

export interface MfaSetupResponse {
  qrCodeUrl: string;
  manualEntryKey: string;
  mfaEnabled: boolean;
}

export interface MfaRequiredResponse {
  mfaTempToken: string;
  expiresInSeconds: number;
}

export async function getMfaStatus(token: string): Promise<ApiResponse<MfaStatusResponse>> {
  return api.get<MfaStatusResponse>('/api/auth/mfa/status', token);
}

export async function beginMfaSetup(token: string): Promise<ApiResponse<MfaSetupResponse>> {
  return api.post<MfaSetupResponse>('/api/auth/mfa/setup', {}, token);
}

export async function verifyMfaSetup(
  code: string,
  token: string
): Promise<ApiResponse<MfaStatusResponse>> {
  return api.post<MfaStatusResponse>('/api/auth/mfa/verify-setup', { code }, token);
}

export async function disableMfa(
  code: string,
  token: string
): Promise<ApiResponse<MfaStatusResponse>> {
  // DELETE with body — the backend reads [FromBody] MfaDisableRequest
  return api.deleteWithBody<MfaStatusResponse>('/api/auth/mfa/disable', { code }, token);
}

export async function mfaChallenge(
  mfaTempToken: string,
  code: string
): Promise<ApiResponse<SecureAuthResponse>> {
  return api.post<SecureAuthResponse>('/api/auth/mfa/challenge', { mfaTempToken, code });
}
