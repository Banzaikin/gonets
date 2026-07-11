import { useEffect, useState } from 'react';
import './ChannelStatusBar.css';

const CHANNEL_NAMES = {
  Internet: 'Интернет',
  Radio: 'Радио',
  Satellite: 'Спутник'
};

const ChannelStatusBar = ({ socket }) => {
  const [channels, setChannels] = useState(() => {
    const saved = sessionStorage.getItem('channels');
    return saved ? JSON.parse(saved) : {};
  });

  useEffect(() => {
    if (!socket) return;

    const handler = (event) => {
      try {
        const msg = JSON.parse(event.data);

        if (msg.type === 'channel_status') {
          setChannels(prev => {
            const newState = {
              ...prev,
              [msg.channel]: msg
            };
            sessionStorage.setItem('channels', JSON.stringify(newState));
            return newState;
          });
        }
      } catch (error) {
        console.error('Ошибка парсинга сообщения:', error);
      }
    };

    socket.addEventListener('message', handler);
    return () => socket.removeEventListener('message', handler);
  }, [socket]);

  const renderSignalBars = (level) => {
    const barsCount = 5;
    const bars = [];
    const activeBars = Math.round(level * barsCount);

    for (let i = 1; i <= barsCount; i++) {
      bars.push(
        <span
          key={i}
          className={`bar ${i <= activeBars ? 'active' : ''}`}
        />
      );
    }

    return <div className="signal-bars">{bars}</div>;
  };

  const getColor = (channel) => {
    if (!channel) return 'gray';
    if (channel.isAvailable === false) return 'red';
    if (channel.isAvailable === true) {
      const level = channel?.quality?.level ?? 1;
      const packetLoss = channel?.quality?.packetLoss ?? 0;
      if (level < 0.3 || packetLoss > 30) return 'yellow';
      return 'green';
    }
    return 'gray';
  };

  const renderStatus = (name) => {
    const channel = channels[name];
    const isAvailable = channel?.isAvailable ?? null;
    const color = getColor(channel);

    return (
      <div className="channel" key={name}>
        <span className={`dot ${color}`} />
        <span className="channel-name">{CHANNEL_NAMES[name]}</span>

        {channel?.quality && (
          <div className="channel-quality">
            <div>{channel.quality.level ? renderSignalBars(channel.quality.level) : 'N/A'}</div>
            <div>Задержка: {channel.quality.latencyMs ?? 'N/A'} мс</div>
            <div>Потеря пакетов: {channel.quality.packetLoss ?? 'N/A'}</div>
            <div>Битрейт: {channel.quality.bandwidthKbps ?? 'N/A'} Кбит/с</div>
          </div>
        )}

        {name !== 'Internet' && channel?.metrics && (
          <div className="channel-metrics">
            <div>SNR: {channel.metrics.snr ?? 'N/A'}</div>
            <div>RSSI: {channel.metrics.rssi ?? 'N/A'}</div>
            <div>BER: {channel.metrics.ber ?? 'N/A'}</div>
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="channel-bar">
      {Object.keys(CHANNEL_NAMES).map(renderStatus)}
    </div>
  );
};

export default ChannelStatusBar;