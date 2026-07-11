import { useEffect, useRef, useState } from 'react';

const WS_STATES = {
  idle: 'idle',
  connecting: 'connecting',
  open: 'open',
  closed: 'closed',
  error: 'error',
};

export default function useWebSocket(authToken) {
  const [socket, setSocket] = useState(null);
  const [status, setStatus] = useState(WS_STATES.idle);
  const socketRef = useRef(null);

  useEffect(() => {
    if (!authToken) {
      setSocket(null);
      setStatus(WS_STATES.idle);
      return;
    }

    const wsUrlBase = import.meta.env.VITE_WS_URL || '';
    const wsUrl = `${wsUrlBase}/ws?token=${encodeURIComponent(authToken)}`;

    const ws = new WebSocket(wsUrl);
    socketRef.current = ws;

    setStatus(WS_STATES.connecting);

    ws.onopen = () => {
      console.log('WebSocket connected');
      setSocket(ws);
      setStatus(WS_STATES.open);
    };

    ws.onclose = () => {
      console.log('WebSocket disconnected');
      setSocket(null);
      setStatus(WS_STATES.closed);
    };

    ws.onerror = (error) => {
      console.error('WebSocket error:', error);
      setStatus(WS_STATES.error);
    };

    return () => {
      const current = socketRef.current;
      socketRef.current = null;

      if (
        current &&
        (current.readyState === WebSocket.OPEN ||
          current.readyState === WebSocket.CONNECTING)
      ) {
        current.close();
      }

      setSocket(null);
    };
  }, [authToken]);

  return { socket, status };
}