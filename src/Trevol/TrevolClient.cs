#nullable enable
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trevol.Generado;
using Trevol.Generado.Api;
using Trevol.Generado.Client;

namespace Trevol
{
    /// <summary>
    /// Las opciones de <see cref="TrevolClient"/>. Todas son opcionales.
    /// </summary>
    public sealed class TrevolClientOptions
    {
        /// <summary>El valor de <c>Trevol-Version</c> en cada petición (por defecto <see cref="TrevolClient.ApiVersionDefecto"/>).</summary>
        public string? ApiVersion { get; set; }
        /// <summary>Reintentos tras el primer intento (3 intentos en total por defecto). 0 los apaga.</summary>
        public int MaxRetries { get; set; } = 2;
        /// <summary>Tiempo máximo por intento (65 s por defecto, como el playground).</summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(65);
        /// <summary>Si <c>Retry-After</c> pide esperar más que esto (60 s por defecto), no se espera: se lanza el error con <c>RetryAfter</c>.</summary>
        public TimeSpan MaxRetryAfter { get; set; } = TimeSpan.FromSeconds(60);
        /// <summary>Por defecto https://api.trevol.si/v1 (para un servidor simulado en las pruebas).</summary>
        public string BaseUrl { get; set; } = TrevolClient.UrlBase;
        /// <summary>Si se pasa y la llave no coincide, <see cref="ConfigurationException"/> ANTES de llamar.</summary>
        public TrevolEnvironment? Environment { get; set; }
        /// <summary>Tu propio manejador (proxy, TLS, métricas, pruebas): el SDK lo envuelve con su <see cref="Transporte"/>.</summary>
        public HttpMessageHandler? Handler { get; set; }

        // Solo para las pruebas del paquete: el reloj, el azar y los UUID.
        internal Func<double, CancellationToken, Task>? Dormir { get; set; }
        internal Func<double>? Azar { get; set; }
        internal Func<string>? Uuid { get; set; }
        internal Func<DateTimeOffset>? Ahora { get; set; }
    }

    /// <summary>
    /// El cliente de la API de Trevol: una propiedad por recurso (<c>Invoices</c>, <c>CreditNotes</c>, …), cada una con los métodos generados de la
    /// especificación (<c>CreateInvoiceAsync</c>, <c>ListInvoicesAsync</c>, <c>RetrieveInvoiceAsync</c>…). Documentación: https://platform.trevol.si/docs
    /// (La clase se llama <c>TrevolClient</c> y no <c>Trevol</c> porque el espacio de nombres ya es <c>Trevol</c>.)
    /// </summary>
    public sealed partial class TrevolClient : IDisposable
    {
        public const string Version = Tablas.Version;
        public const string ApiVersionDefecto = Tablas.ApiVersion;
        public const string UrlBase = "https://api.trevol.si/v1";

        private readonly HttpClient _http;

        /// <summary><c>Test</c> (llave tvl_test_, habilitación de la DIAN) o <c>Live</c> (tvl_live_, producción).</summary>
        public TrevolEnvironment Environment { get; }
        /// <summary>El valor de <c>Trevol-Version</c> que se manda en cada petición.</summary>
        public string ApiVersion { get; }

        public BalanceApi Balance { get; }
        public CreditNotesApi CreditNotes { get; }
        public DebitNotesApi DebitNotes { get; }
        public EquivalentDocumentAdjustmentsApi EquivalentDocumentAdjustments { get; }
        public EquivalentDocumentsApi EquivalentDocuments { get; }
        public EventsApi Events { get; }
        public GeneralApi General { get; }
        public InvoicesApi Invoices { get; }
        public PayrollAdjustmentsApi PayrollAdjustments { get; }
        public PayrollsApi Payrolls { get; }
        public RadianEventsApi RadianEvents { get; }
        public RequestsApi Requests { get; }
        public SupportDocumentAdjustmentsApi SupportDocumentAdjustments { get; }
        public SupportDocumentsApi SupportDocuments { get; }
        public WebhookEndpointsApi WebhookEndpoints { get; }

