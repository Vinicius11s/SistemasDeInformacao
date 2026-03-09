import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from 'react';

// ─── Tipos ────────────────────────────────────────────────────────────────────
export type Theme = 'dark' | 'light';

interface ThemeContextValue {
  theme:       Theme;
  toggleTheme: () => void;
  isDark:      boolean;
}

// ─── Context ──────────────────────────────────────────────────────────────────
const ThemeContext = createContext<ThemeContextValue>({
  theme:       'dark',
  toggleTheme: () => {},
  isDark:      true,
});

// ─── Chave de armazenamento ───────────────────────────────────────────────────
const STORAGE_KEY = 'agile360-theme';

function readStoredTheme(): Theme {
  try {
    const v = localStorage.getItem(STORAGE_KEY);
    return v === 'light' ? 'light' : 'dark';
  } catch {
    return 'dark';
  }
}

// ─── Provider ────────────────────────────────────────────────────────────────
export function ThemeProvider({ children }: { children: ReactNode }) {
  // Inicializa a partir do localStorage (o script inline no index.html já
  // aplicou o data-theme antes do React montar, evitando o flash).
  const [theme, setTheme] = useState<Theme>(readStoredTheme);

  // Sincroniza o atributo data-theme no <html> e o localStorage
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
    try { localStorage.setItem(STORAGE_KEY, theme); } catch { /* ignorar */ }
  }, [theme]);

  const toggleTheme = useCallback(() => {
    setTheme(prev => (prev === 'dark' ? 'light' : 'dark'));
  }, []);

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme, isDark: theme === 'dark' }}>
      {children}
    </ThemeContext.Provider>
  );
}

// ─── Hook ─────────────────────────────────────────────────────────────────────
export const useTheme = () => useContext(ThemeContext);
