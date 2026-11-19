using Newtonsoft.Json.Linq;
using Trevol.Generado;
using Trevol.Generado.Model;
using Xunit;

namespace Trevol.Pruebas;

/// <summary>
/// La llave de administración (<c>tva_test_</c> / <c>tva_live_</c>, docs/api/35): el formato y el tipo de la llave, la empresa del cliente y la de
/// <c>WithCompany</c>, las cabeceras <c>Trevol-Company</c> y <c>Trevol-Environment</c> (solo con una <c>tva_</c> y solo en las rutas de una empresa),
/// el error local sin empresa, el recurso <c>Account</c> con su paginación y que la llave nunca aparece en un mensaje ni en un ToString.
/// </summary>
public class LlaveAdministracionTests
{
    public const string AdminTest = "tva_test_c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4";
    public const string AdminLive = "tva_live_d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5d5";
    public const string Empresa = "com_3f0c9a1e5b2d4c7f8a6e1d2c3b4a5f60";
    public const string OtraEmpresa = "com_0a1b2c3d4e5f60718293a4b5c6d7e8f9";

    private const string Compania = "{\"id\":\"com_3f0c9a1e5b2d4c7f8a6e1d2c3b4a5f60\",\"object\":\"company\",\"nit\":\"900373427\",\"check_digit\":\"6\",\"name\":\"DISTRIBUIDORA LOS ANDES SAS\",\"trade_name\":null,\"status\":\"enabled\",\"onboarding\":{\"step\":\"done\",\"stop_reason\":null},\"emission_environment\":\"live\",\"created_at\":\"2026-11-01T10:00:00-05:00\",\"enabled_at\":\"2026-11-02T10:00:00-05:00\",\"onboarding_url\":\"https://app.trevol.si/empresas/3f0c9a1e-5b2d-4c7f-8a6e-1d2c3b4a5f60\",\"api_keys\":{\"active_test\":1,\"active_live\":0}}";
    private const string InfoLlave = "{\"object\":\"account_key_info\",\"account\":{\"id\":\"acct_7d2e4b6a1c3f4e8d9b0a2c4e6f8a1b3d\"},\"key\":{\"id\":\"akey_9c1e3a5b7d2f4c6e8a0b1d3f5a7c9e2b\",\"object\":\"account_key\",\"prefix\":\"tva_test_c4c4c\",\"environment\":\"test\",\"permissions\":[\"companies:read\",\"documents:write\"],\"allowed_ips\":[],\"api_version\":\"2026-11-18\",\"scope\":{\"type\":\"all\",\"companies\":[]}}}";

    private static string ListaCompanias(bool hayMas, string? siguiente, params string[] ids) =>
        "{\"object\":\"list\",\"has_more\":" + (hayMas ? "true" : "false") + ",\"next_cursor\":" + (siguiente == null ? "null" : $"\"{siguiente}\"") +
        ",\"data\":[" + string.Join(",", ids.Select(i => Compania.Replace(Empresa, i))) + "]}";

    private static void SinCabecerasDeEmpresa(Banco.Pet p)
    {
        Assert.False(p.Cabeceras.ContainsKey("Trevol-Company"), p.Ruta);
        Assert.False(p.Cabeceras.ContainsKey("Trevol-Environment"), p.Ruta);
    }

    // --- La llave ---------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void TvaTestEsTestTvaLiveEsLiveYElTipoSaleDelPrefijo()
    {
        var t = new TrevolClient(AdminTest);
        Assert.Equal(TrevolEnvironment.Test, t.Environment);
        Assert.Equal(TrevolKeyType.Account, t.KeyType);
        Assert.Null(t.Company);
        Assert.Equal(TrevolEnvironment.Live, new TrevolClient(AdminLive).Environment);
        Assert.Equal(TrevolKeyType.Account, new TrevolClient(AdminLive).KeyType);
        Assert.Equal(TrevolKeyType.Api, new TrevolClient(Banco.LlaveTest).KeyType);
        Assert.Equal(TrevolKeyType.Api, new TrevolClient(Banco.LlaveLive).KeyType);
        Assert.Equal(TrevolEnvironment.Live, new TrevolClient("  " + AdminLive + "\n").Environment); // se recorta, como la tvl_
    }

