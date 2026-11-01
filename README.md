# Trevol (.NET)

SDK de .NET para la **API de Trevol**: facturación electrónica, documento soporte, documento equivalente, nómina electrónica y eventos
RADIAN ante la DIAN (Colombia). Documentación: <https://platform.trevol.si/docs>.

> **In English.** .NET SDK for the Trevol API (Colombian e-invoicing, DIAN). Targets `netstandard2.0` and `net8.0`. Typed models and API
> clients generated from the OpenAPI spec (OpenAPI Generator `csharp` / `httpclient`, namespace `Trevol.Generado`), plus a thin hand-written layer
> (namespace `Trevol`, client `TrevolClient`): automatic `Idempotency-Key` (the same one on retries), retries with exponential backoff
> honouring `Retry-After`, typed exceptions by problem+json `code`, environment inferred from the key prefix (`tvl_test_` = DIAN test
> environment, `tvl_live_` = production), pinned `Trevol-Version`, Standard Webhooks signature verification, cursor pagination with
> `IAsyncEnumerable<T>` and PDF/XML/ZIP downloads. Docs are in Spanish like the API.

**Versión 0.1.0** — API `2026-10-22`. Licencia MIT.

## Instalación

```sh
dotnet add package Trevol
```

Funciona en .NET 8 y en cualquier plataforma compatible con `netstandard2.0` (.NET Framework 4.6.2+, .NET Core 2.0+, Mono, Xamarin). Depende de
Newtonsoft.Json, Polly y JsonSubTypes (los usa el código generado) y, en `netstandard2.0`, de `Microsoft.Bcl.AsyncInterfaces`.

## Uso

