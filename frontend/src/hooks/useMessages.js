import { useEffect, useState } from 'react';
import api from '../services/api';

const useMessages = (type) => {
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchMessages = async () => {
      try {
        setLoading(true);
        const data = await api.getMessages(type);
        setMessages(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchMessages();
  }, [type]);

  return { messages, loading, error };
};

export default useMessages;