    [Theory]
    [InlineData("tva_test_c4c4")]
    [InlineData("tva_prod_c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4")]
    [InlineData("tva_test_C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4C4")]
    [InlineData("tva_test_c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4")]
    [InlineData("TVA_test_c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4")]
    [InlineData("tvx_test_c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4")]
    [InlineData("tva_c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4c4")]
    public void UnaTvaMalFormadaEsConfigurationExceptionSinLaLlaveEnElMensaje(string mala)
    {
        var e = Assert.Throws<ConfigurationException>(() => new TrevolClient(mala));
        Assert.DoesNotContain(mala, e.Message + e);
        Assert.DoesNotContain("c4c4c4c4", e.Message + e);
        Assert.Contains("tva_test_", e.Message);
        Assert.Contains("tvl_test_", e.Message);
        Assert.Contains("https://platform.trevol.si/llaves y la de administración en https://platform.trevol.si/administracion", e.Message);
    }

    [Fact]
    public void EnvironmentQueNoCoincideConUnaTvaLoDiceConSuPrefijo()
    {
        var e = Assert.Throws<ConfigurationException>(() => new TrevolClient(AdminTest, new TrevolClientOptions { Environment = TrevolEnvironment.Live }));
        Assert.Contains("tva_test_ = habilitación", e.Message);
        Assert.DoesNotContain("c4c4c4c4", e.Message);
        Assert.Equal(TrevolEnvironment.Live, new TrevolClient(AdminLive, new TrevolClientOptions { Environment = TrevolEnvironment.Live }).Environment);
    }

    // --- La empresa: opción del cliente y WithCompany -----------------------------------------------------------------------------------

    [Fact]
    public void CompanyEnLasOpcionesQuedaEnElCliente()
    {
        var t = new TrevolClient(AdminTest, new TrevolClientOptions { Company = " " + Empresa + " " });
        Assert.Equal(Empresa, t.Company);
    }

    [Theory]
    [InlineData("com_3F0C9A1E5B2D4C7F8A6E1D2C3B4A5F60")]
    [InlineData("com_3f0c9a1e5b2d4c7f8a6e1d2c3b4a5f6")]
    [InlineData("3f0c9a1e5b2d4c7f8a6e1d2c3b4a5f60")]
    [InlineData("inv_3f0c9a1e5b2d4c7f8a6e1d2c3b4a5f60")]
    [InlineData("com_3f0c9a1e-5b2d-4c7f-8a6e-1d2c3b4a5f60")]
    [InlineData("")]
    public void UnaEmpresaMalFormadaEsConfigurationExceptionEnLasOpcionesYEnWithCompany(string mala)
    {
        Assert.Throws<ConfigurationException>(() => new TrevolClient(AdminTest, new TrevolClientOptions { Company = mala }));
        Assert.Throws<ConfigurationException>(() => new TrevolClient(AdminTest).WithCompany(mala));
    }

    [Fact]
    public void WithCompanyNuloEsConfigurationException()
    {
        Assert.Throws<ConfigurationException>(() => new TrevolClient(AdminTest).WithCompany(null!));
    }

    [Fact]
    public void CompanyConUnaTvlEsConfigurationExceptionPorqueLaTvlYaEsDeUnaEmpresa()
    {
        var e = Assert.Throws<ConfigurationException>(() => new TrevolClient(Banco.LlaveTest, new TrevolClientOptions { Company = Empresa }));
        Assert.Contains("tva_", e.Message);
        Assert.Contains("tvl_", e.Message);
        Assert.DoesNotContain("a1a1a1a1", e.Message);
        var w = Assert.Throws<ConfigurationException>(() => new TrevolClient(Banco.LlaveLive).WithCompany(Empresa));
        Assert.Equal(e.Message, w.Message);
    }

