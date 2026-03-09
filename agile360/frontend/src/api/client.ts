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

  // REGRA CRÍTICA: 401 → sessão inválida/expirada → forçar logout e redirecionar para login
  // Não redirecionar se já estiver na tela de login (evita loop)
  if (res.status === 401 && !window.location.pathname.startsWith('/login')) {
    localStorage.removeItem('agile360_token');
    localStorage.removeItem('agile360_refresh');
    window.location.href = '/login';
    return { success: false, error: { message: 'Sessão expirada. Faça login novamente.', statusCode: 401 } } as ApiResponse<T>;
  }

  const json = await res.json().catch(() => ({}));

  if (!res.ok) {
    // json pode ser: { error: { message } }  →  padrão ApiResponse
    //               "string pura"             →  BadRequest("texto") sem wrapper
    //               {}                        →  fallback para statusText
    const message =
      (typeof json === 'string' ? json : json?.error?.message) ||
      res.statusText ||
      'Erro na requisição';
    return {
      success: false,
      error: { message, statusCode: res.status },
    };
  }
  return { success: true, data: json.data ?? json };
}

export const api = {
  async get<T>(path: string, token?: string) {
    return request<T>(path, { method: 'GET', token });
  },
  async post<T>(path: string, body: unknown, token?: string) {
    return request<T>(path, { method: 'POST', body: JSON.stringify(body), token });
  },
  async put<T>(path: string, body: unknown, token?: string) {
    return request<T>(path, { method: 'PUT', body: JSON.stringify(body), token });
  },
  async patch<T>(path: string, body: unknown, token?: string) {
    return request<T>(path, { method: 'PATCH', body: JSON.stringify(body), token });
  },
  async delete<T = void>(path: string, token?: string) {
    return request<T>(path, { method: 'DELETE', token });
  },
  /** DELETE com body JSON — usado por endpoints como DELETE /api/auth/mfa/disable. */
  async deleteWithBody<T = void>(path: string, body: unknown, token?: string) {
    return request<T>(path, { method: 'DELETE', body: JSON.stringify(body), token });
  },
  /** Multipart/form-data — para upload de arquivos */
  async postForm<T>(path: string, form: FormData, token?: string) {
    const headers: HeadersInit = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    // Não definir Content-Type — o browser define com boundary correto
    const res = await fetch(`${API_BASE}${path}`, { method: 'POST', body: form, headers });
    if (res.status === 401 && !window.location.pathname.startsWith('/login')) {
      localStorage.removeItem('agile360_token');
      localStorage.removeItem('agile360_refresh');
      window.location.href = '/login';
      return { success: false, error: { message: 'Sessão expirada. Faça login novamente.', statusCode: 401 } } as ApiResponse<T>;
    }
    const json = await res.json().catch(() => ({}));
    if (!res.ok) {
      const message =
        (typeof json === 'string' ? json : json?.error?.message) ||
        res.statusText ||
        'Erro na requisição';
      return { success: false, error: { message, statusCode: res.status } } as ApiResponse<T>;
    }
    return { success: true, data: json.data ?? json } as ApiResponse<T>;
  },
};
