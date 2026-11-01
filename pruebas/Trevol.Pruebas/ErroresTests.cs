using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Trevol.Pruebas;

public class ErroresTests
{
    private static readonly Dictionary<string, int> Estado = new()
    {
        ["AuthenticationError"] = 401, ["PermissionDeniedError"] = 403, ["NotFoundError"] = 404, ["InvalidRequestError"] = 400, ["IdempotencyError"] = 409,
        ["ValidationError"] = 422, ["ConflictError"] = 409, ["PreconditionError"] = 422, ["DianRejectedError"] = 422, ["BalanceError"] = 402,
        ["RateLimitError"] = 429, ["DianUnavailableError"] = 503, ["InternalServerError"] = 500
    };

    public static IEnumerable<object[]> Codes() =>
        ((JObject)Banco.Leer("errores.json")["clases"]!).Properties().SelectMany(p => ((JArray)p.Value).Select(c => new object[] { p.Name, (string)c! }));

    [Fact]
    public void CubreLos30Code() => Assert.Equal(30, Codes().Count());

    [Theory]
    [MemberData(nameof(Codes))]
    public async Task CadaCodeVaASuClaseATravesDeUnMetodoGenerado(string clase, string code)
    {
        var b = new Banco(Banco.LlaveTest, o => o.MaxRetries = 0, Banco.Problema(Estado[clase], code));
        var e = await Assert.ThrowsAnyAsync<TrevolException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Equal("Trevol." + clase.Replace("Error", "Exception"), e.GetType().FullName);
        Assert.Equal(code, e.Code);
        Assert.Equal(Estado[clase], e.Status);
        Assert.Equal("req_c3c3c3c3c3c3c3c3c3c3c3c3", e.RequestId);
        Assert.Equal($"https://platform.trevol.si/docs/errores#{code}", e.Type);
        Assert.Equal(code, e.Problem!["code"]);
    }

    [Fact]
    public async Task ElErrorNoSeEnvuelveEnApiExceptionDelCodigoGenerado()
    {
        var b = new Banco(Banco.Problema(404, "not_found"));
        var e = await Assert.ThrowsAsync<NotFoundException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.IsNotType<Trevol.Generado.Client.ApiException>(e);
    }

    [Fact]
    public async Task UnCodeDesconocidoEsLaClaseBaseYNoRompe()
    {
        var b = new Banco(Banco.Problema(422, "code_del_futuro"));
        var e = await Assert.ThrowsAnyAsync<TrevolException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Equal(typeof(TrevolException), e.GetType());
        Assert.Equal("code_del_futuro", e.Code);
    }

    [Theory]
    [InlineData(502, typeof(InternalServerException))]
    [InlineData(404, typeof(NotFoundException))]
    [InlineData(409, typeof(ConflictException))]
    [InlineData(401, typeof(AuthenticationException))]
    public async Task UnaRespuestaQueNoEsJsonDaLaClasePorEstadoConCodeNullYElCuerpoRecortado(int estado, Type clase)
    {
        var html = "<html>" + new string('x', 5000) + "</html>";
        var b = new Banco(Banco.LlaveTest, o => o.MaxRetries = 0, Banco.Crudo(estado, Encoding.UTF8.GetBytes(html), "text/html"));
        var e = await Assert.ThrowsAnyAsync<TrevolException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Equal(clase, e.GetType());
        Assert.Null(e.Code);
        Assert.Equal(2048, e.Detail!.Length);
    }

    [Fact]
    public async Task TomaElRequestIdDeLaCabeceraCuandoElCuerpoNoLoTrae()
    {
        var cuerpo = "{\"type\":\"x\",\"title\":\"t\",\"status\":404,\"code\":\"not_found\",\"retryable\":false}";
        var b = new Banco(new Banco.Resp { Estado = 404, Cuerpo = Encoding.UTF8.GetBytes(cuerpo), Tipo = "application/problem+json", Cabeceras = { ["Request-Id"] = "req_d4d4d4d4d4d4d4d4d4d4d4d4" } });
        var e = await Assert.ThrowsAsync<NotFoundException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Equal("req_d4d4d4d4d4d4d4d4d4d4d4d4", e.RequestId);
    }

    [Fact]
    public async Task ValidationExceptionTraeErrorsConFieldPointerYMessage()
    {
        var b = new Banco(Banco.Problema(422, "validation_error", "{\"errors\":[{\"field\":\"items\",\"pointer\":\"/items\",\"message\":\"Falta\"}]}"));
        var e = await Assert.ThrowsAsync<ValidationException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        var x = e.Errors[0];
        Assert.Equal(("items", "/items", "Falta"), (x.Field, x.Pointer, x.Message));
    }

    [Fact]
    public async Task LosCamposExtraQuedanEnProblem()
    {
        var b = new Banco(Banco.Problema(409, "external_id_in_use", "{\"existing_id\":\"inv_x\"}"));
        var e = await Assert.ThrowsAsync<ConflictException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Equal("inv_x", e.Problem!["existing_id"]);
    }

    [Fact]
    public async Task LaLlaveNoApareceEnToStringMensajeNiRastroDeNingunError()
    {
        var b = new Banco(Banco.Problema(401, "invalid_api_key"));
        var e = await Assert.ThrowsAsync<AuthenticationException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        foreach (var t in new[] { e.ToString(), e.Message, e.StackTrace ?? "", b.Trevol.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(e.Problem) })
        {
            Assert.DoesNotContain(Banco.LlaveTest, t);
            Assert.DoesNotContain("a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1", t);
        }
        Assert.Contains("invalid_api_key", e.ToString());
        Assert.Contains("HTTP 401", e.ToString());
        Assert.Contains("req_", e.ToString());
    }

    [Fact]
    public async Task UnFalloDeRedEsApiConnectionExceptionConLaClaveYSeReintentaSoloSiEsSeguro()
    {
        var b = new Banco(Banco.Cortar());
        var e = await Assert.ThrowsAsync<ApiConnectionException>(() => b.Trevol.Invoices.CreateInvoiceAsync(IdempotenciaTests.Entrada()));
        Assert.NotNull(e.IdempotencyKey);
        Assert.Equal(3, b.Peticiones.Count); // con clave: 3 intentos
        var b2 = new Banco(Banco.Cortar());
        await Assert.ThrowsAsync<ApiConnectionException>(() => b2.Trevol.WebhookEndpoints.CreateWebhookEndpointAsync(TrevolJson.Deserialize<Trevol.Generado.Model.WebhookEndpointInput>("{\"url\":\"https://tu-servidor.co/h\",\"events\":[\"*\"]}")));
        Assert.Single(b2.Peticiones); // sin clave: un solo intento
    }

    [Fact]
    public async Task Un429ConRetryAfterMayorQueElMaximoNoEsperaYLanzaElErrorConRetryAfter()
    {
        var b = new Banco(Banco.Problema(429, "rate_limited", "{\"retryable\":true}", new() { ["Retry-After"] = "120" }));
        var e = await Assert.ThrowsAsync<RateLimitException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Equal(120.0, e.RetryAfter);
        Assert.Empty(b.Esperas);
        Assert.Single(b.Peticiones);
    }

    [Fact]
    public async Task Un202EsUnResultadoNoUnErrorNiUnReintento()
    {
        var b = new Banco(Banco.Json(202, "{\"id\":\"" + Banco.IdDoc + "\",\"object\":\"invoice\",\"status\":\"processing\"}"));
        var doc = await b.Trevol.Invoices.CreateInvoiceAsync(IdempotenciaTests.Entrada());
        Assert.Equal("processing", doc.Status);
        Assert.Single(b.Peticiones);
    }
}
