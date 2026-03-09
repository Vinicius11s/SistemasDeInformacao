import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    host: true, // acessível em localhost e 127.0.0.1 (evita "Não é possível acessar esse site" no Windows)
    proxy: {
      '/api': {
        // HTTP 5000 → compatível com `dotnet run` padrão (profile "http" do launchSettings.json).
        // Para usar HTTPS, troque por: target: 'https://localhost:5001', secure: false
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
});
