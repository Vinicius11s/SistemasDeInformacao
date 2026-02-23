import { Outlet } from 'react-router-dom';
import { Navbar } from '../components/Navbar';
import { useAuth } from '../context/AuthContext';

export function PublicLayout() {
  const { state, logout } = useAuth();
  return (
    <div className="min-h-screen flex flex-col">
      <Navbar
        isAuthenticated={!!state.token}
        userName={state.user?.nome}
        onLogout={logout}
      />
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  );
}
