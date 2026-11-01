#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trevol.Generado.Model;

namespace Trevol
{
    /// <summary>
    /// Verificación de webhooks (docs/api/43 §2.6.9): Standard Webhooks sobre el CUERPO CRUDO. Es una implementación propia (D8): la única
    /// biblioteca de Standard Webhooks para .NET en NuGet es de un tercero, sin licencia SPDX clara, sin <c>netstandard2.0</c>, ligada a ASP.NET Core
    /// y sin cambios desde 2025; el algoritmo son ≈ 40 líneas (HMAC-SHA256 sobre «id.timestamp.cuerpo», clave = base64 tras <c>whsec_</c>,
    /// tolerancia de 5 minutos, comparación en tiempo constante) y pasa los mismos vectores que los demás SDK.
    /// </summary>
    public sealed class Webhook
    {
        /// <summary>Tolerancia de <c>webhook-timestamp</c>, en segundos, hacia atrás y hacia adelante.</summary>
        public const long Tolerancia = 300;

        private static readonly Regex Digitos = new Regex("^[0-9]{1,12}$", RegexOptions.CultureInvariant);
        private readonly byte[] _clave;
        private readonly Func<long> _ahora;

        /// <param name="secret">El secreto del endpoint (<c>whsec_…</c>), que se muestra una sola vez al crearlo o rotarlo.</param>
        public Webhook(string secret) : this(secret, () => DateTimeOffset.UtcNow.ToUnixTimeSeconds()) { }

        internal Webhook(string secret, Func<long> ahora)
        {
            var s = (secret ?? "").Trim();
            if (s.Length == 0 || s == "whsec_")
                throw new ConfigurationException("Falta el secreto del webhook (whsec_…). Se muestra una sola vez al crear o rotar el endpoint.");
            if (s.StartsWith("whsec_", StringComparison.Ordinal)) s = s.Substring(6);
            try
            {
                _clave = Convert.FromBase64String(s);
            }
            catch (FormatException) // nunca se cuenta el secreto
            {
                throw new ConfigurationException("El secreto del webhook no tiene el formato esperado (whsec_ + base64).");
            }
            if (_clave.Length == 0) throw new ConfigurationException("El secreto del webhook no tiene el formato esperado (whsec_ + base64).");
            _ahora = ahora;
        }

        private static string? Primero(object? v)
        {
            if (v == null) return null;
            if (v is string s) return s;
            if (v is IEnumerable<string> e) return e.FirstOrDefault();
            return v.ToString();
        }

        /// <summary>Las tres cabeceras en minúsculas, venga lo que venga (<c>HttpRequestHeaders</c>, <c>IHeaderDictionary</c> de ASP.NET, un diccionario). La primera repetida gana.</summary>
        internal static Dictionary<string, string> Normalizar<TValor>(IEnumerable<KeyValuePair<string, TValor>> cabeceras)
        {
            var salida = new Dictionary<string, string>();
            foreach (var par in cabeceras)
            {
                var nombre = par.Key.ToLowerInvariant();
                if ((nombre == "webhook-id" || nombre == "webhook-timestamp" || nombre == "webhook-signature") && !salida.ContainsKey(nombre))
                {
                    var v = Primero(par.Value);
                    if (v != null) salida[nombre] = v;
                }
            }
            return salida;
        }

        /// <summary>Verifica el cuerpo crudo en bytes (lo normal: leer el <c>Request.Body</c> sin deserializar).</summary>
        public Event Verify<TValor>(byte[] body, IEnumerable<KeyValuePair<string, TValor>> headers)
        {
            string texto;
            try
            {
                texto = new UTF8Encoding(false, true).GetString(body);
            }
            catch (ArgumentException)
            {
                throw new WebhookVerificationException("El cuerpo del webhook no es UTF-8.", "cuerpo");
            }
            return Verify(texto, headers);
        }

        /// <summary>
        /// Verifica la firma y la fecha de una entrega y devuelve el evento. Lanza <see cref="WebhookVerificationException"/> si algo no cuadra.
        /// Ojo: la verificación NO evita repeticiones dentro de los 5 minutos ni los reintentos de entrega: deduplica por <c>webhook-id</c>.
        /// </summary>
        /// <param name="body">El CUERPO CRUDO tal como llegó (la firma se calcula sobre los bytes exactos).</param>
        /// <param name="headers">Las cabeceras: <c>HttpRequestHeaders</c>, <c>IHeaderDictionary</c> o un diccionario, en cualquier caja.</param>
        public Event Verify<TValor>(string body, IEnumerable<KeyValuePair<string, TValor>> headers)
        {
            if (body == null)
                throw new WebhookVerificationException("Falta el cuerpo del webhook: debe ser el CUERPO CRUDO, no un objeto ya deserializado " +
                    "(en ASP.NET Core: leer Request.Body con un StreamReader antes de cualquier [FromBody]).", "cuerpo");
            var h = Normalizar(headers);
            foreach (var n in new[] { "webhook-id", "webhook-timestamp", "webhook-signature" })
                if (!h.TryGetValue(n, out var v) || v.Length == 0) throw new WebhookVerificationException($"Falta la cabecera {n}.", "cabeceras");
            if (!Digitos.IsMatch(h["webhook-timestamp"])) throw new WebhookVerificationException("El webhook-timestamp no es un número de segundos.", "fecha");
            var marca = long.Parse(h["webhook-timestamp"], System.Globalization.CultureInfo.InvariantCulture);
            if (Math.Abs(_ahora() - marca) > Tolerancia)
                throw new WebhookVerificationException("La fecha del webhook está fuera de la tolerancia de 5 minutos.", "fecha");

            byte[] esperada;
            using (var hmac = new HMACSHA256(_clave))
                esperada = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{h["webhook-id"]}.{marca}.{body}"));
            var esperadaB64 = Encoding.ASCII.GetBytes(Convert.ToBase64String(esperada));
            var coincide = false;
            foreach (var firma in h["webhook-signature"].Split(' '))
            {
                var partes = firma.Split(new[] { ',' }, 2);
                if (partes.Length == 2 && partes[0] == "v1" && IgualesEnTiempoConstante(esperadaB64, Encoding.ASCII.GetBytes(partes[1])))
                    coincide = true; // basta una válida (rotación del secreto); se recorren todas
            }
            if (!coincide)
                throw new WebhookVerificationException("La firma del webhook no coincide: revisa el secreto y que el cuerpo sea el crudo, sin reescribir.", "firma");
            try
            {
                return TrevolJson.Deserialize<Event>(body);
            }
            catch (Exception e) when (e is JsonException || e is ArgumentException || e is InvalidOperationException)
            {
                throw new WebhookVerificationException("El cuerpo firmado no es un evento válido.", "cuerpo");
            }
        }

        private static bool IgualesEnTiempoConstante(byte[] a, byte[] b)
        {
            var diff = a.Length ^ b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        /// <summary>
        /// El <c>data</c> del evento como el modelo de su <c>type</c> (p. ej. <c>DocumentEventData</c> para <c>invoice.accepted</c>). null si el tipo
        /// es nuevo y este SDK todavía no lo conoce: el evento no se pierde, <c>evento.Data</c> sigue ahí como diccionario.
        /// </summary>
        public object? EventData(Event evento)
        {
            if (evento.Type == null || evento.Data == null || !Generado.Tablas.EventoModelos.TryGetValue(evento.Type, out var modelo)) return null;
            return JObject.FromObject(evento.Data).ToObject(modelo);
        }
    }
}
