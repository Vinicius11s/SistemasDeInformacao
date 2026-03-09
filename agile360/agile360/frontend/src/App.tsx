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
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AuthProvider>
  );
}

export default App;
