# Registro de cambios

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el versionado, [SemVer](https://semver.org/lang/es/): antes
de la 1.0, un cambio incompatible sube el número menor.

## [0.1.0] — sin publicar

Primera versión (API `2026-10-22`).

- `TrevolClient` con una propiedad por etiqueta de la API (`Invoices`, `CreditNotes`, `DebitNotes`, `SupportDocuments`, `Payrolls`,
  `RadianEvents`, `WebhookEndpoints`, `Events`, `Requests`, `Balance`, `General`…) y los modelos y clientes generados de la especificación
  (`Trevol.Generado`). Destinos `netstandard2.0` y `net8.0`.
- `Idempotency-Key` automática (la misma en los reintentos), reintentos con espera creciente que respetan `Retry-After`, excepciones tipadas por
  `code` (`Trevol.*Exception`), entorno deducido del prefijo de la llave, `Trevol-Version` fijada.
- `PaginateAsync` (`IAsyncEnumerable<T>`), descargas de PDF, XML y ZIP como `Stream`, `Webhook.Verify` y `Webhook.EventData` (Standard Webhooks,
  verificación propia).
