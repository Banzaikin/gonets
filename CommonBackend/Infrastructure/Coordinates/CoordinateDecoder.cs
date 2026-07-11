using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using CommonBackend.Application.Interfaces;

namespace CommonBackend.Infrastructure.Coordinates
{
    public class CoordinateDecoder : ICoordinateService
    {
        public string Decode(byte[] data)
        {
            if (data.Length <= 16)
                throw new ArgumentException("Данные слишком короткие");

            var payload = data.AsSpan(16); // Отбрасываем 16 байт заголовка

            var results = new List<object>();

            for (int i = 0; i + 20 <= payload.Length; i += 20)
            {
                var block = payload.Slice(i, 20);
                var binary = new StringBuilder();
                foreach (var b in block)
                    binary.Append(Convert.ToString(b, 2).PadLeft(8, '0'));

                var bitString = binary.ToString();

                // Извлечение времени
                int year = 2000 + Convert.ToInt32(bitString.Substring(0, 6), 2);
                int month = Convert.ToInt32(bitString.Substring(6, 4), 2);
                int day = Convert.ToInt32(bitString.Substring(10, 5), 2);
                int hour = Convert.ToInt32(bitString.Substring(15, 5), 2);
                int minute = Convert.ToInt32(bitString.Substring(20, 6), 2);
                int second = Convert.ToInt32(bitString.Substring(26, 6), 2);

                var dateTime = new DateTime(year, month, day, hour, minute, second);

                // Извлечение полушарий и тревожной кнопки
                int latHemisphereBit = Convert.ToInt32(bitString.Substring(35, 1), 2);
                int lonHemisphereBit = Convert.ToInt32(bitString.Substring(67, 1), 2);
                bool alarmButtonPressed = bitString[107] == '1';

                // Извлечение координат
                int latDegrees = Convert.ToInt32(bitString.Substring(36, 8), 2);
                int latMinutes = Convert.ToInt32(bitString.Substring(44, 6), 2);
                int latFraction = Convert.ToInt32(bitString.Substring(50, 14), 2);

                int lonDegrees = Convert.ToInt32(bitString.Substring(68, 8), 2);
                int lonMinutes = Convert.ToInt32(bitString.Substring(76, 6), 2);
                int lonFraction = Convert.ToInt32(bitString.Substring(82, 14), 2);

                double latitude = latDegrees + (latMinutes / 60.0) + (latFraction / (60.0 * 10000));
                double longitude = lonDegrees + (lonMinutes / 60.0) + (lonFraction / (60.0 * 10000));

                // Применение знаков в зависимости от полушария
                if (latHemisphereBit == 1)
                    latitude *= -1;
                if (lonHemisphereBit == 1)
                    longitude *= -1;

                results.Add(new
                {
                    timestamp = dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    alarm = alarmButtonPressed,
                    hemisphere = new
                    {
                        lat = latHemisphereBit == 0 ? "N" : "S",
                        lon = lonHemisphereBit == 0 ? "E" : "W"
                    },
                    coordinates = new
                    {
                        lat = latitude,
                        lon = longitude
                    }
                });
            }

            return JsonSerializer.Serialize(results);
        }
    }
}