    [Fact]
    public void WithCompanyDevuelveOtroClienteYElOriginalNoCambia()
    {
        var t = new TrevolClient(AdminLive, new TrevolClientOptions { Company = Empresa, ApiVersion = "2026-10-22" });
        var d = t.WithCompany(OtraEmpresa);
        Assert.NotSame(t, d);
        Assert.Equal(Empresa, t.Company);
        Assert.Equal(OtraEmpresa, d.Company);
        Assert.Equal(TrevolEnvironment.Live, d.Environment);
        Assert.Equal(TrevolKeyType.Account, d.KeyType);
        Assert.Equal("2026-10-22", d.ApiVersion);
        Assert.Equal(OtraEmpresa, d.WithCompany(OtraEmpresa).Company);
    }

    // --- Las cabeceras ------------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ConTvaYEmpresaUnaRutaDeEmpresaLlevaTrevolCompanyYTrevolEnvironmentDelPrefijo()
    {
        var b = new Banco(AdminTest, o => o.Company = Empresa, Banco.Json(200, Banco.Documento));
        await b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        var c = b.Peticiones[0].Cabeceras;
        Assert.Equal("Bearer " + AdminTest, c["Authorization"]);
        Assert.Equal(Empresa, c["Trevol-Company"]);
        Assert.Equal("test", c["Trevol-Environment"]);
        Assert.Equal("2026-11-18", c["Trevol-Version"]);

        var l = new Banco(AdminLive, o => o.Company = Empresa, Banco.Json(200, Banco.Documento));
        await l.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        Assert.Equal("live", l.Peticiones[0].Cabeceras["Trevol-Environment"]);
        Assert.Equal(Empresa, l.Peticiones[0].Cabeceras["Trevol-Company"]);
    }

    [Fact]
    public async Task ConTvaYEmpresaUnPostLlevaLasCabecerasYLaIdempotencyKeyIgualEnCadaReintento()
    {
        var b = new Banco(AdminTest, o => o.Company = Empresa, Banco.Problema(503, "dian_unavailable"), Banco.Json(201, Banco.Documento));
        await b.Trevol.Invoices.CreateInvoiceAsync(TrevolJson.Deserialize<InvoiceInput>(Banco.EntradaFactura));
        Assert.Equal(2, b.Peticiones.Count);
        foreach (var p in b.Peticiones)
        {
            Assert.Equal(Empresa, p.Cabeceras["Trevol-Company"]);
            Assert.Equal("test", p.Cabeceras["Trevol-Environment"]);
        }
        Assert.Equal(b.Peticiones[0].Cabeceras["Idempotency-Key"], b.Peticiones[1].Cabeceras["Idempotency-Key"]);
    }

    [Fact]
    public async Task ConTvaSinEmpresaUnaRutaDeEmpresaEsConfigurationExceptionAntesDeMandarNada()
    {
        var b = new Banco(AdminTest, null, Banco.Json(200, Banco.Documento));
        var e = await Assert.ThrowsAsync<ConfigurationException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Contains("retrieveInvoice", e.Message);
        Assert.Contains("WithCompany", e.Message);
        Assert.Contains("Company", e.Message);
        Assert.DoesNotContain("c4c4c4c4", e.Message + e);
        await Assert.ThrowsAsync<ConfigurationException>(() => b.Trevol.Invoices.CreateInvoiceAsync(TrevolJson.Deserialize<InvoiceInput>(Banco.EntradaFactura)));
        await Assert.ThrowsAsync<ConfigurationException>(() => b.Trevol.General.RetrieveCompanyAsync());
        Assert.Empty(b.Peticiones);
    }

    [Fact]
    public async Task ConTvaLasOperacionesDeAccountNoLlevanLasCabecerasDeEmpresaAunqueHayaEmpresa()
    {
        foreach (var empresa in new[] { null, Empresa })
        {
            var b = new Banco(AdminTest, o => o.Company = empresa, Banco.Json(200, InfoLlave), Banco.Json(200, ListaCompanias(false, null, Empresa)), Banco.Json(200, Compania));
            var info = await b.Trevol.Account.RetrieveAccountAsync();
            await b.Trevol.Account.ListAccountCompaniesAsync(limit: 10);
            await b.Trevol.Account.RetrieveAccountCompanyAsync(Empresa);
            Assert.Equal("account_key_info", info.Object);
            Assert.Equal(new[] { "/v1/account", "/v1/account/companies?limit=10", $"/v1/account/companies/{Empresa}" }, b.Peticiones.Select(p => p.Ruta).ToArray());
            foreach (var p in b.Peticiones)
            {
                SinCabecerasDeEmpresa(p);
                Assert.Equal("Bearer " + AdminTest, p.Cabeceras["Authorization"]);
            }
        }
    }

