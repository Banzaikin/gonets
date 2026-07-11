import { useEffect, useRef, useState } from 'react';
import { formatUtcToLocal } from '../../utils/dateUtils';
import './MapModal.css';

const MapModal = ({ points, onClose }) => {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const mapRef = useRef(null);
  const mapInstance = useRef(null);
  const markerRef = useRef(null);

  const parsePoint = (point) => {
    if (!point) return null;

    if (typeof point === 'string') {
      try {
        const parsed = JSON.parse(point);
        return normalizePoint(parsed);
      } catch {
        return null;
      }
    }

    if (typeof point === 'object' && typeof point.content === 'string') {
      try {
        const parsed = JSON.parse(point.content);
        return normalizePoint(parsed);
      } catch {
        return null;
      }
    }

    if (typeof point === 'object') {
      return normalizePoint(point);
    }

    return null;
  };

  const normalizePoint = (point) => {
    if (!point) return null;

    const lat =
      point?.coordinates?.lat ??
      point?.latitude ??
      null;

    const lon =
      point?.coordinates?.lon ??
      point?.longitude ??
      null;

    if (typeof lat !== 'number' || typeof lon !== 'number') {
      return null;
    }

    return {
      nodeId: point.nodeId ?? null,
      timestamp: point.timestamp ?? point.utcDateTime ?? null,
      alarm: typeof point.alarm === 'boolean' ? point.alarm : false,
      coordinates: {
        lat,
        lon
      },
      speedKmh: typeof point.speedKmh === 'number' ? point.speedKmh : null,
      course: typeof point.course === 'number' ? point.course : null,
      altitude: typeof point.altitude === 'number' ? point.altitude : null
    };
  };

  const selected = parsePoint(points?.[selectedIndex]);

  useEffect(() => {
    const initMap = () => {
      if (!selected || !selected.coordinates) return;

      const center = [selected.coordinates.lat, selected.coordinates.lon];

      if (mapInstance.current) {
        mapInstance.current.destroy();
      }

      mapInstance.current = new window.ymaps.Map(mapRef.current, {
        center,
        zoom: 13,
        controls: []
      });

      const balloonLines = [
        `Точка: ${selected.coordinates.lat.toFixed(6)}, ${selected.coordinates.lon.toFixed(6)}`
      ];

      if (selected.timestamp) {
        balloonLines.push(`Время: ${formatUtcToLocal(selected.timestamp)}`);
      }

      balloonLines.push(`Сигнал тревоги: ${selected.alarm ? 'Да' : 'Нет'}`);

      if (selected.nodeId) {
        balloonLines.push(`Узел: ${selected.nodeId}`);
      }

      if (selected.speedKmh !== null) {
        balloonLines.push(`Скорость: ${selected.speedKmh.toFixed(2)} км/ч`);
      }

      if (selected.course !== null) {
        balloonLines.push(`Курс: ${selected.course.toFixed(2)}°`);
      }

      if (selected.altitude !== null) {
        balloonLines.push(`Высота: ${selected.altitude.toFixed(1)} м`);
      }

      markerRef.current = new window.ymaps.Placemark(
        center,
        {
          hintContent: selected.timestamp
            ? `Время: ${formatUtcToLocal(selected.timestamp)}`
            : `Точка: ${selected.coordinates.lat.toFixed(6)}, ${selected.coordinates.lon.toFixed(6)}`,
          balloonContent: balloonLines.join('<br/>')
        },
        {
          preset: selected.alarm ? 'islands#redDotIcon' : 'islands#blueDotIcon'
        }
      );

      mapInstance.current.geoObjects.add(markerRef.current);
    };

    if (window.ymaps) {
      window.ymaps.ready(initMap);
    } else {
      const existingScript = document.querySelector('script[src*="api-maps.yandex.ru"]');

      if (!existingScript) {
        const script = document.createElement('script');
        script.src = 'https://api-maps.yandex.ru/2.1/?apikey=test-key&lang=ru_RU';
        script.onload = () => window.ymaps.ready(initMap);
        document.head.appendChild(script);
      } else {
        existingScript.addEventListener('load', () => window.ymaps.ready(initMap));
      }
    }

    return () => {
      if (mapInstance.current) {
        mapInstance.current.destroy();
        mapInstance.current = null;
      }
    };
  }, [selected]);

  if (!points || points.length === 0) {
    return (
      <div className="modal-overlay" onClick={onClose}>
        <div className="modal-content" onClick={(e) => e.stopPropagation()}>
          <button className="close-button" onClick={onClose}>✖</button>
          <p>Нет координат для отображения</p>
        </div>
      </div>
    );
  }

  const selectedPoint = parsePoint(points?.[selectedIndex]);

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="close-button" onClick={onClose}>✖</button>

        <div className="coordinates-display">
          <label htmlFor="coord-selector">Точка:</label>
          <select
            id="coord-selector"
            value={selectedIndex}
            onChange={(e) => setSelectedIndex(Number(e.target.value))}
          >
            {points.map((point, index) => {
              const parsed = parsePoint(point);

              return (
                <option key={index} value={index}>
                  {parsed?.timestamp 
                    ? formatUtcToLocal(parsed.timestamp)
                    : `Точка ${index + 1}`} 
                  {parsed?.alarm ? ' ⚠️' : ''}
                </option>
              );
            })}
          </select>
        </div>

        {!selectedPoint ? (
          <p>Не удалось распознать координаты для выбранной точки</p>
        ) : (
          <div ref={mapRef} style={{ width: '100%', height: '400px' }} />
        )}
      </div>
    </div>
  );
};

export default MapModal;
