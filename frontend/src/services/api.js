import { getStoredToken } from './authStorage';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

function buildAuthHeaders(extraHeaders = {}) {
  const token = getStoredToken();

  if (!token) {
    return { ...extraHeaders };
  }

  return {
    ...extraHeaders,
    Authorization: `Bearer ${token}`,
  };
}

async function parseJsonSafe(response) {
  try {
    return await response.json();
  } catch {
    return null;
  }
}

async function throwHttpError(response, fallbackMessage) {
  const errorData = await parseJsonSafe(response);

  const message =
    errorData?.message ||
    errorData?.error ||
    errorData?.title ||
    `${fallbackMessage}. HTTP ${response.status}`;

  const error = new Error(message);
  error.status = response.status;
  error.payload = errorData;
  throw error;
}

const api = {
  async getMessages(type) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/${type}-messages`, {
        headers: buildAuthHeaders(),
      });

      if (!response.ok) {
        await throwHttpError(response, 'Не удалось получить сообщения');
      }

      return await response.json();
    } catch (error) {
      console.error('Ошибка при получении сообщений:', error);
      throw error;
    }
  },

  async sendMessage(formData) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/send-message`, {
        method: 'POST',
        headers: buildAuthHeaders(),
        body: formData,
      });

      if (!response.ok) {
        await throwHttpError(response, 'Не удалось отправить сообщение');
      }

      return await response.json();
    } catch (error) {
      console.error('Ошибка при отправке сообщения:', error);
      throw error;
    }
  },

  async getAudio(messageId) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/audio/${messageId}`, {
        headers: buildAuthHeaders(),
      });

      if (!response.ok) {
        await throwHttpError(response, 'Не удалось получить аудио');
      }

      return await response.blob();
    } catch (error) {
      console.error('Ошибка при получении аудио:', error);
      throw error;
    }
  },

  async uploadAudio(file) {
    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch(`${API_BASE_URL}/api/audio/upload`, {
        method: 'POST',
        headers: buildAuthHeaders(),
        body: formData,
      });

      if (!response.ok) {
        await throwHttpError(response, 'Не удалось загрузить аудио');
      }

      return await response.json();
    } catch (error) {
      console.error('Ошибка при загрузке аудио:', error);
      throw error;
    }
  }
};

export const audioUtils = {
  playAudio(blob) {
    return new Promise((resolve, reject) => {
      const audioUrl = URL.createObjectURL(blob);
      const audio = new Audio(audioUrl);

      audio.onended = () => {
        URL.revokeObjectURL(audioUrl);
        resolve();
      };

      audio.onerror = (error) => {
        URL.revokeObjectURL(audioUrl);
        reject(error);
      };

      audio.play().catch(reject);
    });
  },

  async recordAudio(duration = 10000) {
    return new Promise(async (resolve, reject) => {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        const mediaRecorder = new MediaRecorder(stream);
        const audioChunks = [];

        mediaRecorder.ondataavailable = (event) => {
          audioChunks.push(event.data);
        };

        mediaRecorder.onstop = () => {
          const audioBlob = new Blob(audioChunks, { type: 'audio/wav' });
          stream.getTracks().forEach((track) => track.stop());
          resolve(audioBlob);
        };

        mediaRecorder.start();

        setTimeout(() => {
          if (mediaRecorder.state !== 'inactive') {
            mediaRecorder.stop();
          }
        }, duration);
      } catch (error) {
        reject(error);
      }
    });
  },

  blobToBase64(blob) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onloadend = () => {
        const result = reader.result;
        if (typeof result === 'string') {
          resolve(result.split(',')[1]);
        } else {
          reject(new Error('Failed to convert blob to base64'));
        }
      };

      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }
};

export default api;