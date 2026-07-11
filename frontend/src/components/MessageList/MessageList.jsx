import { useEffect, useState, useCallback } from 'react';
import useMessages from '../../hooks/useMessages';
import './MessageList.css';
import { TYPE_MESSAGE } from '../../constants/messageTypes';
import MapModal from '../MapModal/MapModal';

const MessageList = ({ activeTab, token, socket }) => {
  const { messages, loading, error } = useMessages(activeTab);
  const [realTimeMessages, setRealTimeMessages] = useState([]);
  const [mapData, setMapData] = useState(null);
  const [currentlyPlaying, setCurrentlyPlaying] = useState(null);

  const formatDateTime = useCallback((dateStr, timeStr) => {
    try {
      const date = new Date(`${dateStr}T${timeStr}`);
      return date.toLocaleString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return `${dateStr} ${timeStr}`;
    }
  }, []);

  const playAudio = useCallback(async (audioKey, rawMessage = null) => {
    try {
      console.log('AUDIO rawMessage =', rawMessage);
      console.log('AUDIO audioKey =', audioKey);

      if (!audioKey) {
        console.error('Нет идентификатора аудио', rawMessage);
        return;
      }

      if (currentlyPlaying === audioKey) return;

      setCurrentlyPlaying(audioKey);

      const requestUrl = `${import.meta.env.VITE_API_BASE_URL}/api/audio/${audioKey}`;
      console.log('AUDIO requestUrl =', requestUrl);

      const response = await fetch(requestUrl, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      console.log('AUDIO response.status =', response.status);

      if (!response.ok) {
        throw new Error(`Ошибка загрузки аудио: ${response.status}`);
      }

      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const audio = new Audio(url);

      audio.onended = () => {
        URL.revokeObjectURL(url);
        setCurrentlyPlaying(null);
      };

      audio.onerror = () => {
        URL.revokeObjectURL(url);
        setCurrentlyPlaying(null);
        console.error('Ошибка воспроизведения аудио');
      };

      await audio.play();
    } catch (error) {
      setCurrentlyPlaying(null);
      console.error('Ошибка воспроизведения:', error);
    }
  }, [currentlyPlaying, token]);

  useEffect(() => {
    if (!socket) return;

    const handleMessage = (event) => {
      try {
        const message = JSON.parse(event.data);
        if (message.type === `${activeTab}_message`) {
          setRealTimeMessages(prev => [message, ...prev]);
        }
      } catch (e) {
        console.error('Ошибка обработки сообщения:', e);
      }
    };

    socket.addEventListener('message', handleMessage);

    return () => socket.removeEventListener('message', handleMessage);
  }, [socket, activeTab]);

  if (loading) return <div className="loading">Загрузка сообщений...</div>;
  if (error) return <div className="error">Ошибка: {error}</div>;

  const displayedMessages = [...realTimeMessages, ...messages];
  const coordinatesMessages = displayedMessages.filter(
    m => m.typeMessage === TYPE_MESSAGE.Coordinates
  );

  const normalMessages = displayedMessages.filter(
    m => m.typeMessage !== TYPE_MESSAGE.Coordinates
  );
  
  const title = {
    incoming: 'Входящие сообщения',
    outgoing: 'Исходящие сообщения',
    sent: 'Отправленные сообщения'
  }[activeTab] || 'Сообщения';

  // Собираем все координаты для карты
  const handleOpenMap = () => {
    const allPoints = coordinatesMessages.flatMap(msg => {
      try {
        const parsed = JSON.parse(msg.content);
        return Array.isArray(parsed) ? parsed : [parsed];
      } catch (e) {
        console.error('Ошибка парсинга координат:', e);
        return [];
      }
    });

    if (allPoints.length === 0) {
      alert('Нет координат для отображения');
      return;
    }

    setMapData({ points: allPoints });
  };

  return (
    <>
      <div className="map-button-wrapper">
        <button
          className="map-button-global"
          onClick={handleOpenMap}
        >
          Открыть карту ({coordinatesMessages.length})
        </button>
      </div>

      <div className="message-container">
        <h2>{title}</h2>
        
        {displayedMessages.length === 0 ? (
          <p className="no-messages">Нет сообщений</p>
        ) : (
          <ul className="message-list">
            {normalMessages.map((msg) => (
              <li key={msg.messageId} className="message-item">
                <div className="message-header">
                  <span className="message-sender">
                    {activeTab === 'incoming' ? `От: ${msg.from}` : 
                     activeTab === 'outgoing' ? `Кому: ${msg.to}` : 
                     `Отправлено: ${msg.to}`}
                  </span>
                  <span className="message-time">
                    {formatDateTime(msg.date, msg.time)}
                  </span>
                </div>
                
                <div className="message-content">
                  {msg.typeMessage === TYPE_MESSAGE.Audio ? (
                    <div className="audio-message">
                      <button 
                        onClick={() => playAudio(msg.messageId)}
                        className={`play-button ${currentlyPlaying === msg.messageId ? 'playing' : ''}`}
                        disabled={currentlyPlaying === msg.messageId}
                      >
                        {currentlyPlaying === msg.messageId ? '▶ Воспроизводится' : '▶ Воспроизвести'}
                      </button>
                    </div>
                  ) : (
                    <p>{msg.content}</p>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
        
        {mapData && mapData.points && (
          <MapModal 
            points={mapData.points}
            onClose={() => setMapData(null)} 
          />
        )}
      </div>
    </>
  );
};

export default MessageList;