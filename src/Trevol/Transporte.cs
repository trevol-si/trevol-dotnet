#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Trevol.Generado;

namespace Trevol
{
    /// <summary>Qué pasó en un intento: una respuesta (con <see cref="Estado"/>) o un fallo de red (sin respuesta). <c>RetryAfter</c> en segundos, ya resuelto.</summary>
    public sealed class ResultadoIntento
    {
        private ResultadoIntento(bool red, int? estado, string? code, bool? retryable, double? retryAfter)
        {
            Red = red; Estado = estado; Code = code; Retryable = retryable; RetryAfter = retryAfter;
        }

        public bool Red { get; }
        public int? Estado { get; }
        public string? Code { get; }
        public bool? Retryable { get; }
        public double? RetryAfter { get; }

        public static ResultadoIntento DeRed() => new ResultadoIntento(true, null, null, null, null);

        public static ResultadoIntento DeRespuesta(int estado, string? code = null, bool? retryable = null, double? retryAfter = null) =>
            new ResultadoIntento(false, estado, code, retryable, retryAfter);
    }

    /// <summary>La decisión de <see cref="Transporte.Decidir"/>. <c>Motivo</c> explica un «no»: «exito», «agotado» o «retry_after_excede».</summary>
    public sealed class Decision
    {
        internal Decision(bool reintentar, double? espera, string? motivo) { Reintentar = reintentar; Espera = espera; Motivo = motivo; }

        public bool Reintentar { get; }
        public double? Espera { get; }
        public string? Motivo { get; }
    }

    /// <summary>
    /// El transporte: lo que hace que las llamadas se comporten igual en los 7 SDK (docs/api/43 §2.6.2 a §2.6.5). Es un
    /// <see cref="DelegatingHandler"/> dentro del <see cref="HttpClient"/> (el punto de extensión del código generado): pone las cabeceras y la
    /// Idempotency-Key, reintenta y convierte toda respuesta que no es 2xx en una <see cref="TrevolException"/> que atraviesa el código generado
    /// sin envolverse en <c>ApiException</c>.
    /// </summary>
    public sealed class Transporte : DelegatingHandler
    {
        private static readonly int[] Reintentables = { 429, 500, 502, 503, 504 };
        private static readonly string[] EnCurso = { "request_in_progress", "previous_event_pending" };
        private const string Problema = "application/problem+json";
        private static readonly Regex ClaveValida = new Regex("^[!-~]{1,255}$", RegexOptions.CultureInvariant);

        /// <summary>La espera propia antes del intento siguiente: 0,5 · 2^(intento−1), con tope de 8 s, por un factor al azar en [0,75; 1].</summary>
        public static double EsperaPropia(int intento, double azar) => Math.Min(0.5 * Math.Pow(2, intento - 1), 8.0) * (0.75 + 0.25 * azar);

        /// <summary>Función PURA: ¿se reintenta y cuánto se espera? Las mismas reglas y los mismos casos (<c>pruebas/datos/transporte.json</c>) en los 7 SDK.</summary>
        public static Decision Decidir(int intento, string reintento, int maxReintentos, double maxRetryAfter, double azar, ResultadoIntento r)
        {
            if (!r.Red && r.Estado >= 200 && r.Estado < 300) return new Decision(false, null, "exito");
            bool reintentable;
            if (r.Red) reintentable = reintento == "seguro";
            else if (reintento == "solo_429") reintentable = r.Estado == 429;
            else if (r.Estado == 402) reintentable = false;
            else reintentable = Reintentables.Contains(r.Estado!.Value) || (r.Code != null && EnCurso.Contains(r.Code)) || r.Retryable == true;
            if (!reintentable) return new Decision(false, null, null);
            if (intento > maxReintentos) return new Decision(false, null, "agotado");
            var propia = EsperaPropia(intento, azar);
            var pedida = !r.Red && r.RetryAfter > 0 ? r.RetryAfter!.Value : 0.0;
            if (pedida > maxRetryAfter) return new Decision(false, null, "retry_after_excede");
            return new Decision(true, Math.Max(pedida, propia), null);
        }

        internal static void ValidarClave(string? clave)
        {
            if (clave == null || !ClaveValida.IsMatch(clave))
                throw new ConfigurationException("La idempotencyKey debe tener de 1 a 255 caracteres ASCII visibles (sin espacios).");
        }

        private static readonly Dictionary<string, Tablas.Operacion> Exactas = new Dictionary<string, Tablas.Operacion>();
        private static readonly List<KeyValuePair<Tablas.Operacion, Regex>> Plantillas = new List<KeyValuePair<Tablas.Operacion, Regex>>();

        static Transporte()
        {
            foreach (var o in Tablas.Operaciones)
            {
                if (o.Ruta.Contains("{"))
                    Plantillas.Add(new KeyValuePair<Tablas.Operacion, Regex>(o, new Regex("^" + Regex.Replace(Regex.Escape(o.Ruta).Replace("\\{", "{").Replace("\\}", "}"), "\\{[a-z_]+\\}", "[^/]+") + "$", RegexOptions.CultureInvariant)));
                else
                    Exactas[o.Metodo + " " + o.Ruta] = o;
            }
        }

        /// <summary>La operación (tablas) de un método y una ruta SIN el prefijo /v1. Las rutas fijas ganan a las plantillas.</summary>
        internal static Tablas.Operacion? Operacion(string metodo, string ruta)
        {
            if (Exactas.TryGetValue(metodo + " " + ruta, out var o)) return o;
            foreach (var p in Plantillas)
                if (p.Key.Metodo == metodo && p.Value.IsMatch(ruta)) return p.Key;
            return null;
        }

