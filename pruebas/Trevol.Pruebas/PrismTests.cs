using Newtonsoft.Json.Linq;
using Trevol.Generado.Model;
using Xunit;

namespace Trevol.Pruebas;

/// <summary>
/// Contra Prism (servidor simulado desde docs/api/openapi.json, docs/api/43 §2.10.2):
/// <c>sdks/herramientas/prism.sh -- bash -c 'cd sdks/csharp &amp;&amp; dotnet test pruebas/Trevol.Pruebas'</c>.
/// Prism valida cada petición contra la especificación PÚBLICA: si el SDK serializa mal un cuerpo, olvida una cabecera o la Idempotency-Key,
/// responde 4xx y la prueba falla (el motivo viene en la cabecera <c>sl-violations</c>, que la excepción conserva).
/// </summary>
public class PrismTests
{
    private static readonly string Url = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL") ?? "";
    private static readonly string Llave = Environment.GetEnvironmentVariable("TREVOL_API_KEY") ?? Banco.LlaveTest;

    private static TrevolClient Cliente(int reintentos = 0, HttpMessageHandler? handler = null, Func<double, CancellationToken, Task>? dormir = null) =>
        new(Llave, new TrevolClientOptions { BaseUrl = Url, MaxRetries = reintentos, Handler = handler, Dormir = dormir });

    private static T Ejemplo<T>(string op, string? nombre = null)
    {
        foreach (var o in (JArray)JArray.Parse(Banco.Datos("operaciones.json")))
        {
            if ((string)o["operationId"]! != op) continue;
            foreach (var e in (JArray)o["ejemplos"]!)
                if (nombre == null || (string)e["nombre"]! == nombre) return TrevolJson.Deserialize<T>(e["valor"]!.ToString());
        }
        throw new InvalidOperationException($"no hay ejemplo {nombre} de {op}");
    }

    private static async Task<T> ConViolaciones<T>(Func<Task<T>> f)
    {
        try
        {
            return await f();
        }
        catch (TrevolException e)
        {
            var v = e.Headers.FirstOrDefault(h => h.Key.Equals("sl-violations", StringComparison.OrdinalIgnoreCase)).Value;
            if (v != null && v.Count > 0) Assert.Fail($"Prism rechazó la petición ({e.Status}): {string.Join(",", v)}");
            throw;
        }
    }

    /// <summary>Un manejador que agrega <c>Prefer</c> (Prism devuelve ese error o ejemplo).</summary>
    private sealed class ConPrefer : DelegatingHandler
    {
        private readonly string _prefer;
        public ConPrefer(string prefer) : base(new HttpClientHandler()) { _prefer = prefer; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            request.Headers.TryAddWithoutValidation("Prefer", _prefer);
            return base.SendAsync(request, ct);
        }
    }

    [PrismFact]
    public async Task CreaUnaFactura201ConElCuerpoDeEjemplo()
    {
        var doc = await ConViolaciones(() => Cliente().Invoices.CreateInvoiceAsync(Ejemplo<InvoiceInput>("createInvoice")));
        Assert.Equal("invoice", doc.Object.ToString().ToLowerInvariant());
    }

    [PrismFact]
    public async Task CreaUnaNotaCreditoYCalculaUnPreview()
    {
        var t = Cliente();
        var nc = await ConViolaciones(() => t.CreditNotes.CreateCreditNoteAsync(Ejemplo<NoteInput>("createCreditNote")));
        var p = await ConViolaciones(() => t.Invoices.PreviewInvoiceAsync(Ejemplo<InvoiceInput>("previewInvoice")));
        Assert.NotNull(nc);
        Assert.Equal("preview", p.Object.ToString().ToLowerInvariant());
        Assert.NotNull(p.Totals);
    }

    [PrismFact]
    public async Task CorrigeUnaFacturaRechazadaConPut()
    {
        Assert.NotNull(await ConViolaciones(() => Cliente().Invoices.CorrectInvoiceAsync("inv_" + new string('0', 32), Ejemplo<InvoiceInput>("correctInvoice"))));
    }

    [PrismFact]
    public async Task ListaFacturasYEventosEIteraConTopeDe3()
    {
        var t = Cliente();
        var vistos = new List<string>();
        await foreach (var f in t.PaginateAsync(c => t.Invoices.ListInvoicesAsync(limit: 2, startingAfter: c)))
        {
            vistos.Add(f.Id);
            if (vistos.Count == 3) break; // Prism repite siempre la misma página con has_more=true (C8): el tope es de la prueba
        }
        Assert.NotEmpty(vistos);
        var n = 0;
        await foreach (var e in t.PaginateAsync(c => t.Events.ListEventsAsync(startingAfter: c)))
        {
            Assert.NotNull(e.Id);
            if (++n == 3) break;
        }
        Assert.True(n > 0);
    }

