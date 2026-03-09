import { Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { PublicLayout } from './layouts/PublicLayout';
import { DashboardLayout } from './layouts/DashboardLayout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Landing } from './pages/Landing';
import { Login } from './pages/Login';
import { Register } from './pages/Register';
import { ForgotPassword } from './pages/ForgotPassword';
import { ResetPassword } from './pages/ResetPassword';
import { DashboardHome } from './pages/DashboardHome';
import { Clientes } from './pages/Clientes';
import { Processos } from './pages/Processos';
import { Audiencias } from './pages/Audiencias';
import { Prazos } from './pages/Prazos';
import { StagingClientes } from './pages/StagingClientes';
import { MfaChallenge } from './pages/MfaChallenge';
import { SecuritySettings } from './pages/SecuritySettings';
import { MinhaConta } from './pages/MinhaConta';
import { SettingsPlaceholder } from './pages/SettingsPlaceholder';
import { SettingsLayout } from './layouts/SettingsLayout';

function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route element={<PublicLayout />}>
          <Route path="/" element={<Landing />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/reset-password" element={<ResetPassword />} />
          <Route path="/mfa-challenge" element={<MfaChallenge />} />
        </Route>
        <Route
          path="/app"
          element={
            <ProtectedRoute>
              <DashboardLayout />
            </ProtectedRoute>
          }
        >
          <Route index element={<DashboardHome />} />
          <Route path="clientes" element={<Clientes />} />
          <Route path="processos" element={<Processos />} />
          <Route path="audiencias" element={<Audiencias />} />
          <Route path="prazos" element={<Prazos />} />
          <Route path="staging" element={<StagingClientes />} />
          <Route path="configuracoes" element={<SettingsLayout />}>
            <Route index element={<Navigate to="/app/configuracoes/minha-conta" replace />} />
            <Route path="minha-conta" element={<MinhaConta />} />
            <Route path="seguranca" element={<SecuritySettings />} />
            <Route path="notificacoes" element={<SettingsPlaceholder />} />
            <Route path="integracoes" element={<SettingsPlaceholder />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AuthProvider>
  );
}

export default App;
