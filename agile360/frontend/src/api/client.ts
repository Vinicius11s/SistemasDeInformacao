const API_BASE = import.meta.env.VITE_API_URL || '';

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: { message: string; code?: string; statusCode?: number };
  timestamp?: string;
}

async function request<T>(
  path: string,
  options: RequestInit & { token?: string } = {}
): Promise<ApiResponse<T>> {
  const { token, ...init } = options;
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(init.headers as Record<string, string>),
  };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}${path}`, { ...init, headers });
  const json = await res.json().catch(() => ({}));

  if (!res.ok) {
    return {
      success: false,
      error: {
        message: json?.error?.message || res.statusText || 'Erro na requisição',
        statusCode: res.status,
      },
    };
  }
  return { success: true, data: json.data ?? json };
}

export const api = {
  async post<T>(path: string, body: unknown, token?: string) {
    return request<T>(path, { method: 'POST', body: JSON.stringify(body), token });
  },
  async get<T>(path: string, token?: string) {
    return request<T>(path, { method: 'GET', token });
  },
};
