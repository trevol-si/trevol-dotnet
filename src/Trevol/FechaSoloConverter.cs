#nullable enable
using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Trevol
{
    /// <summary>
    /// Las fechas (<c>format: date</c>) de los cuerpos de petición salen como «yyyy-MM-dd». El generador con <c>useDateTimeOffset</c> tipa
    /// TODA fecha como <c>DateTimeOffset</c> y solo pone el convertidor de fechas en las propiedades sueltas, no en las LISTAS
    /// (<c>payment_dates</c>), que saldrían como «2026-10-31T00:00:00-05:00» y la API rechaza. Ningún cuerpo de petición tiene un
    /// <c>date-time</c> (una prueba lo vigila con la especificación), así que todo <c>DateTimeOffset</c> que se escribe es una fecha.
    /// Al leer acepta fechas y fechas con hora.
    /// </summary>
    internal sealed class FechaSoloConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is DateTimeOffset fecha) writer.WriteValue(fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            else writer.WriteNull();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            if (reader.Value is DateTimeOffset ya) return ya;
            if (reader.Value is DateTime dt) return new DateTimeOffset(dt);
            return DateTimeOffset.Parse(Convert.ToString(reader.Value, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }
    }
}