    [Fact]
    public async Task ConTvaLaSaludNoLlevaLasCabecerasDeEmpresa()
    {
        var b = new Banco(AdminLive, null, Banco.Json(200, "{\"status\":\"ok\"}"));
        await b.Trevol.General.GetHealthAsync();
        SinCabecerasDeEmpresa(b.Peticiones[0]);
    }

    [Fact]
    public async Task UnaTvlNuncaMandaLasCabecerasDeEmpresaNiEnLasRutasDeEmpresaNiEnAccount()
    {
        var b = new Banco(Banco.LlaveTest, null, Banco.Json(200, Banco.Documento));
        await b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        Assert.Single(b.Peticiones);
        SinCabecerasDeEmpresa(b.Peticiones[0]);
    }

    [Fact]
    public async Task UnaTvlEnAccountEsConfigurationExceptionAntesDeLaRed()
    {
        // Las diez operaciones de /v1/account solo admiten una tva_: con una tvl_ la API contestaría 401 de seguro, así que no salen.
        var b = new Banco(Banco.LlaveLive, null);
        var llamadas = new Func<Task>[]
        {
            () => b.Trevol.Account.RetrieveAccountAsync(),
            () => b.Trevol.Account.ListAccountCompaniesAsync(),
            () => b.Trevol.Account.CreateAccountCompanyAsync(new AccountCompanyInput(nit: "900373427")),
            () => b.Trevol.Account.RetrieveAccountCompanyAsync(Empresa),
            () => b.Trevol.Account.ListAccountCompanyApiKeysAsync(Empresa),
            () => b.Trevol.Account.CreateAccountCompanyApiKeyAsync(Empresa, new AccountApiKeyInput(name: "Facturación", permissions: AccountApiKeyInput.PermissionsEnum.Read)),
            () => b.Trevol.Account.RevokeAccountCompanyApiKeyAsync(Empresa, "key_0a1b2c3d4e5f60718293a4b5c6d7e8f9"),
            () => b.Trevol.Account.ListAccountDianDocumentsAsync(),
            () => b.Trevol.Account.ListAccountRadianEventsAsync(),
            () => b.Trevol.Account.ListAccountRequestsAsync(),
        };
        foreach (var llamar in llamadas)
        {
            var e = await Assert.ThrowsAsync<ConfigurationException>(llamar);
            Assert.Contains("tva_test_", e.Message);
            Assert.Contains("tvl_", e.Message);
            Assert.DoesNotContain("b2b2b2b2", e.Message + e);
        }
        var r = await Assert.ThrowsAsync<ConfigurationException>(async () =>
        {
            await foreach (var _ in b.Trevol.PaginateAsync(c => b.Trevol.Account.ListAccountCompaniesAsync(startingAfter: c))) { }
        });
        Assert.Contains("listAccountCompanies", r.Message);
        Assert.Empty(b.Peticiones);
    }

    [Fact]
    public async Task UnaTvaLlamandoConUnaLlaveRevocadaDaElErrorTipadoSinLaLlave()
    {
        var b = new Banco(AdminLive, o => o.Company = Empresa, Banco.Problema(401, "invalid_api_key"));
        var e = await Assert.ThrowsAsync<AuthenticationException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        var todo = e.Message + e + string.Join(",", e.Headers.SelectMany(h => h.Value)) + e.Problem;
        Assert.DoesNotContain("d5d5d5d5", todo);
    }

