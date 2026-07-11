import { useState } from 'react';
import Login from './components/Login/Login';
import MessageList from './components/MessageList/MessageList';
import MessageInput from './components/MessageInput/MessageInput';
import ChannelStatusBar from './components/ChannelStatusBar/ChannelStatusBar';
import { useAuth } from './hooks/useAuth';
import useWebSocket from './hooks/useWebSocket';
import './App.css';

function App() {
  const { token, isAuthenticated, login, logout } = useAuth();
  const [activeTab, setActiveTab] = useState('incoming');
  const { socket } = useWebSocket(token);

  if (!isAuthenticated) {
    return <Login onLogin={login} />;
  }

  return (
    <div className="app-container" translate="no">
      <header className="app-header">
        <div className="header-top">
          <ChannelStatusBar socket={socket} />
          <button className="logout-button" onClick={logout} translate="no">
            Выйти
          </button>
        </div>
        <h1 translate="no">Спутниковый мессенджер</h1>
      </header>

      <nav className="tabs">
        <button
          className={`tab ${activeTab === 'incoming' ? 'active' : ''}`}
          onClick={() => setActiveTab('incoming')}
          translate="no"
        >
          Входящие
        </button>
        <button
          className={`tab ${activeTab === 'outgoing' ? 'active' : ''}`}
          onClick={() => setActiveTab('outgoing')}
          translate="no"
        >
          Исходящие
        </button>
        <button
          className={`tab ${activeTab === 'sent' ? 'active' : ''}`}
          onClick={() => setActiveTab('sent')}
          translate="no"
        >
          Отправленные
        </button>
      </nav>

      <MessageInput activeTab={activeTab} />
      <MessageList activeTab={activeTab} token={token} socket={socket} />
    </div>
  );
}

export default App;