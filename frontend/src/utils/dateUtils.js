export const formatUtcToLocal = (timestamp, withSeconds = true) => {
  if (!timestamp) return null;

  try {
    const safeTimestamp = timestamp.includes(' ')
      ? timestamp.replace(' ', 'T') + 'Z'
      : timestamp;

    const date = new Date(safeTimestamp);

    if (isNaN(date)) return timestamp;

    return date.toLocaleString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: withSeconds ? '2-digit' : undefined
    });
  } catch {
    return timestamp;
  }
};


export const formatLegacyDateTime = (dateStr, timeStr) => {
  if (!dateStr || !timeStr) return `${dateStr ?? ''} ${timeStr ?? ''}`;

  try {
    const iso = `${dateStr.slice(0, 4)}-${dateStr.slice(4, 6)}-${dateStr.slice(6, 8)}T${timeStr.slice(0, 2)}:${timeStr.slice(2, 4)}:${timeStr.slice(4, 6)}Z`;

    const date = new Date(iso);

    if (isNaN(date)) return `${dateStr} ${timeStr}`;

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
};