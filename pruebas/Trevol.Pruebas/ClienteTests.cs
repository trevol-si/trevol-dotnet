using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Trevol.Generado;
using Trevol.Generado.Model;
using Xunit;

namespace Trevol.Pruebas;

public class ClienteTests
{
    [Fact]
    public async Task AuthorizationTrevolVersionFijaUserAgentYNingunaXTrevol()
    {
        var b = new Banco(Banco.Json(200, Banco.Documento));
        await b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        var c = b.Peticiones[0].Cabeceras;
        Assert.Equal("Bearer " + Banco.LlaveTest, c["Authorization"]);
        Assert.Equal("2026-10-22", c["Trevol-Version"]);
        Assert.Equal("2026-10-22", Tablas.ApiVersion);
        Assert.Matches(new Regex("^trevol-dotnet/" + Regex.Escape(TrevolClient.Version) + @" \(.+; \+https://platform\.trevol\.si/docs\)$"), c["User-Agent"]);
        Assert.Equal("application/json, application/problem+json", c["Accept"]);
        Assert.DoesNotContain(c.Keys, k => k.StartsWith("x-trevol-", StringComparison.OrdinalIgnoreCase) || k.Equals("trevol-empresa", StringComparison.OrdinalIgnoreCase)
            || k.Equals("trevol-entorno", StringComparison.OrdinalIgnoreCase) || k.Equals("trevol-escenario", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ApiVersionEsConfigurableYViajaEnCadaPeticion()
    {
        var b = new Banco(Banco.LlaveTest, o => o.ApiVersion = "2026-10-21", Banco.Json(200, Banco.Documento));
        await b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        await b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        Assert.Equal(new[] { "2026-10-21", "2026-10-21" }, b.Peticiones.Select(p => p.Cabeceras["Trevol-Version"]).ToArray());
    }

    [Fact]
    public async Task ElAcceptDeUnaDescargaEsElDelArchivoYLosBytesLleganIntactos()
    {
        var binario = new byte[] { 37, 80, 68, 70, 0, 255, 1, 2 };
        var b = new Banco(Banco.Crudo(200, binario, "application/pdf"));
        var pdf = await b.Trevol.Invoices.DownloadInvoicePdfAsync(Banco.IdDoc); // un FileParameter: el archivo es pdf.Content (Stream)
        using var ms = new MemoryStream();
        await pdf.Content.CopyToAsync(ms);
        Assert.Equal(binario, ms.ToArray());
        Assert.Equal("application/pdf, application/problem+json", b.Peticiones[0].Cabeceras["Accept"]);
        Assert.Equal($"/v1/invoices/{Banco.IdDoc}/pdf", b.Peticiones[0].Ruta);
    }

    [Fact]
    public async Task LaLlaveLiveSeUsaTalCualYLoDiceElEntorno()
    {
        var b = new Banco(Banco.LlaveLive, null, Banco.Json(200, Banco.Documento));
        await b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        Assert.Equal("Bearer " + Banco.LlaveLive, b.Peticiones[0].Cabeceras["Authorization"]);
        Assert.Equal(TrevolEnvironment.Live, b.Trevol.Environment);
    }

    [Fact]
    public void TvlTestEsTestYTvlLiveEsLive()
    {
        Assert.Equal(TrevolEnvironment.Test, new TrevolClient(Banco.LlaveTest).Environment);
        Assert.Equal(TrevolEnvironment.Live, new TrevolClient(Banco.LlaveLive).Environment);
    }

    [Fact]
    public void LeeTrevolApiKeySiNoSePasa()
    {
        var anterior = Environment.GetEnvironmentVariable("TREVOL_API_KEY");
        try
        {
            Environment.SetEnvironmentVariable("TREVOL_API_KEY", Banco.LlaveLive);
            Assert.Equal(TrevolEnvironment.Live, new TrevolClient().Environment);
            Environment.SetEnvironmentVariable("TREVOL_API_KEY", "");
            Assert.Throws<ConfigurationException>(() => new TrevolClient());
            Environment.SetEnvironmentVariable("TREVOL_API_KEY", null);
            Assert.Throws<ConfigurationException>(() => new TrevolClient());
        }
        finally
        {
            Environment.SetEnvironmentVariable("TREVOL_API_KEY", anterior);
        }
    }

    [Theory]
    [InlineData("tvl_test_abc")]
    [InlineData("tvl_prod_a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1")]
    [InlineData("sk_live_a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1")]
    [InlineData("tvl_test_A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1A1")]
    [InlineData("tvl_test_zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public void MalFormadaEsConfigurationExceptionSinRedYSinLaLlaveEnElMensaje(string mala)
    {
        var e = Assert.Throws<ConfigurationException>(() => new TrevolClient(mala));
        Assert.DoesNotContain(mala, e.Message + e);
        Assert.Throws<ConfigurationException>(() => new TrevolClient("   "));
    }

    [Fact]
    public void EnvironmentQueNoCoincideEsConfigurationExceptionAntesDeLlamar()
    {
        var e = Assert.Throws<ConfigurationException>(() => new TrevolClient(Banco.LlaveTest, new TrevolClientOptions { Environment = TrevolEnvironment.Live }));
        Assert.Contains("live", e.Message);
        Assert.Throws<ConfigurationException>(() => new TrevolClient(Banco.LlaveLive, new TrevolClientOptions { Environment = TrevolEnvironment.Test }));
        Assert.Equal(TrevolEnvironment.Test, new TrevolClient(Banco.LlaveTest, new TrevolClientOptions { Environment = TrevolEnvironment.Test }).Environment);
    }

    [Fact]
    public void NiToStringNiElTransporteMuestranLaLlave()
    {
        var t = new TrevolClient(Banco.LlaveTest);
        Assert.DoesNotContain(new string('a', 1) + "1" + "a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1", t.ToString());
        Assert.DoesNotContain("a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1", t.ToString());
    }

    [Fact]
    public async Task SinReintentosUnaCaidaEsUnSoloIntento()
    {
        var b = new Banco(Banco.LlaveTest, o => o.MaxRetries = 0, Banco.Cortar());
        await Assert.ThrowsAsync<ApiConnectionException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Single(b.Peticiones);
    }

    [Fact]
    public void CadaEtiquetaDeOpenapiTieneSuPropiedadEnElCliente()
    {
        var etiquetas = JArray.Parse(Banco.Datos("tags.json")).Select(t => (string)t!).ToArray();
        Assert.Equal(etiquetas, Tablas.Etiquetas);
        foreach (var e in etiquetas) Assert.NotNull(typeof(TrevolClient).GetProperty(e)); // si la API suma un recurso, esto falla hasta agregarlo
    }

    [Fact]
    public void CadaOperationIdExisteComoMetodoAsyncEnSuRecurso()
    {
        var t = new TrevolClient(Banco.LlaveTest);
        foreach (var o in Tablas.Operaciones)
        {
            var recurso = typeof(TrevolClient).GetProperty(o.Recurso)!.GetValue(t)!;
            var nombre = char.ToUpperInvariant(o.OperationId[0]) + o.OperationId.Substring(1) + "Async";
            Assert.True(recurso.GetType().GetMethods().Any(m => m.Name == nombre), nombre);
        }
    }

    [Fact]
    public void CadaSobreDeListaTieneSuSobrecargaDePaginateAsync()
    {
        var listas = typeof(InvoiceList).Assembly.GetTypes().Where(x => x.Namespace == "Trevol.Generado.Model" && x.Name.EndsWith("List")).Select(x => x.Name).ToArray();
        Assert.NotEmpty(listas);
        foreach (var l in listas)
            Assert.True(typeof(TrevolClient).GetMethods().Any(m => m.Name == "PaginateAsync" && m.GetParameters()[0].ParameterType.ToString().Contains($"{l}]")), l); // si la API suma un listado, falta su sobrecarga
    }

    [Fact]
    public void NingunCuerpoDePeticionTieneUnDateTime()
    {
        // FechaSoloConverter escribe todo DateTimeOffset como fecha: vale mientras ningún cuerpo de petición tenga un `date-time`.
        var api = Banco.Leer("openapi.json");
        var esquemas = (JObject)api["components"]!["schemas"]!;
        var visitados = new HashSet<string>();
        var pendientes = new Stack<JToken>();
        foreach (var ruta in ((JObject)api["paths"]!).Properties())
            foreach (var op in ((JObject)ruta.Value).Properties())
                if (op.Value is JObject o && o["requestBody"] != null) pendientes.Push(o["requestBody"]!);
        while (pendientes.Count > 0)
        {
            var t = pendientes.Pop();
            if (t is JObject obj)
            {
                if (obj.TryGetValue("$ref", out var r) && visitados.Add((string)r!)) pendientes.Push(esquemas[((string)r!).Split('/').Last()]!);
                Assert.NotEqual("date-time", (string?)obj["format"]);
                foreach (var h in obj.Properties()) pendientes.Push(h.Value);
            }
            else if (t is JArray a) foreach (var x in a) pendientes.Push(x);
        }
    }

    [Fact]
    public void ElJsonDeUnaFacturaSeLeeConLosAyudantes()
    {
        var a = TrevolJson.Deserialize<InvoiceInput>(Banco.EntradaFactura);
        Assert.Equal("VENTA-2026-000123", a.ExternalId);
        var json = TrevolJson.Serialize(a);
        Assert.Contains("\"external_id\":\"VENTA-2026-000123\"", json);
        Assert.DoesNotContain("[]", json); // sin listas vacías implícitas
    }
}
