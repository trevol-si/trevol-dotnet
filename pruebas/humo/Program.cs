// Prueba de humo REAL en habilitación (docs/api/43 §2.10.4). NO es parte de la suite: la corre una persona, con una llave de PRUEBAS.
//
//   TREVOL_API_KEY=tvl_test_… dotnet run --project pruebas/humo
//
// Lecturas, un preview y un error tipado (validation_error); con TREVOL_HUMO_EMITIR=1 además EMITE una factura en habilitación, espera a que
// quede final y descarga el PDF. Se NIEGA a correr si la llave no es tvl_test_. TREVOL_BASE_URL (opcional) apunta a otro
// servidor, p. ej. un Prism local.
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Trevol;
using Trevol.Generado.Model;

var llave = Environment.GetEnvironmentVariable("TREVOL_API_KEY") ?? "";
if (!Regex.IsMatch(llave, "^tvl_test_[0-9a-f]{64}$"))
{
    Console.Error.WriteLine("humo: me niego a correr si TREVOL_API_KEY no es una llave de PRUEBAS (tvl_test_…). Nunca se prueba con tvl_live_.");
    return 2;
}
var opciones = new TrevolClientOptions { Environment = TrevolEnvironment.Test };
if (Environment.GetEnvironmentVariable("TREVOL_BASE_URL") is { Length: > 0 } baseUrl) opciones.BaseUrl = baseUrl;
using var trevol = new TrevolClient(llave, opciones);

var total = 0;
var fallas = 0;
async Task Paso(string nombre, Func<Task<string>> f)
{
    total++;
    try
    {
        var detalle = await f();
        Console.WriteLine($"ok   {nombre}{(detalle.Length > 0 ? " — " + detalle : "")}");
    }
    catch (Exception e) // es una prueba de humo: cuenta cualquier fallo
    {
        fallas++;
        Console.WriteLine($"FALLA {nombre} — {e.GetType().Name}: {e.Message}");
    }
}

await Paso("GetHealthAsync", async () => { await trevol.General.GetHealthAsync(); return ""; });
await Paso("RetrieveCompanyAsync", async () =>
{
    var c = await trevol.General.RetrieveCompanyAsync();
    if (c.VarEnvironment != "test") throw new InvalidOperationException($"la empresa dice environment={c.VarEnvironment}"); // (el generador llama VarEnvironment a la propiedad «environment»)
    return $"{c.Name} ({c.Nit})";
});
InvoiceInput Factura(string externo, bool conLineas) => TrevolJson.Deserialize<InvoiceInput>(
    "{\"external_id\": \"" + externo + "\", \"customer\": {\"final_consumer\": true}, \"items\": "
    + (conLineas ? """[{"description": "Prueba de humo del SDK de Trevol", "quantity": "1", "unit_price": "1000", "taxes": [{"code": "01", "rate": "19"}]}]""" : "[]")
    + """, "payment": {"form": "cash", "methods": [{"code": "10"}]}}""")!;

await Paso("PreviewInvoiceAsync", async () =>
{
    var p = await trevol.Invoices.PreviewInvoiceAsync(Factura("HUMO-PREVIEW", true));
    return "total " + (p.Totals != null && p.Totals.TryGetValue("total", out var total1) ? total1 : "?");
});
await Paso("PreviewInvoiceAsync sin líneas → ValidationException (code validation_error)", async () =>
{
    try
    {
        await trevol.Invoices.PreviewInvoiceAsync(Factura("HUMO-INVALIDA", false));
    }
    catch (ValidationException e)
    {
        if (e.Code != "validation_error") throw new InvalidOperationException($"code = {e.Code}");
        return $"{e.Status} {e.Code} {(e.Errors.Count > 0 ? e.Errors[0].Pointer : "")}";
    }
    throw new InvalidOperationException("no lanzó ValidationException");
});
await Paso("ListInvoicesAsync + PaginateAsync (3)", async () =>
{
    await trevol.Invoices.ListInvoicesAsync(limit: 3);
    var n = 0;
    await foreach (var _ in trevol.PaginateAsync(c => trevol.Invoices.ListInvoicesAsync(limit: 3, startingAfter: c)))
        if (++n == 3) break;
    return $"{n} facturas";
});
await Paso("RetrieveInvoiceAsync inexistente → NotFoundException con request_id", async () =>
{
    try
    {
        await trevol.Invoices.RetrieveInvoiceAsync("inv_" + new string('0', 32));
    }
    catch (NotFoundException e)
    {
        if (e.RequestId?.StartsWith("req_") != true) throw new InvalidOperationException("sin request_id");
        return e.RequestId;
    }
    throw new InvalidOperationException("no lanzó NotFoundException");
});
await Paso("Webhook.Verify (firmado ahora, con un secreto local)", () =>
{
    var clave = Encoding.UTF8.GetBytes("trevol-humo-secreto-de-32-bytes!");
    var id = "evt_" + new string('1', 32);
    var cuerpo = $"{{\"id\":\"{id}\",\"object\":\"event\",\"type\":\"webhook.test\",\"timestamp\":\"{DateTime.UtcNow:O}\",\"environment\":\"test\",\"api_version\":\"2026-10-22\",\"data\":{{}}}}";
    var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    using var hmac = new HMACSHA256(clave);
    var firma = "v1," + Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{id}.{ts}.{cuerpo}")));
    var e = new Webhook("whsec_" + Convert.ToBase64String(clave)).Verify(cuerpo, new Dictionary<string, string> { ["webhook-id"] = id, ["webhook-timestamp"] = ts.ToString(), ["webhook-signature"] = firma });
    return Task.FromResult(e.Type ?? "");
});

if (Environment.GetEnvironmentVariable("TREVOL_HUMO_EMITIR") == "1")
{
    string? id = null;
    await Paso("CreateInvoiceAsync (EMITE en habilitación)", async () =>
    {
        var d = await trevol.Invoices.CreateInvoiceAsync(invoiceInput: Factura($"HUMO-CS-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}", true));
        id = d.Id;
        return $"{d.Id} {d.Status}";
    });
    if (id != null)
    {
        await Paso("espera un estado final (hasta 90 s)", async () =>
        {
            var limite = DateTime.UtcNow.AddSeconds(90);
            for (;;)
            {
                var estado = (await trevol.Invoices.RetrieveInvoiceAsync(id)).Status;
                if (estado is not ("pending" or "sending" or "processing"))
                {
                    if (estado != "accepted") throw new InvalidOperationException($"status = {estado}");
                    return estado;
                }
                if (DateTime.UtcNow > limite) throw new InvalidOperationException($"status = {estado}");
                await Task.Delay(3000);
            }
        });
        await Paso("DownloadInvoicePdfAsync", async () =>
        {
            var archivo = await trevol.Invoices.DownloadInvoicePdfAsync(id: id);
            using var memoria = new MemoryStream();
            await archivo.Content.CopyToAsync(memoria);
            var bytes = memoria.ToArray();
            if (bytes.Length < 4 || Encoding.ASCII.GetString(bytes, 0, 4) != "%PDF") throw new InvalidOperationException("no es un PDF");
            return $"PDF {bytes.Length} bytes";
        });
    }
}
else
{
    Console.WriteLine("(no se emite nada: define TREVOL_HUMO_EMITIR=1 para emitir una factura en habilitación)");
}

Console.WriteLine($"\n{total - fallas}/{total} pasos bien");
return fallas > 0 ? 1 : 0;
