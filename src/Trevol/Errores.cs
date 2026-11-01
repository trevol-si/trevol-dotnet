#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Trevol.Generado;

namespace Trevol
{
    /// <summary>Convierte una respuesta que no es 2xx en su excepción tipada (docs/api/43 §2.6.5).</summary>
    internal static class Errores
    {
        private const int Recorte = 2048;

        internal static double? Numero(object? v)
        {
            double n;
            switch (v)
            {
                case null: return null;
                case bool _: return null;
                case string s when double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out n): break;
                case string _: return null;
                case IConvertible c when v is long || v is int || v is double || v is float || v is decimal: n = c.ToDouble(CultureInfo.InvariantCulture); break;
                default: return null;
            }
            return n >= 0 && !double.IsNaN(n) && !double.IsInfinity(n) ? n : (double?)null;
        }

        /// <summary>El valor de <c>Retry-After</c>: segundos o una fecha HTTP. null si no está o no se entiende.</summary>
        internal static double? LeerRetryAfter(string? valor, DateTimeOffset ahora)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            var n = Numero(valor);
            if (n != null) return n;
            if (DateTimeOffset.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var fecha))
                return Math.Max(0, Math.Ceiling((fecha - ahora).TotalSeconds));
            return null;
        }

        private static object? Plano(JToken t)
        {
            switch (t.Type)
            {
                case JTokenType.Object: return ((JObject)t).Properties().ToDictionary(p => p.Name, p => Plano(p.Value));
                case JTokenType.Array: return t.Select(Plano).ToList();
                case JTokenType.Null: return null;
                default: return ((JValue)t).Value;
            }
        }

        internal static TrevolException DeRespuesta(int estado, string texto, IReadOnlyDictionary<string, IReadOnlyList<string>> cabeceras, string? idempotencyKey, DateTimeOffset ahora)
        {
            var cab = cabeceras.ToDictionary(k => k.Key.ToLowerInvariant(), k => string.Join(", ", k.Value));
            JObject? cuerpo = null;
            try
            {
                cuerpo = JToken.Parse(texto) as JObject;
            }
            catch (Exception) // no es JSON (el HTML de un proxy)
            {
                cuerpo = null;
            }
            string? Cadena(string clave) => cuerpo != null && cuerpo.TryGetValue(clave, out var v) && v.Type == JTokenType.String ? (string?)v : null;
            var code = Cadena("code");
            var detail = Cadena("detail");
            var title = Cadena("title");
            var errores = new List<FieldError>();
            if (cuerpo != null && cuerpo["errors"] is JArray lista)
            {
                foreach (var x in lista.OfType<JObject>())
                {
                    var extra = x.Properties().Where(p => p.Name != "field" && p.Name != "pointer" && p.Name != "message").ToDictionary(p => p.Name, p => Plano(p.Value));
                    string? C(string k) => x.TryGetValue(k, out var v) && v.Type == JTokenType.String ? (string?)v : null;
                    errores.Add(new FieldError(C("field"), C("pointer"), C("message"), extra));
                }
            }
            bool? retryable = cuerpo != null && cuerpo["retryable"] is JValue rv && rv.Type == JTokenType.Boolean ? (bool?)rv.Value<bool>() : null;
            var retryAfter = LeerRetryAfter(cab.TryGetValue("retry-after", out var ra) ? ra : null, ahora);
            if (retryAfter == null && cuerpo != null && cuerpo.TryGetValue("retry_after", out var rt)) retryAfter = Numero(((JValue)rt).Value);
            var mensaje = detail ?? title ?? (cuerpo != null ? $"La API respondió {estado}" : $"La API respondió {estado} sin problem+json");
            string? nombre;
            if (code != null)
            {
                Tablas.ClasePorCode.TryGetValue(code, out nombre); // un code que el SDK no conoce (la API sumó uno) da la base, nunca rompe
            }
            else if (!Tablas.ClasePorEstado.TryGetValue(estado.ToString(CultureInfo.InvariantCulture), out nombre) && estado >= 500)
            {
                Tablas.ClasePorEstado.TryGetValue("5xx", out nombre);
            }
            var detalle = cuerpo != null ? detail : RecortarCuerpo(texto);
            var requestId = Cadena("request_id") ?? (cab.TryGetValue("request-id", out var rid) ? rid : null);
            var problema = cuerpo != null ? (IReadOnlyDictionary<string, object?>)cuerpo.Properties().ToDictionary(p => p.Name, p => Plano(p.Value)) : null;
            var type = Cadena("type");
            switch (nombre)
            {
                case "AuthenticationError": return new AuthenticationException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "PermissionDeniedError": return new PermissionDeniedException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "NotFoundError": return new NotFoundException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "InvalidRequestError": return new InvalidRequestException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "IdempotencyError": return new IdempotencyException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "ValidationError": return new ValidationException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "ConflictError": return new ConflictException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "PreconditionError": return new PreconditionException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "DianRejectedError": return new DianRejectedException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "BalanceError": return new BalanceException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "RateLimitError": return new RateLimitException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "DianUnavailableError": return new DianUnavailableException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                case "InternalServerError": return new InternalServerException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
                default: return new TrevolException(mensaje, estado, code, type, title, detalle, requestId, retryable, retryAfter, errores, idempotencyKey, problema, cabeceras);
            }
        }

        private static string? RecortarCuerpo(string texto)
        {
            var t = texto.Trim();
            return t.Length == 0 ? null : t.Substring(0, Math.Min(t.Length, Recorte));
        }
    }
}
