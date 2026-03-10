import { api, ApiResponse } from './client';
import { SecureAuthResponse } from './auth';

// ── Tipos de resposta ─────────────────────────────────────────────────────────

// NOTA: As interfaces abaixo usam snake_case para espelhar exatamente o que a API envia.
// O backend (Program.cs) configura JsonNamingPolicy.SnakeCaseLower — não há camada de
// transformação no client.ts, então as chaves devem ser idênticas ao JSON recebido.

export interface MfaStatusResponse {
  mfa_enabled: boolean;
}

export interface MfaSetupResponse {
  qr_code_url: string;
  manual_entry_key: string;
  mfa_enabled: boolean;
}

export interface MfaRequiredResponse {
  mfa_temp_token: string;
  expires_in_seconds: number;
}

/**
 * F1 — Resposta do verify-setup.
 * Contém o status de ativação + os 10 recovery codes em plaintext.
 * Os códigos são exibidos APENAS nesta resposta — única oportunidade de visualização.
 */
export interface MfaActivatedResponse {
  mfa_enabled: boolean;
  /** 10 códigos no formato XXXX-XXXX — texto limpo, exibição única */
  recovery_codes: string[];
}

/**
 * F1 — Resposta do GET /recovery-codes/count.
 * Quantos recovery codes não usados o advogado ainda possui.
 */
export interface RecoveryCodesCountResponse {
  remaining: number;
}

// ── Chamadas de API ───────────────────────────────────────────────────────────

export async function getMfaStatus(token: string): Promise<ApiResponse<MfaStatusResponse>> {
  return api.get<MfaStatusResponse>('/api/auth/mfa/status', token);
}

export async function beginMfaSetup(token: string): Promise<ApiResponse<MfaSetupResponse>> {
  return api.post<MfaSetupResponse>('/api/auth/mfa/setup', {}, token);
}

/**
 * Confirma o primeiro código TOTP — ativa o MFA e recebe os 10 recovery codes.
 * Retorna MfaActivatedResponse (não mais MfaStatusResponse) para incluir os códigos.
 */
export async function verifyMfaSetup(
  code: string,
  token: string
): Promise<ApiResponse<MfaActivatedResponse>> {
  return api.post<MfaActivatedResponse>('/api/auth/mfa/verify-setup', { code }, token);
}

export async function disableMfa(
  code: string,
  token: string
): Promise<ApiResponse<MfaStatusResponse>> {
  return api.deleteWithBody<MfaStatusResponse>('/api/auth/mfa/disable', { code }, token);
}

export async function mfaChallenge(
  mfaTempToken: string,
  code: string
): Promise<ApiResponse<SecureAuthResponse>> {
  return api.post<SecureAuthResponse>('/api/auth/mfa/challenge', { mfaTempToken, code });
}

/**
 * F2 — Retorna quantos recovery codes o advogado ainda tem disponíveis.
 * Usado na view de status para exibir badge "X de 10 códigos restantes".
 */
export async function getRecoveryCodesCount(
  token: string
): Promise<ApiResponse<RecoveryCodesCountResponse>> {
  return api.get<RecoveryCodesCountResponse>('/api/auth/mfa/recovery-codes/count', token);
}

/**
 * F2 — Usa um recovery code no fluxo de challenge MFA (alternativa ao TOTP).
 * Retorna o mesmo SecureAuthResponse que o challenge TOTP normal.
 */
export async function mfaChallengeWithRecovery(
  mfaTempToken: string,
  code: string
): Promise<ApiResponse<SecureAuthResponse>> {
  return api.post<SecureAuthResponse>(
    '/api/auth/mfa/challenge/recovery',
    { mfaTempToken, code }
  );
}

/**
 * F2 — Regenera os 10 recovery codes (invalida os anteriores).
 * Requer MFA ativo. Limitado a 3 req/hora no backend.
 */
export async function regenerateRecoveryCodes(
  token: string
): Promise<ApiResponse<{ codes: string[] }>> {
  return api.post<{ codes: string[] }>(
    '/api/auth/mfa/recovery-codes/generate',
    {},
    token
  );
}
