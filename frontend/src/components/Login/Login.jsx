import { useState } from 'react';
import './Login.css';

function Login({ onLogin }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/Auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
      });

      if (!response.ok) {
        throw new Error('Неверный логин или пароль');
      }

      const data = await response.json();
      onLogin(data.token); // передаём токен наверх
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-wrapper">
        <h2 className="login-title">Авторизация в Спутниковой системе Гонец</h2>
        <form onSubmit={handleSubmit} className="login-form">
        <input 
            type="text" 
            placeholder="Логин" 
            value={username} 
            onChange={e => setUsername(e.target.value)} 
            required 
        />
        <input 
            type="password" 
            placeholder="Пароль" 
            value={password} 
            onChange={e => setPassword(e.target.value)} 
            required 
        />
        <button type="submit" disabled={loading}>
            {loading ? 'Вход...' : 'Войти'}
        </button>
        {error && <p style={{color: 'red'}}>{error}</p>}
        </form>
    </div>
  );
}

export default Login;
