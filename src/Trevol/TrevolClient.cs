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
        /// <summary>Si se pasa y la llave no coincide, <see cref="ConfigurationException"/> ANTES de llamar. Es solo una comprobación: el entorno sale del prefijo de la llave.</summary>
        public TrevolEnvironment? Environment { get; set; }
        /// <summary>
        /// Solo con una llave de administración (<c>tva_…</c>): la empresa (<c>com_…</c>) de las rutas de una empresa. El SDK la manda en
        /// <c>Trevol-Company</c>, con <c>Trevol-Environment</c> del prefijo de la llave. Con una llave <c>tvl_</c>, <see cref="ConfigurationException"/>.
        /// Para otra empresa con el mismo cliente, <see cref="TrevolClient.WithCompany"/>.
        /// </summary>
        public string? Company { get; set; }
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
        private readonly Transporte _transporte;
        private readonly Configuration _config;

        /// <summary><c>Test</c> (llave tvl_test_ o tva_test_, habilitación de la DIAN) o <c>Live</c> (tvl_live_ o tva_live_, producción).</summary>
        public TrevolEnvironment Environment { get; }
        /// <summary><c>Api</c> (llave <c>tvl_</c>, de una empresa) o <c>Account</c> (llave <c>tva_</c>, de administración).</summary>
        public TrevolKeyType KeyType { get; }
        /// <summary>La empresa (<c>com_…</c>) de las rutas de una empresa con una llave de administración; null si no hay (y siempre con una <c>tvl_</c>).</summary>
        public string? Company { get; }
        /// <summary>El valor de <c>Trevol-Version</c> que se manda en cada petición.</summary>
        public string ApiVersion { get; }

        /// <summary>Lo de una cuenta con varias empresas (<c>/v1/account</c>): pide una llave de administración (<c>tva_…</c>) y no usa <see cref="Company"/>.</summary>
        public AccountApi Account { get; private set; }
        public BalanceApi Balance { get; private set; }
        public CreditNotesApi CreditNotes { get; private set; }
        public DebitNotesApi DebitNotes { get; private set; }
        public EquivalentDocumentAdjustmentsApi EquivalentDocumentAdjustments { get; private set; }
        public EquivalentDocumentsApi EquivalentDocuments { get; private set; }
        public EventsApi Events { get; private set; }
        public GeneralApi General { get; private set; }
        public InvoicesApi Invoices { get; private set; }
        public PayrollAdjustmentsApi PayrollAdjustments { get; private set; }
        public PayrollsApi Payrolls { get; private set; }
        public RadianEventsApi RadianEvents { get; private set; }
        public RequestsApi Requests { get; private set; }
        public SupportDocumentAdjustmentsApi SupportDocumentAdjustments { get; private set; }
        public SupportDocumentsApi SupportDocuments { get; private set; }
        public WebhookEndpointsApi WebhookEndpoints { get; private set; }
        public CustomersApi Customers { get; private set; }
        public DianDocumentsApi DianDocuments { get; private set; }
        public DianSyncsApi DianSyncs { get; private set; }
        public InvoiceBatchesApi InvoiceBatches { get; private set; }
        public NumberingAssociationsApi NumberingAssociations { get; private set; }
        public NumberingRangesApi NumberingRanges { get; private set; }
        public NumberingRequestsApi NumberingRequests { get; private set; }
        public PaperNumberingRangesApi PaperNumberingRanges { get; private set; }
        public ProductsApi Products { get; private set; }
        public RadianStatusApi RadianStatus { get; private set; }
        public ReportRunsApi ReportRuns { get; private set; }
        public ReportsApi Reports { get; private set; }
        // 0.4.0: las listas de valores y el diseño del PDF (plantillas, diseños, ajustes y logo de la empresa).
        public CatalogsApi Catalogs { get; private set; }
        public CompanySettingsApi CompanySettings { get; private set; }
        public PdfDesignsApi PdfDesigns { get; private set; }
        public PdfTemplatesApi PdfTemplates { get; private set; }

        /// <param name="apiKey"><c>tvl_test_…</c> o <c>tvl_live_…</c> (de una empresa), o <c>tva_test_…</c> o <c>tva_live_…</c> (de administración); si falta se lee <c>TREVOL_API_KEY</c>.</param>
        /// <param name="options">Las opciones; ninguna es obligatoria.</param>
        public TrevolClient(string? apiKey = null, TrevolClientOptions? options = null)
        {
            options ??= new TrevolClientOptions();
            var llave = apiKey ?? System.Environment.GetEnvironmentVariable("TREVOL_API_KEY");
            if (string.IsNullOrWhiteSpace(llave))
                throw new ConfigurationException("Falta la llave de la API: pásala a new TrevolClient(...) o define la variable de entorno TREVOL_API_KEY.");
            (KeyType, Environment) = Llave.Leer(llave);
            var prefijo = KeyType == TrevolKeyType.Account ? "tva" : "tvl";
            if (options.Environment != null && options.Environment != Environment)
                throw new ConfigurationException($"Pediste el entorno «{options.Environment.ToString()!.ToLowerInvariant()}» pero la llave es de «{Environment.ToString().ToLowerInvariant()}» " +
                    $"({(Environment == TrevolEnvironment.Test ? prefijo + "_test_ = habilitación" : prefijo + "_live_ = producción")}).");
            if (options.Company != null) Company = Llave.Empresa(options.Company, KeyType);
            if (options.MaxRetries < 0) throw new ConfigurationException("MaxRetries no puede ser negativo.");
            ApiVersion = options.ApiVersion ?? ApiVersionDefecto;
            var baseUrl = options.BaseUrl.TrimEnd('/');
            var ruta = new Uri(baseUrl).AbsolutePath.TrimEnd('/');

            _transporte = new Transporte(
                options.Handler ?? new HttpClientHandler(), llave!.Trim(), KeyType, Environment, ApiVersion,
                $"trevol-dotnet/{Version} ({System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Trim()}; +https://platform.trevol.si/docs)",
                options.MaxRetries, options.MaxRetryAfter.TotalSeconds, options.Timeout, ruta,
                options.Dormir ?? ((s, ct) => Task.Delay(TimeSpan.FromSeconds(s), ct)),
                options.Azar ?? (() => new Random().NextDouble()), options.Uuid ?? (() => Guid.NewGuid().ToString()), options.Ahora ?? (() => DateTimeOffset.UtcNow));
            // El tiempo por intento lo pone el transporte: el HttpClient no limita el conjunto de intentos.
            _http = new HttpClient(new ConEmpresa(Company, _transporte), true) { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
            _config = new Configuration { BasePath = baseUrl };
            Recursos(_http, _config);
        }

        /// <summary>El cliente derivado de <see cref="WithCompany"/>: la misma llave, el mismo transporte y las mismas conexiones; otra empresa.</summary>
        private TrevolClient(TrevolClient padre, string empresa)
        {
            Environment = padre.Environment;
            KeyType = padre.KeyType;
            ApiVersion = padre.ApiVersion;
            Company = empresa;
            _transporte = padre._transporte;
            _config = padre._config;
            // disposeHandler: false: cerrar el derivado no cierra el transporte del cliente original (ese lo cierra su Dispose).
            _http = new HttpClient(new ConEmpresa(empresa, _transporte), false) { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
            Recursos(_http, _config);
        }

        /// <summary>
        /// Solo con una llave de administración (<c>tva_…</c>): un cliente para la empresa <paramref name="company"/> (<c>com_…</c>) que comparte la
        /// llave, el transporte y las conexiones de este; crearlo es barato (uno por petición, si quieres). Este cliente no cambia. Con una llave
        /// <c>tvl_</c> o un id mal formado, <see cref="ConfigurationException"/>. El derivado deja de servir cuando se cierra (<see cref="Dispose"/>) el original.
        /// </summary>
        public TrevolClient WithCompany(string company) => new TrevolClient(this, Llave.Empresa(company, KeyType));

        private void Recursos(HttpClient http, Configuration config)
        {
            Account = new AccountApi(http, config);
            Balance = new BalanceApi(http, config);
            CreditNotes = new CreditNotesApi(http, config);
            DebitNotes = new DebitNotesApi(http, config);
            EquivalentDocumentAdjustments = new EquivalentDocumentAdjustmentsApi(http, config);
            EquivalentDocuments = new EquivalentDocumentsApi(http, config);
            Events = new EventsApi(http, config);
            General = new GeneralApi(http, config);
            Invoices = new InvoicesApi(http, config);
            PayrollAdjustments = new PayrollAdjustmentsApi(http, config);
            Payrolls = new PayrollsApi(http, config);
            RadianEvents = new RadianEventsApi(http, config);
            Requests = new RequestsApi(http, config);
            SupportDocumentAdjustments = new SupportDocumentAdjustmentsApi(http, config);
            SupportDocuments = new SupportDocumentsApi(http, config);
            WebhookEndpoints = new WebhookEndpointsApi(http, config);
            Customers = new CustomersApi(http, config);
            DianDocuments = new DianDocumentsApi(http, config);
            DianSyncs = new DianSyncsApi(http, config);
            InvoiceBatches = new InvoiceBatchesApi(http, config);
            NumberingAssociations = new NumberingAssociationsApi(http, config);
            NumberingRanges = new NumberingRangesApi(http, config);
            NumberingRequests = new NumberingRequestsApi(http, config);
            PaperNumberingRanges = new PaperNumberingRangesApi(http, config);
            Products = new ProductsApi(http, config);
            RadianStatus = new RadianStatusApi(http, config);
            ReportRuns = new ReportRunsApi(http, config);
            Reports = new ReportsApi(http, config);
            Catalogs = new CatalogsApi(http, config);
            CompanySettings = new CompanySettingsApi(http, config);
            PdfDesigns = new PdfDesignsApi(http, config);
            PdfTemplates = new PdfTemplatesApi(http, config);

            // Piloto: el generador marca como «emitir aunque sea nulo» los campos opcionales sin tipo (p. ej. `rounding`), y un `"rounding": null` que la
            // persona no escribió lo rechaza la API (422). Una propiedad sin valor se omite, como en los demás SDK.
            foreach (var cliente in new[]
            {
                Account.ApiClient, Balance.ApiClient, CreditNotes.ApiClient, DebitNotes.ApiClient, EquivalentDocumentAdjustments.ApiClient, EquivalentDocuments.ApiClient,
                Events.ApiClient, General.ApiClient, Invoices.ApiClient, PayrollAdjustments.ApiClient, Payrolls.ApiClient, RadianEvents.ApiClient,
                Requests.ApiClient, SupportDocumentAdjustments.ApiClient, SupportDocuments.ApiClient, WebhookEndpoints.ApiClient,
                Customers.ApiClient, DianDocuments.ApiClient, DianSyncs.ApiClient, InvoiceBatches.ApiClient, NumberingAssociations.ApiClient,
                NumberingRanges.ApiClient, NumberingRequests.ApiClient, PaperNumberingRanges.ApiClient, Products.ApiClient, RadianStatus.ApiClient,
                ReportRuns.ApiClient, Reports.ApiClient, Catalogs.ApiClient, CompanySettings.ApiClient, PdfDesigns.ApiClient,
                PdfTemplates.ApiClient
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
        public override string ToString() => $"TrevolClient(environment={Environment}, keyType={KeyType}, company={Company ?? "null"}, apiVersion={ApiVersion})";
    }
}
