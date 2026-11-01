// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploCorrectEquivalentDocumentAdjustment
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<EquivalentDocumentAdjustmentInput>("""
            {
              "external_id": "TIQ-1-DEV",
              "note_type": "credit",
              "concept": "1",
              "equivalent_document": {
                "id": "pos_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b"
              },
              "items": [
                {
                  "description": "Devolución gaseosa",
                  "unit": "94",
                  "quantity": "1",
                  "unit_price": "3000",
                  "taxes": [
                    {
                      "code": "01",
                      "rate": "19"
                    }
                  ]
                }
              ]
            }
            """);
        var resultado = await trevol.EquivalentDocumentAdjustments.CorrectEquivalentDocumentAdjustmentAsync(
            id: "posa_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b",
            equivalentDocumentAdjustmentInput: entrada
            // , idempotencyKey: "VENTA-2026-000123" // opcional: si falta, el SDK pone una y la repite en los reintentos
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