        private static string Aceptar(string? binario)
        {
            switch (binario)
            {
                case "pdf": return "application/pdf, " + Problema;
                case "xml": return "application/xml, " + Problema;
                case "zip": return "application/zip, " + Problema;
                default: return "application/json, " + Problema;
            }
        }

        private readonly string _llave;
        private readonly string _apiVersion;
        private readonly string _userAgent;
        private readonly int _maxReintentos;
        private readonly double _maxRetryAfter;
        private readonly TimeSpan _timeout;
        private readonly string _rutaBase;
        private readonly Func<double, CancellationToken, Task> _dormir;
        private readonly Func<double> _azar;
        private readonly Func<string> _uuid;
        private readonly Func<DateTimeOffset> _ahora;

        internal Transporte(HttpMessageHandler interno, string llave, string apiVersion, string userAgent, int maxReintentos, double maxRetryAfter,
            TimeSpan timeout, string rutaBase, Func<double, CancellationToken, Task> dormir, Func<double> azar, Func<string> uuid, Func<DateTimeOffset> ahora)
            : base(interno)
        {
            _llave = llave; _apiVersion = apiVersion; _userAgent = userAgent; _maxReintentos = maxReintentos; _maxRetryAfter = maxRetryAfter;
            _timeout = timeout; _rutaBase = rutaBase; _dormir = dormir; _azar = azar; _uuid = uuid; _ahora = ahora;
        }

        /// <summary>La llave nunca sale en un ToString, un log ni un volcado.</summary>
        public override string ToString() => $"Transporte(apiVersion={_apiVersion}, maxReintentos={_maxReintentos})";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var metodo = request.Method.Method.ToUpperInvariant();
            var ruta = request.RequestUri!.AbsolutePath;
            if (_rutaBase.Length > 0 && ruta.StartsWith(_rutaBase, StringComparison.Ordinal)) ruta = ruta.Substring(_rutaBase.Length);
            var op = Operacion(metodo, ruta);
            var reintento = op != null ? op.Reintento : (metodo == "GET" ? "seguro" : "solo_429");

            // La Idempotency-Key: una por llamada, la misma en todos los reintentos.
            string? clave = null;
            if (op != null && op.Idempotente)
            {
                if (request.Headers.TryGetValues("Idempotency-Key", out var dadas))
                {
                    clave = dadas.First();
                    ValidarClave(clave);
                }
                else
                {
                    clave = _uuid();
                }
            }
            // El cuerpo (StringContent) se vuelve a leer en cada intento; se copia para no depender de que se haya descartado.
            byte[]? cuerpo = request.Content == null ? null : await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var tipo = request.Content?.Headers.ContentType;

            for (var intento = 1; ; intento++)
            {
                using var peticion = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };
                foreach (var h in request.Headers.Where(h => !string.Equals(h.Key, "Idempotency-Key", StringComparison.OrdinalIgnoreCase) && !string.Equals(h.Key, "Accept", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(h.Key, "User-Agent", StringComparison.OrdinalIgnoreCase) && !string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase)))
                    peticion.Headers.TryAddWithoutValidation(h.Key, h.Value);
                if (cuerpo != null)
                {
                    peticion.Content = new ByteArrayContent(cuerpo);
                    if (tipo != null) peticion.Content.Headers.ContentType = tipo;
                }
                if (clave != null) peticion.Headers.TryAddWithoutValidation("Idempotency-Key", clave);
                peticion.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _llave);
                peticion.Headers.TryAddWithoutValidation("Trevol-Version", _apiVersion);
                peticion.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
                peticion.Headers.TryAddWithoutValidation("Accept", Aceptar(op?.Binario));

                HttpResponseMessage? respuesta = null;
                Exception? causa = null;
                using (var tiempo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    tiempo.CancelAfter(_timeout);
                    try
                    {
                        respuesta = await base.SendAsync(peticion, tiempo.Token).ConfigureAwait(false);
                    }
                    catch (HttpRequestException e)
                    {
                        causa = e;
                    }
                    catch (OperationCanceledException e) when (!cancellationToken.IsCancellationRequested)
                    {
                        causa = e; // el tiempo de ESTE intento se agotó: es un fallo de red
                    }
                    catch (System.IO.IOException e)
                    {
                        causa = e;
                    }
                }

                Decision decision;
                if (respuesta != null)
                {
                    if (respuesta.IsSuccessStatusCode) return respuesta;
                    var estado = (int)respuesta.StatusCode;
                    var texto = await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var cabeceras = new Dictionary<string, IReadOnlyList<string>>();
                    foreach (var h in respuesta.Headers) cabeceras[h.Key] = h.Value.ToList();
                    foreach (var h in respuesta.Content.Headers) cabeceras[h.Key] = h.Value.ToList();
                    respuesta.Dispose();
                    var error = Errores.DeRespuesta(estado, texto, cabeceras, clave, _ahora());
                    decision = Decidir(intento, reintento, _maxReintentos, _maxRetryAfter, _azar(), ResultadoIntento.DeRespuesta(estado, error.Code, error.Retryable, error.RetryAfter));
                    if (!decision.Reintentar) throw error;
                }
                else
                {
                    decision = Decidir(intento, reintento, _maxReintentos, _maxRetryAfter, _azar(), ResultadoIntento.DeRed());
                    if (!decision.Reintentar)
                        throw new ApiConnectionException($"No pude conectar con la API de Trevol ({Excepcion.Describir(causa!)}).", clave, causa);
                }
                await _dormir(decision.Espera ?? 0.0, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
