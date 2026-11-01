using Trevol.Generado.Model;
using Xunit;

namespace Trevol.Pruebas;

public class IdempotenciaTests
{
    internal static InvoiceInput Entrada() => TrevolJson.Deserialize<InvoiceInput>(Banco.EntradaFactura);

    [Fact]
    public async Task GeneraUnaClaveYLaRepiteEnLosTresIntentos()
    {
        var b = new Banco(Banco.Problema(503, "dian_unavailable", "{\"retryable\":true}"), Banco.Problema(503, "dian_unavailable", "{\"retryable\":true}"), Banco.Json(201, Banco.Documento));
        await b.Trevol.Invoices.CreateInvoiceAsync(Entrada());
        var claves = b.Peticiones.Select(p => p.Cabeceras["Idempotency-Key"]).ToArray();
        Assert.Equal(3, claves.Length);
        Assert.Equal(new[] { "00000000-0000-4000-8000-000000000001" }, claves.Distinct().ToArray());
        Assert.Single(b.Peticiones.Select(p => p.Cuerpo).Distinct()); // el cuerpo viaja entero en cada intento
        Assert.Contains("\"external_id\":\"VENTA-2026-000123\"", b.Peticiones[2].Cuerpo.Replace(" ", ""));
        Assert.Equal(2, b.Esperas.Count);
    }

    [Fact]
    public async Task RespetaLaClaveDeLaPersona()
    {
        var b = new Banco(Banco.Json(201, Banco.Documento));
        await b.Trevol.Invoices.CreateInvoiceAsync(Entrada(), "VENTA-2026-000123");
        Assert.Equal("VENTA-2026-000123", b.Peticiones[0].Cabeceras["Idempotency-Key"]);
    }

    [Theory]
    [InlineData("con espacio")]
    [InlineData("tilde-é")]
    public async Task UnaClaveMalaSeRechazaLocalmenteSinRed(string mala)
    {
        var b = new Banco(Banco.Json(201, Banco.Documento));
        await Assert.ThrowsAsync<ConfigurationException>(() => b.Trevol.Invoices.CreateInvoiceAsync(Entrada(), mala));
        Assert.Empty(b.Peticiones);
    }

    [Fact]
    public async Task UnaClaveLargaSeRechazaLocalmenteSinRed()
    {
        var b = new Banco(Banco.Json(201, Banco.Documento));
        await Assert.ThrowsAsync<ConfigurationException>(() => b.Trevol.Invoices.CreateInvoiceAsync(Entrada(), new string('x', 256)));
        Assert.Empty(b.Peticiones);
    }

    [Fact]
    public async Task LaLlevaElErrorParaReintentarMasTardeConLaMisma()
    {
        var b = new Banco(Banco.Problema(422, "dian_rejected"));
        var e = await Assert.ThrowsAsync<DianRejectedException>(() => b.Trevol.Invoices.CreateInvoiceAsync(Entrada()));
        Assert.Equal("00000000-0000-4000-8000-000000000001", e.IdempotencyKey);
    }

    [Fact]
    public async Task CrearCorregirYLaRecargaLaLlevanElRestoNo()
    {
        Banco.Resp SegunLaRuta(Banco.Pet p)
        {
            if (p.Ruta.EndsWith("/top_ups")) return Banco.Json(201, "{\"object\":\"top_up\",\"reference\":\"TRV-REC-1\",\"amount_cop\":50000,\"status\":\"prepared\"}");
            if (p.Ruta.EndsWith("/webhook_endpoints")) return Banco.Json(201, "{\"id\":\"we_" + new string('a', 32) + "\",\"object\":\"webhook_endpoint\",\"environment\":\"test\"}");
            if (p.Metodo == "POST" && p.Ruta.EndsWith("/invoices")) return Banco.Json(201, Banco.Documento);
            return Banco.Json(200, Banco.Documento);
        }
        var b = new Banco((Func<Banco.Pet, Banco.Resp>)SegunLaRuta);
        var t = b.Trevol;
        await t.Invoices.CreateInvoiceAsync(Entrada());
        await t.Invoices.CorrectInvoiceAsync(Banco.IdDoc, Entrada());
        await t.Balance.CreateBalanceTopUpAsync(new TopUpInput(50000));
        await t.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        await t.Invoices.SendInvoiceAsync(Banco.IdDoc);
        await t.WebhookEndpoints.CreateWebhookEndpointAsync(TrevolJson.Deserialize<WebhookEndpointInput>("{\"url\":\"https://tu-servidor.co/h\",\"events\":[\"*\"]}"));
        await t.Invoices.PreviewInvoiceAsync(Entrada());
        var vistas = b.Peticiones.Select(p => $"{p.Metodo} {p.Ruta.Split('?')[0]} {(p.Cabeceras.ContainsKey("Idempotency-Key") ? "True" : "False")}").ToArray();
        Assert.Equal(new[]
        {
            "POST /v1/invoices True", $"PUT /v1/invoices/{Banco.IdDoc} True", "POST /v1/balance/top_ups True", $"GET /v1/invoices/{Banco.IdDoc} False",
            $"POST /v1/invoices/{Banco.IdDoc}/send False", "POST /v1/webhook_endpoints False", "POST /v1/invoices/preview False"
        }, vistas);
    }
}