    [Fact]
    public async Task LosErroresDeLaLlaveDeAdministracionLleganTipados()
    {
        var existente = Banco.Problema(409, "company_exists", "{\"company\":" + Compania + "}");
        var b = new Banco(AdminLive, o => o.MaxRetries = 0, existente);
        var c = await Assert.ThrowsAsync<ConflictException>(() => b.Trevol.Account.CreateAccountCompanyAsync(new AccountCompanyInput(nit: "900373427")));
        Assert.Equal("company_exists", c.Code);
        Assert.Equal(Empresa, ((Dictionary<string, object?>)c.Problem!["company"]!)["id"]);

        var tope = Banco.Problema(429, "company_limit_reached", "{\"retryable\":true,\"retry_after\":3600,\"limit\":\"hour\"}", new() { ["Retry-After"] = "3600" });
        var r = new Banco(AdminLive, null, tope);
        var l = await Assert.ThrowsAsync<RateLimitException>(() => r.Trevol.Account.CreateAccountCompanyAsync(new AccountCompanyInput(nit: "900373427"), "ALTA-900373427"));
        Assert.Equal(3600, l.RetryAfter);
        Assert.Equal("ALTA-900373427", l.IdempotencyKey);
        Assert.Single(r.Peticiones); // pide esperar más que MaxRetryAfter: no espera, lanza

        var m = new Banco(AdminTest, o => { o.Company = Empresa; o.MaxRetries = 0; }, Banco.Problema(403, "permission_denied", "{\"reason\":\"environment_mismatch\"}"));
        var p = await Assert.ThrowsAsync<PermissionDeniedException>(() => m.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Equal("environment_mismatch", (string)p.Problem!["reason"]!);
    }

    [Fact]
    public async Task CreateAccountCompanyLlevaIdempotencyKeyAutomaticaLaMismaEnLosReintentosYSinCabecerasDeEmpresa()
    {
        var b = new Banco(AdminLive, o => o.Company = Empresa, Banco.Problema(500, "internal_error"), Banco.Json(201, Compania.Replace("\"enabled\"", "\"pending\"")));
        var c = await b.Trevol.Account.CreateAccountCompanyAsync(new AccountCompanyInput(nit: "900373427", name: "DISTRIBUIDORA LOS ANDES SAS"));
        Assert.Equal("pending", c.Status);
        Assert.Equal(2, b.Peticiones.Count);
        Assert.Equal("00000000-0000-4000-8000-000000000001", b.Peticiones[0].Cabeceras["Idempotency-Key"]);
        Assert.Equal(b.Peticiones[0].Cabeceras["Idempotency-Key"], b.Peticiones[1].Cabeceras["Idempotency-Key"]);
        Assert.Contains("\"nit\":\"900373427\"", b.Peticiones[0].Cuerpo);
        foreach (var p in b.Peticiones) SinCabecerasDeEmpresa(p);
    }

    /// <summary>Al repetirla con la misma Idempotency-Key la API devuelve la llave con token null: un reintento a ciegas perdería el token.</summary>
    [Fact]
    public async Task CrearUnaLlaveDeEmpresaNoSeReintentaAnteUn5xxNiUnCorteYSiAnteUn429()
    {
        var op = Tablas.Operaciones.Single(o => o.OperationId == "createAccountCompanyApiKey");
        Assert.True(op.Idempotente);
        Assert.Equal("solo_429", op.Reintento);
        var entrada = new AccountApiKeyInput(name: "ERP de la bodega", permissions: AccountApiKeyInput.PermissionsEnum.Write);

        var a = new Banco(AdminTest, null, Banco.Problema(503, "dian_unavailable", "{\"retryable\":true}"));
        var e = await Assert.ThrowsAsync<DianUnavailableException>(() => a.Trevol.Account.CreateAccountCompanyApiKeyAsync(Empresa, entrada));
        Assert.Equal("00000000-0000-4000-8000-000000000001", e.IdempotencyKey);
        Assert.Single(a.Peticiones);
        Assert.Empty(a.Esperas);

        var b = new Banco(AdminTest, null, Banco.Cortar());
        await Assert.ThrowsAsync<ApiConnectionException>(() => b.Trevol.Account.CreateAccountCompanyApiKeyAsync(Empresa, entrada));
        Assert.Single(b.Peticiones);

        var llave = "{\"id\":\"key_4b6d8f0a2c4e4a8b9d3f5b7d9e1a3c5f\",\"object\":\"api_key\",\"token\":\"tvl_test_" + string.Concat(Enumerable.Repeat("e7", 32)) + "\"}";
        var c = new Banco(AdminTest, null, Banco.Problema(429, "rate_limited", "{\"retryable\":true,\"limit\":\"api_keys\"}", new() { ["Retry-After"] = "2" }),
            Banco.Json(201, llave));
        var k = await c.Trevol.Account.CreateAccountCompanyApiKeyAsync(Empresa, entrada);
        Assert.StartsWith("tvl_test_", k.Token);
        Assert.Equal(2, c.Peticiones.Count);
        Assert.Equal(new[] { 2.0 }, c.Esperas);
        Assert.Equal(c.Peticiones[0].Cabeceras["Idempotency-Key"], c.Peticiones[1].Cabeceras["Idempotency-Key"]);
    }

    [Fact]
    public async Task RevocarUnaLlaveDeEmpresaSoloSeReintentaCon429()
    {
        var b = new Banco(AdminTest, null, Banco.Problema(503, "dian_unavailable"));
        await Assert.ThrowsAnyAsync<TrevolException>(() => b.Trevol.Account.RevokeAccountCompanyApiKeyAsync(Empresa, "key_4b6d8f0a2c4e4a8b9d3f5b7d9e1a3c5f"));
        Assert.Single(b.Peticiones);
        Assert.Equal("DELETE", b.Peticiones[0].Metodo);
        Assert.False(b.Peticiones[0].Cabeceras.ContainsKey("Idempotency-Key"));
    }

    // --- WithCompany comparte el transporte -------------------------------------------------------------------------------------------

    [Fact]
    public async Task WithCompanyCompartePorElMismoTransporteYCadaClienteMandaSuEmpresa()
    {
        var b = new Banco(AdminLive, o => o.Company = Empresa, Banco.Json(200, Banco.Documento));
        var otra = b.Trevol.WithCompany(OtraEmpresa);
        await Task.WhenAll(
            b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc),
            otra.Invoices.RetrieveInvoiceAsync(Banco.IdDoc),
            b.Trevol.WithCompany(OtraEmpresa).Invoices.RetrieveInvoiceAsync(Banco.IdDoc));
        Assert.Equal(3, b.Peticiones.Count); // el mismo manejador (la misma red falsa): el transporte es el del cliente original
        Assert.Equal(new[] { Empresa, OtraEmpresa, OtraEmpresa }, b.Peticiones.Select(p => p.Cabeceras["Trevol-Company"]).OrderBy(x => x == Empresa ? 0 : 1).ToArray());
        Assert.All(b.Peticiones, p => Assert.Equal("live", p.Cabeceras["Trevol-Environment"]));
        Assert.All(b.Peticiones, p => Assert.Equal("Bearer " + AdminLive, p.Cabeceras["Authorization"]));
    }

