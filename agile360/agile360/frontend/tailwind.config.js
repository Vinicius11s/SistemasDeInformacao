/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        bg: 'var(--color-bg)',
        surface: 'var(--color-surface)',
        border: 'var(--color-border)',
        primary: 'var(--color-primary)',
        'primary-hover': 'var(--color-primary-hover)',
        text: 'var(--color-text)',
        'text-muted': 'var(--color-text-muted)',
        error: 'var(--color-error)',
        success: 'var(--color-success)',
      },
      fontFamily: {
        ui: ['var(--font-ui)'],
        mono: ['var(--font-mono)'],
      },
    },
  },
  plugins: [],
};