        /// <param name="apiKey"><c>tvl_test_…</c> o <c>tvl_live_…</c>; si falta se lee <c>TREVOL_API_KEY</c>.</param>
        /// <param name="options">Las opciones; ninguna es obligatoria.</param>
        public TrevolClient(string? apiKey = null, TrevolClientOptions? options = null)
        {
            options ??= new TrevolClientOptions();
            var llave = apiKey ?? System.Environment.GetEnvironmentVariable("TREVOL_API_KEY");
            if (string.IsNullOrWhiteSpace(llave))
                throw new ConfigurationException("Falta la llave de la API: pásala a new TrevolClient(...) o define la variable de entorno TREVOL_API_KEY.");
            Environment = Llave.Entorno(llave);
            if (options.Environment != null && options.Environment != Environment)
                throw new ConfigurationException($"Pediste el entorno «{options.Environment.ToString()!.ToLowerInvariant()}» pero la llave es de «{Environment.ToString().ToLowerInvariant()}» " +
                    $"({(Environment == TrevolEnvironment.Test ? "tvl_test_ = habilitación" : "tvl_live_ = producción")}).");
            if (options.MaxRetries < 0) throw new ConfigurationException("MaxRetries no puede ser negativo.");
            ApiVersion = options.ApiVersion ?? ApiVersionDefecto;
            var baseUrl = options.BaseUrl.TrimEnd('/');
            var ruta = new Uri(baseUrl).AbsolutePath.TrimEnd('/');

            var transporte = new Transporte(
                options.Handler ?? new HttpClientHandler(), llave!.Trim(), ApiVersion,
                $"trevol-dotnet/{Version} ({System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Trim()}; +https://platform.trevol.si/docs)",
                options.MaxRetries, options.MaxRetryAfter.TotalSeconds, options.Timeout, ruta,
                options.Dormir ?? ((s, ct) => Task.Delay(TimeSpan.FromSeconds(s), ct)),
                options.Azar ?? (() => new Random().NextDouble()), options.Uuid ?? (() => Guid.NewGuid().ToString()), options.Ahora ?? (() => DateTimeOffset.UtcNow));
            // El tiempo por intento lo pone el transporte: el HttpClient no limita el conjunto de intentos.
            _http = new HttpClient(transporte, true) { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
            var config = new Configuration { BasePath = baseUrl };

            Balance = new BalanceApi(_http, config);
            CreditNotes = new CreditNotesApi(_http, config);
            DebitNotes = new DebitNotesApi(_http, config);
            EquivalentDocumentAdjustments = new EquivalentDocumentAdjustmentsApi(_http, config);
            EquivalentDocuments = new EquivalentDocumentsApi(_http, config);
            Events = new EventsApi(_http, config);
            General = new GeneralApi(_http, config);
            Invoices = new InvoicesApi(_http, config);
            PayrollAdjustments = new PayrollAdjustmentsApi(_http, config);
            Payrolls = new PayrollsApi(_http, config);
            RadianEvents = new RadianEventsApi(_http, config);
            Requests = new RequestsApi(_http, config);
            SupportDocumentAdjustments = new SupportDocumentAdjustmentsApi(_http, config);
            SupportDocuments = new SupportDocumentsApi(_http, config);
            WebhookEndpoints = new WebhookEndpointsApi(_http, config);

            // Piloto: el generador marca como «emitir aunque sea nulo» los campos opcionales sin tipo (p. ej. `rounding`), y un `"rounding": null` que la
            // persona no escribió lo rechaza la API (422). Una propiedad sin valor se omite, como en los demás SDK.
            foreach (var cliente in new[]
            {
                Balance.ApiClient, CreditNotes.ApiClient, DebitNotes.ApiClient, EquivalentDocumentAdjustments.ApiClient, EquivalentDocuments.ApiClient,
                Events.ApiClient, General.ApiClient, Invoices.ApiClient, PayrollAdjustments.ApiClient, Payrolls.ApiClient, RadianEvents.ApiClient,
                Requests.ApiClient, SupportDocumentAdjustments.ApiClient, SupportDocuments.ApiClient, WebhookEndpoints.ApiClient
            })
            {
                cliente.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                // Piloto: el generador pone el convertidor de fechas (`yyyy-MM-dd`) en las propiedades sueltas pero no en las LISTAS de fechas
                // (`payment_dates`), que saldrían como «2026-10-31T00:00:00» y la API rechaza. Este convertidor solo actúa donde no hay otro.
                cliente.SerializerSettings.Converters.Add(new FechaSoloConverter());
            }
        }

        public void Dispose() => _http.Dispose();

        /// <summary>La llave no se guarda en el objeto; un ToString nunca debe enseñar nada sensible.</summary>
        public override string ToString() => $"TrevolClient(environment={Environment}, apiVersion={ApiVersion})";
    }
}
