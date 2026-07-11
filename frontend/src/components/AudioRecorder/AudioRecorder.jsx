import { useState, useRef, useEffect } from 'react';
import './AudioRecorder.css';

const AudioRecorder = ({ onRecordingComplete, disabled }) => {
  const [isRecording, setIsRecording] = useState(false);
  const [recordingTime, setRecordingTime] = useState(0);
  const mediaRecorderRef = useRef(null);
  const audioChunksRef = useRef([]);
  const timerRef = useRef(null);
  const audioContextRef = useRef(null);

  useEffect(() => {
    return () => {
      if (mediaRecorderRef.current?.stream) {
        mediaRecorderRef.current.stream.getTracks().forEach(track => track.stop());
      }
      if (timerRef.current) clearInterval(timerRef.current);
      if (audioContextRef.current?.state !== 'closed') {
        audioContextRef.current?.close();
      }
    };
  }, []);

  const startRecording = async () => {
    if (disabled || isRecording) return;
    
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ 
        audio: { 
          sampleRate: 16000,
          channelCount: 1,
          echoCancellation: true,
          noiseSuppression: true
        } 
      });
      
      audioContextRef.current = new (window.AudioContext || window.webkitAudioContext)({
        sampleRate: 16000
      });
      
      const source = audioContextRef.current.createMediaStreamSource(stream);
      const processor = audioContextRef.current.createScriptProcessor(4096, 1, 1);
      
      audioChunksRef.current = [];
      
      processor.onaudioprocess = (e) => {
        const input = e.inputBuffer.getChannelData(0);
        const pcmData = new Int16Array(input.length);
        for (let i = 0; i < input.length; i++) {
          pcmData[i] = Math.max(-32768, Math.min(32767, input[i] * 32768));
        }
        audioChunksRef.current.push(pcmData);
      };
      
      source.connect(processor);
      processor.connect(audioContextRef.current.destination);
      
      mediaRecorderRef.current = {
        stream,
        processor,
        stop: () => {
          stream.getTracks().forEach(track => track.stop());
          processor.disconnect();
          
          const totalLength = audioChunksRef.current.reduce((acc, chunk) => acc + chunk.length, 0);
          const merged = new Int16Array(totalLength);
          let offset = 0;
          
          audioChunksRef.current.forEach(chunk => {
            merged.set(chunk, offset);
            offset += chunk.length;
          });
          
          const wavBlob = encodeWAV(merged, 16000);
          onRecordingComplete(wavBlob);
        }
      };
      
      setRecordingTime(0);
      setIsRecording(true);
      timerRef.current = setInterval(() => {
        setRecordingTime(prev => prev + 1);
      }, 1000);
      
    } catch (error) {
      console.error('Ошибка записи:', error);
      alert('Не удалось начать запись. Проверьте разрешения микрофона.');
    }
  };

  const stopRecording = () => {
    if (!isRecording || !mediaRecorderRef.current) return;
    
    mediaRecorderRef.current.stop();
    clearInterval(timerRef.current);
    setIsRecording(false);
    setRecordingTime(0);
  };

  const encodeWAV = (samples, sampleRate) => {
    const buffer = new ArrayBuffer(44 + samples.length * 2);
    const view = new DataView(buffer);

    const writeString = (view, offset, string) => {
      for (let i = 0; i < string.length; i++) {
        view.setUint8(offset + i, string.charCodeAt(i));
      }
    };

    writeString(view, 0, 'RIFF');
    view.setUint32(4, 36 + samples.length * 2, true);
    writeString(view, 8, 'WAVE');
    writeString(view, 12, 'fmt ');
    view.setUint32(16, 16, true);
    view.setUint16(20, 1, true);
    view.setUint16(22, 1, true);
    view.setUint32(24, sampleRate, true);
    view.setUint32(28, sampleRate * 2, true);
    view.setUint16(32, 2, true);
    view.setUint16(34, 16, true);
    writeString(view, 36, 'data');
    view.setUint32(40, samples.length * 2, true);

    for (let i = 0; i < samples.length; i++) {
      view.setInt16(44 + (i * 2), samples[i], true);
    }

    return new Blob([view], { type: 'audio/wav' });
  };

  return (
    <div className="audio-recorder">
      <button
        onClick={isRecording ? stopRecording : startRecording}
        className={`record-button ${isRecording ? 'recording' : ''}`}
        disabled={disabled}
      >
        {isRecording ? `⏹ Остановить (${recordingTime} сек)` : '🎤 Запись'}
      </button>
    </div>
  );
};

export default AudioRecorder;