import { useCallback, useState } from 'react';
import {
  getStoredToken,
  setStoredToken,
  clearStoredToken,
} from '../services/authStorage';

export function useAuth() {
  const [token, setToken] = useState(() => getStoredToken());

  const login = useCallback((newToken) => {
    setStoredToken(newToken);
    setToken(newToken);
  }, []);

  const logout = useCallback(() => {
    clearStoredToken();
    setToken(null);
  }, []);

  return {
    token,
    isAuthenticated: Boolean(token),
    login,
    logout,
  };
}