    [Fact]
    public async Task WithCompanyDesdeUnClienteSinEmpresaYCerrarElDerivadoNoCierraElOriginal()
    {
        var b = new Banco(AdminTest, null, Banco.Json(200, Banco.Documento));
        using (var d = b.Trevol.WithCompany(Empresa))
        {
            await d.Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        }
        await b.Trevol.WithCompany(OtraEmpresa).Invoices.RetrieveInvoiceAsync(Banco.IdDoc);
        await Assert.ThrowsAsync<ConfigurationException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync(Banco.IdDoc)); // el original sigue sin empresa
        Assert.Equal(new[] { Empresa, OtraEmpresa }, b.Peticiones.Select(p => p.Cabeceras["Trevol-Company"]).ToArray());
    }

    // --- Paginación del recurso Account -------------------------------------------------------------------------------------------------

    [Fact]
    public async Task PaginateAsyncRecorreLasEmpresasDeLaCuentaConElCursor()
    {
        var b = new Banco(AdminTest, null,
            Banco.Json(200, ListaCompanias(true, OtraEmpresa, Empresa, OtraEmpresa)),
            Banco.Json(200, ListaCompanias(false, null, "com_" + new string('1', 32))));
        var ids = new List<string>();
        await foreach (var c in b.Trevol.PaginateAsync(cursor => b.Trevol.Account.ListAccountCompaniesAsync(limit: 2, startingAfter: cursor)))
            ids.Add(c.Id);
        Assert.Equal(new[] { Empresa, OtraEmpresa, "com_" + new string('1', 32) }, ids.ToArray());
        Assert.Equal("/v1/account/companies?limit=2", b.Peticiones[0].Ruta);
        Assert.Equal($"/v1/account/companies?limit=2&starting_after={OtraEmpresa}", b.Peticiones[1].Ruta);
        foreach (var p in b.Peticiones) SinCabecerasDeEmpresa(p);
    }

    [Fact]
    public async Task PaginateAsyncRecorreElRegistroDeLaCuentaUsandoElIdSiNoHayNextCursor()
    {
        const string fila = "{\"object\":\"request\",\"id\":\"ID\",\"method\":\"GET\",\"path\":\"/v1/invoices\",\"status\":200,\"code\":null,\"duration_ms\":12,\"company\":null,\"ip\":\"203.0.113.10\",\"idempotent_replayed\":false,\"created_at\":\"2026-11-18T10:02:11-05:00\"}";
        var b = new Banco(AdminTest, null,
            Banco.Json(200, "{\"object\":\"list\",\"has_more\":true,\"next_cursor\":null,\"data\":[" + fila.Replace("ID", "req_8e2f4a6c8e2f4a6c8e2f4a6c") + "]}"),
            Banco.Json(200, "{\"object\":\"list\",\"has_more\":false,\"next_cursor\":null,\"data\":[" + fila.Replace("ID", "req_5b1c5b1c5b1c5b1c5b1c5b1c") + "]}"));
        var ids = new List<string>();
        await foreach (var r in b.Trevol.PaginateAsync(c => b.Trevol.Account.ListAccountRequestsAsync(startingAfter: c)))
            ids.Add(r.Id);
        Assert.Equal(new[] { "req_8e2f4a6c8e2f4a6c8e2f4a6c", "req_5b1c5b1c5b1c5b1c5b1c5b1c" }, ids.ToArray());
        Assert.Equal("/v1/account/requests?starting_after=req_8e2f4a6c8e2f4a6c8e2f4a6c", b.Peticiones[1].Ruta);
    }

    [Fact]
    public void LasCincoListasDeAccountTienenSuSobrecargaDePaginateAsync()
    {
        foreach (var l in new[] { "AccountCompanyList", "AccountApiKeyList", "AccountDianDocumentList", "AccountRadianEventList", "AccountRequestLogList" })
            Assert.True(typeof(TrevolClient).GetMethods().Any(m => m.Name == "PaginateAsync" && m.GetParameters()[0].ParameterType.ToString().Contains($"{l}]")), l);
    }

    // --- La tabla y los datos compartidos ----------------------------------------------------------------------------------------------

    [Fact]
    public void LaTablaDiceQueLlaveAdmiteCadaOperacionComoLosDatosCompartidos()
    {
        var datos = JArray.Parse(Banco.Datos("operaciones.json")).ToDictionary(o => (string)o["operationId"]!, o => (string)o["llave"]!);
        Assert.Equal(datos.Count, Tablas.Operaciones.Count);
        foreach (var o in Tablas.Operaciones)
        {
            Assert.Equal(datos[o.OperationId], o.Llave);
            Assert.Equal(o.Recurso == "Account" ? "administracion" : o.OperationId == "getHealth" ? "ninguna" : "api_o_administracion", o.Llave);
        }
        Assert.Equal(10, Tablas.Operaciones.Count(o => o.Llave == "administracion"));
    }

    // --- Nada muestra la llave ----------------------------------------------------------------------------------------------------------

    [Fact]
    public void NiElClienteNiElDerivadoNiElTransporteMuestranLaLlave()
    {
        var t = new TrevolClient(AdminLive, new TrevolClientOptions { Company = Empresa });
        var d = t.WithCompany(OtraEmpresa);
        foreach (var texto in new[] { t.ToString(), d.ToString() })
        {
            Assert.DoesNotContain("d5d5d5d5", texto);
            Assert.DoesNotContain("tva_live_", texto);
            Assert.Contains("keyType=Account", texto);
        }
        Assert.Contains(Empresa, t.ToString());
        Assert.Contains(OtraEmpresa, d.ToString());
        var transporte = typeof(TrevolClient).GetField("_transporte", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(t)!;
        Assert.DoesNotContain("d5d5d5d5", transporte.ToString());
        Assert.Contains("keyType=Account", transporte.ToString());
    }
}
