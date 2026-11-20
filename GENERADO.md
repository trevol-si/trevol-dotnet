# Código generado

**No edites `src/Trevol/Generado/`**: `sdks/generar.sh` la borra y la vuelve a escribir entera. La capa propia (cliente,
reintentos, errores, paginación, webhooks) vive fuera de esa carpeta.

| | |
|---|---|
| Generador | OpenAPI Generator 7.26.0 (`csharp`) |
| Especificación | `especificacion/openapi.sdk.enums-abiertos.json` (variante 3.0.3 de `docs/api/openapi.json`, versión de la API 2026-11-18) |
| sha256 de la especificación | `5aa068bcbfb2ef88eb20653c9dd34df80d56bd89adcc47efa83cb1be0baa131f` |
| Configuración | `openapi-generator.yaml` |

Regenerar (desde el monorepo de Trevol): `sdks/generar.sh csharp`. Comprobar que lo del repositorio es lo que sale de la
especificación: `sdks/generar.sh --comprobar csharp`.
