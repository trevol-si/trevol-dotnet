#nullable enable
using Newtonsoft.Json;

namespace Trevol
{
    /// <summary>
    /// Ayudantes de JSON con la misma configuración de Newtonsoft.Json que usa el código generado. Sirven para armar un cuerpo de petición
    /// desde JSON (<c>TrevolJson.Deserialize&lt;InvoiceInput&gt;(texto)</c>) o para imprimir una respuesta. Los campos se llaman igual que en la API
    /// (<c>external_id</c>); los importes y las fechas viajan como texto.
    /// </summary>
    public static class TrevolJson
    {
        private static readonly JsonSerializerSettings Ajustes = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateParseHandling = DateParseHandling.None
        };

        public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Ajustes)!;

        public static string Serialize(object modelo) => JsonConvert.SerializeObject(modelo, Ajustes);
    }
}
