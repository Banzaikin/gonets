import { useState, useRef } from 'react';
import AudioRecorder from '../AudioRecorder/AudioRecorder';
import api from '../../services/api';
import './MessageInput.css';
import { TYPE_MESSAGE } from '../../constants/messageTypes';

const MessageInput = ({ activeTab }) => {
  const [recipient, setRecipient] = useState('');
  const [textMessage, setTextMessage] = useState('');
  const [audioBlob, setAudioBlob] = useState(null);
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState(null);
  const audioPlayerRef = useRef(null);

  const handleSendMessage = async () => {
    if ((!textMessage.trim() && !audioBlob) || !recipient.trim()) return;

    setIsSending(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append('to', recipient.trim());
      
      const now = new Date();
      formData.append('date', now.toISOString().split('T')[0]);
      formData.append('time', now.toTimeString().split(' ')[0].slice(0, 5));

      if (audioBlob) {
        formData.append('typeMessage', TYPE_MESSAGE.Audio.toString());
        formData.append('file', audioBlob, 'recording.wav');
      } else {
        formData.append('typeMessage', TYPE_MESSAGE.Text.toString());
        formData.append('content', textMessage.trim());
      }

      await api.sendMessage(formData);
      setTextMessage('');
      setAudioBlob(null);
    } catch (error) {
      console.error('Ошибка отправки:', error);
      setError(error.message || 'Не удалось отправить сообщение');
    } finally {
      setIsSending(false);
    }
  };

  return (
    <div className="message-input">
      <input
        value={recipient}
        onChange={(e) => setRecipient(e.target.value)}
        placeholder="Получатель (ID)"
        className="recipient-input"
        disabled={isSending}
      />

      <AudioRecorder 
        onRecordingComplete={setAudioBlob}
        disabled={isSending}
      />

      {audioBlob ? (
        <div className="audio-preview">
          <audio 
            ref={audioPlayerRef}
            src={URL.createObjectURL(audioBlob)} 
            controls
          />
          <button 
            onClick={() => {
              setAudioBlob(null);
              if (audioPlayerRef.current) {
                audioPlayerRef.current.pause();
                audioPlayerRef.current.currentTime = 0;
              }
            }}
            disabled={isSending}
          >
            Удалить
          </button>
        </div>
      ) : (
        <textarea
          value={textMessage}
          onChange={(e) => setTextMessage(e.target.value)}
          placeholder="Текст сообщения"
          className="message-textarea"
          disabled={isSending}
        />
      )}

      {error && <div className="error-message">{error}</div>}

      <button
        onClick={handleSendMessage}
        disabled={
          (!textMessage.trim() && !audioBlob) || 
          !recipient.trim() || 
          isSending
        }
        className="send-button"
      >
        {isSending ? 'Отправка...' : 'Отправить'}
      </button>
    </div>
  );
};

export default MessageInput;