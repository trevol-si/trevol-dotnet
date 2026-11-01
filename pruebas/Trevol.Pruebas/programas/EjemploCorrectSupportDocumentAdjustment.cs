// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploCorrectSupportDocumentAdjustment
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<SupportDocumentAdjustmentInput>("""
            {
              "external_id": "OC-991-ANULA",
              "concept": "2",
              "description": "Anulación: la compra de leche se registró dos veces en el ERP",
              "support_document": {
                "id": "sd_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b"
              },
              "items": [
                {
                  "description": "Leche cruda",
                  "unit": "LTR",
                  "quantity": "120",
                  "unit_price": "1800"
                }
              ]
            }
            """);
        var resultado = await trevol.SupportDocumentAdjustments.CorrectSupportDocumentAdjustmentAsync(
            id: "sda_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b",
            supportDocumentAdjustmentInput: entrada
            // , idempotencyKey: "VENTA-2026-000123" // opcional: si falta, el SDK pone una y la repite en los reintentos
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
