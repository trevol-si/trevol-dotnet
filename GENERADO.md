# Código generado

**No edites `src/Trevol/Generado/`**: `sdks/generar.sh` la borra y la vuelve a escribir entera. La capa propia (cliente,
reintentos, errores, paginación, webhooks) vive fuera de esa carpeta.

| | |
|---|---|
| Generador | OpenAPI Generator 7.26.0 (`csharp`) |
| Especificación | `especificacion/openapi.sdk.enums-abiertos.json` (variante 3.0.3 de `docs/api/openapi.json`, versión de la API 2026-11-18) |
| sha256 de la especificación | `4ab73606bc7ce824d31f4904dd506892f95240f6d55b61a0fb84357fe6612aaf` |
| Configuración | `openapi-generator.yaml` |

Regenerar (desde el monorepo de Trevol): `sdks/generar.sh csharp`. Comprobar que lo del repositorio es lo que sale de la
especificación: `sdks/generar.sh --comprobar csharp`.