La llave de la API se crea en la consola de desarrolladores (<https://platform.trevol.si/llaves>) y se guarda en el entorno de tu **servidor**:
`TREVOL_API_KEY`. Una llave `tvl_test_…` llama a la habilitación de la DIAN; una `tvl_live_…`, a producción. El SDK deduce el entorno del
prefijo (`trevol.Environment`). La clase se llama `TrevolClient` y no `Trevol` porque el espacio de nombres ya es `Trevol`.

```csharp
using Trevol;
using Trevol.Generado.Model;

var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY")); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

try
{
    var entrada = TrevolJson.Deserialize<InvoiceInput>("""
        {"external_id": "VENTA-2026-000123", "customer": {"final_consumer": true},
         "items": [{"description": "Café especial 500 g", "quantity": "1", "unit_price": "32000", "taxes": [{"code": "01", "rate": "19"}]}],
         "payment": {"form": "cash", "methods": [{"code": "10"}]}}
        """);
    var factura = await trevol.Invoices.CreateInvoiceAsync(
        invoiceInput: entrada,
        idempotencyKey: "VENTA-2026-000123"); // opcional: si falta, el SDK pone una y la repite en los reintentos
    if (factura.Status == "processing")
    {
        // 202: la DIAN no contestó; Trevol lo termina y avisa por webhook. NO crees otro documento para la misma venta.
    }
}
catch (ValidationException e)
{
    foreach (var x in e.Errors) Console.Error.WriteLine($"{x.Pointer} {x.Message}");
}
catch (TrevolException e)
{
    Console.Error.WriteLine($"{e.Code} {e.RequestId}");
}

// Todas las páginas de un listado, perezosamente (IAsyncEnumerable<T>)
await foreach (var f in trevol.PaginateAsync(cursor => trevol.Invoices.ListInvoicesAsync(limit: 100, startingAfter: cursor)))
    Console.WriteLine($"{f.Id} {f.Status}");

// Los archivos llegan como un Stream dentro de un FileParameter
var pdf = await trevol.Invoices.DownloadInvoicePdfAsync(id: "inv_…");
await using var salida = File.Create("factura.pdf");
await pdf.Content.CopyToAsync(salida);
```

Los nombres de los métodos son los del `operationId` de la [especificación](https://platform.trevol.si/docs/api) con el sufijo `Async`
(`CreateInvoiceAsync`, `ListInvoicesAsync`, `RetrieveInvoiceAsync`, `CorrectInvoiceAsync`, `SendInvoiceAsync`, `DownloadInvoicePdfAsync`…); cada
uno tiene también su versión síncrona. **Usa argumentos con nombre**: el orden posicional de los parámetros lo decide el generador y puede cambiar
si la API suma un filtro opcional; los nombres no. El cuerpo es el modelo generado (`InvoiceInput`) o, lo más cómodo, desde JSON con
`TrevolJson.Deserialize<InvoiceInput>(texto)` (los campos se llaman igual que en la API: `external_id`); los importes y las fechas viajan como texto
(`"32000.50"`, `"2026-10-31"`). (Los literales `"""` de texto sin procesar necesitan C# 11 en *tu* código; el SDK compila con cualquier versión.)

## Lo que conviene saber

### Idempotencia: manda `external_id` y tu propia clave

El SDK genera una `Idempotency-Key` por llamada y **la repite en todos los reintentos de esa llamada**. Pero si tu proceso muere y vuelves a
llamar, saldrá otra clave y, si no mandaste `external_id` (es opcional), podría duplicarse el documento. Por eso:

- manda siempre `external_id` (tu id de la venta), y
- pasa una `idempotencyKey` derivada de tu venta (`idempotencyKey: "VENTA-2026-000123"`).

Ante una `ConflictException` con `Code == "external_id_in_use"`, el documento ya existe: usa `e.Problem["existing_id"]`. Toda excepción lleva la
clave que se usó (`IdempotencyKey`) para reintentar más tarde con la misma. Una `ApiConnectionException` también.

### Respuestas 202 (`processing`, `sending`, `validating`, `contingency`)

No son errores ni se reintentan: se devuelven como el modelo de la respuesta. **No crees otro documento**: espera el webhook o consulta con
`RetrieveInvoiceAsync`; repetir la misma llamada con la misma `Idempotency-Key` continúa el mismo documento. El PDF y el ZIP llegan después del
`201` (evento `invoice.delivered`, o `delivery.status == "completed"`).

### Reintentos

Hasta 2 reintentos (3 intentos) con espera creciente (0,5 s, 1 s, 2 s… con tope de 8 s y un poco de azar) ante un error de red, 429, 500, 502,
503, 504, un 409 `request_in_progress` o `previous_event_pending`, y todo problema que diga `retryable: true` (salvo el 402). Respeta
`Retry-After`; si pide más de `MaxRetryAfter` (60 s) no espera y lanza la excepción con `RetryAfter`. Las operaciones sin `Idempotency-Key`
(crear un endpoint, `…/send`, `DELETE`) solo se reintentan con 429. Los 202 y los errores finales (400, 401, 402, 403, 404, 422 y los 409
`invalid_state`, `external_id_in_use`, `duplicate_document`) **nunca** se reintentan. El tiempo máximo es por intento (`Timeout`, 65 s), no
por llamada: el `HttpClient` interno no tiene `Timeout`. Para cancelar toda la llamada pasa un `CancellationToken`.

### La versión de la API

El SDK fija `Trevol-Version: 2026-10-22` en cada petición. **Ojo:** una llave creada con la versión `2026-10-21` que empiece a usar este SDK
pasa a `2026-10-22` (el encabezado manda): recibe `202 processing` donde antes recibía `503`. Si todavía no quieres ese cambio:
`new TrevolClient(llave, new TrevolClientOptions { ApiVersion = "2026-10-21" })`. Más en <https://platform.trevol.si/docs/versiones>.

### Excepciones tipadas

`TrevolException` es la base; las clases salen del `code` del problem+json: `AuthenticationException`, `PermissionDeniedException`,
`NotFoundException`, `InvalidRequestException`, `IdempotencyException`, `ValidationException` (con `Errors`: `Field`, `Pointer`, `Message`),
`ConflictException`, `PreconditionException`, `DianRejectedException` (con `Problem["document"]` y `Problem["rules"]`), `BalanceException`,
`RateLimitException`, `DianUnavailableException` e `InternalServerException`. Un `code` que el SDK todavía no conoce llega como `TrevolException`
con ese `Code`: nunca rompe. Además: `ApiConnectionException` (no hubo respuesta), `ConfigurationException` (mal uso, antes de llamar) y
`WebhookVerificationException` (`Reason`: firma, fecha, cabeceras o cuerpo). (`Trevol.ValidationException` no es la de
`System.ComponentModel.DataAnnotations`: si importas ambos espacios de nombres, califícala.) Cada excepción tiene `Status`, `Code`, `Type`,
`Title`, `Detail`, `RequestId`, `Retryable`, `RetryAfter`, `Errors`, `IdempotencyKey`, `Problem` (el cuerpo completo) y `Headers` (los de la
respuesta). Ninguna incluye tu llave.

### Webhooks

```csharp
var wh = new Webhook(Environment.GetEnvironmentVariable("TREVOL_WEBHOOK_SECRET")!); // whsec_…

app.MapPost("/webhooks/trevol", async (HttpRequest request) =>
{
    using var lector = new StreamReader(request.Body);
    var cuerpo = await lector.ReadToEndAsync(); // el cuerpo CRUDO, sin [FromBody]
    try
    {
        var evento = wh.Verify(cuerpo, request.Headers); // IHeaderDictionary, HttpRequestHeaders o un diccionario
        // Deduplica por evento.Id (también llega en la cabecera webhook-id): la verificación no evita repeticiones.
        var datos = wh.EventData(evento); // el modelo según evento.Type (p. ej. DocumentEventData); null si el tipo es nuevo
        return Results.Ok();
    }
    catch (WebhookVerificationException)
    {
        return Results.BadRequest();
    }
});
```

La firma se calcula sobre los **bytes exactos**. `Verify` acepta el cuerpo como `string` o `byte[]` y las cabeceras en cualquier caja; tolera 5
minutos hacia atrás y hacia adelante en `webhook-timestamp` y compara en tiempo constante. La verificación es **propia** (≈ 40 líneas, los mismos
vectores que los demás SDK): la única biblioteca de Standard Webhooks en NuGet es de un tercero, sin licencia SPDX clara, sin `netstandard2.0`,
ligada a ASP.NET Core y sin cambios desde 2025.

### Estado y cabeceras de la respuesta

Cada operación tiene `…WithHttpInfoAsync` que devuelve `ApiResponse<T>` con `StatusCode`, `Headers` y `Data`:
`var r = await trevol.Invoices.CreateInvoiceWithHttpInfoAsync(entrada); r.Headers["Request-Id"]; r.Headers["Idempotent-Replayed"]; r.Data`.

## Opciones

```csharp
new TrevolClient(llave, new TrevolClientOptions
{
    ApiVersion = "2026-10-22",               // Trevol-Version en cada petición
    MaxRetries = 2,                          // 0 los apaga
    Timeout = TimeSpan.FromSeconds(65),      // por intento
    MaxRetryAfter = TimeSpan.FromSeconds(60),
    BaseUrl = "https://api.trevol.si/v1",
    Environment = TrevolEnvironment.Test,    // si la llave no coincide: ConfigurationException antes de llamar
    Handler = miHandler,                     // proxy, TLS, métricas, pruebas: el SDK lo envuelve con su Transporte
});
```

`TrevolClient` es `IDisposable` (cierra su `HttpClient`): en ASP.NET regístralo como `singleton`.

## Cómo está hecho

- `src/Trevol/Generado/` lo escribe [OpenAPI Generator](https://openapi-generator.tech) 7.26.0 (`csharp`, `httpclient`) desde la especificación
  pública y **no se edita** (ver `GENERADO.md`). Para que un valor nuevo de un `enum` en lo que la API devuelve no rompa a quien ya integró,
  los modelos de respuesta se generan sin `enum`.
- Lo propio (`src/Trevol/*.cs`) vive fuera y solo usa el punto de extensión del código generado: un `DelegatingHandler` (`Transporte`) dentro
  del `HttpClient`, que lanza la excepción tipada y reintenta. Dos ajustes del serializador del generador, por piloto: las propiedades sin valor
  se omiten (no `"rounding": null`) y las listas de fechas salen como `yyyy-MM-dd`.
- Las pruebas: `dotnet test pruebas/Trevol.Pruebas` (unitarias sin red, con los mismos casos de los demás SDK en `pruebas/datos/`; corren en
  `net8.0`, y `netstandard2.0` se compila); `TREVOL_PRISM_URL=http://127.0.0.1:4010 dotnet test pruebas/Trevol.Pruebas` suma las de un servidor
  simulado (`npx @stoplight/prism-cli mock pruebas/datos/openapi.json -p 4010`) que valida cada petición contra la especificación, y que además
  corre como programa cada fragmento de código de la documentación (se compilan siempre).
- `dotnet run --project pruebas/humo` es la prueba de humo **real** (solo con una llave `tvl_test_`; no emite documentos).

## Licencia

MIT © 2026 Trevol.