    [PrismFact]
    public async Task DescargaPdfXmlZipYRespuestaDianLosBytesLleganYElTipoEsElCorrecto()
    {
        var t = Cliente();
        var id = "inv_" + new string('0', 32);
        foreach (var descarga in new Func<Task<Generado.Client.FileParameter>>[]
            { () => t.Invoices.DownloadInvoicePdfAsync(id), () => t.Invoices.DownloadInvoiceXmlAsync(id), () => t.Invoices.DownloadInvoiceZipAsync(id), () => t.Invoices.DownloadInvoiceDianResponseAsync(id) })
        {
            var f = await ConViolaciones(descarga);
            using var ms = new MemoryStream();
            await f.Content.CopyToAsync(ms);
            Assert.True(ms.Length > 0);
        }
        var r = await t.Invoices.DownloadInvoicePdfWithHttpInfoAsync(id);
        Assert.Equal(System.Net.HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("application/pdf", string.Join(",", r.Headers.First(h => h.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase)).Value));
    }

    [PrismFact]
    public async Task PreferCode422SobreUnEventoRadianEsPreconditionExceptionTacitAcceptanceNotDue()
    {
        var t = Cliente(handler: new ConPrefer("code=422, example=tacita"));
        var e = await Assert.ThrowsAsync<PreconditionException>(() => t.RadianEvents.CreateRadianEventAsync(Ejemplo<RadianEventInput>("createRadianEvent", "tacita")));
        Assert.Equal("tacit_acceptance_not_due", e.Code);
        Assert.Equal(422, e.Status);
    }

    [PrismFact]
    public async Task PreferCode409SobreUnEventoRadianEsConflictException()
    {
        var t = Cliente(handler: new ConPrefer("code=409, example=duplicado"));
        var e = await Assert.ThrowsAsync<ConflictException>(() => t.RadianEvents.CreateRadianEventAsync(Ejemplo<RadianEventInput>("createRadianEvent", "acuse")));
        Assert.Equal(409, e.Status);
        Assert.StartsWith("rev_", (string)e.Problem!["existing_id"]!);
    }

    [PrismFact]
    public async Task PreferCode429EsUnaExcepcionConStatus429RetryableYLaClave()
    {
        var t = Cliente(handler: new ConPrefer("code=429"));
        var e = await Assert.ThrowsAnyAsync<TrevolException>(() => t.Invoices.CreateInvoiceAsync(Ejemplo<InvoiceInput>("createInvoice")));
        Assert.Equal(429, e.Status);
        Assert.True(e.Retryable);
        Assert.NotNull(e.IdempotencyKey);
    }

    [PrismFact]
    public async Task CreaUnEndpointDeWebhooksYMandaLaPrueba202()
    {
        var t = Cliente();
        var ep = await ConViolaciones(() => t.WebhookEndpoints.CreateWebhookEndpointAsync(Ejemplo<WebhookEndpointInput>("createWebhookEndpoint")));
        var entrega = await ConViolaciones(() => t.WebhookEndpoints.TestWebhookEndpointAsync("we_" + new string('0', 32)));
        Assert.Equal("webhookendpoint", ep.Object.ToString().ToLowerInvariant().Replace("_", ""));
        Assert.NotNull(entrega);
    }

    [PrismFact]
    public async Task CompanyYAcquirerDevuelvenModelosNoVoid()
    {
        var t = Cliente();
        var c = await ConViolaciones(() => t.General.RetrieveCompanyAsync());
        var a = await ConViolaciones(() => t.General.RetrieveDianAcquirerAsync("31", "900373591"));
        Assert.NotNull(c.Object);
        Assert.NotNull(a.Object);
    }

    [PrismFact]
    public async Task ReintentaContraPrismConLaMismaClaveYAgotaConElErrorDel503()
    {
        var esperas = new List<double>();
        var t = Cliente(1, new ConPrefer("code=503"), (s, _) => { esperas.Add(s); return Task.CompletedTask; });
        var e = await Assert.ThrowsAnyAsync<TrevolException>(() => t.Invoices.CreateInvoiceAsync(Ejemplo<InvoiceInput>("createInvoice"), "PRUEBA-PRISM-1"));
        Assert.Equal(503, e.Status);
        Assert.Equal("PRUEBA-PRISM-1", e.IdempotencyKey);
        Assert.Single(esperas);
    }
}
