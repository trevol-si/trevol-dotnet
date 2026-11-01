// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploCorrectCreditNote
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<NoteInput>("""
            {
              "external_id": "DEV-2026-000045",
              "concept": "1",
              "description": "Devolución de una unidad por empaque dañado",
              "invoice": {
                "id": "inv_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b"
              },
              "items": [
                {
                  "description": "Café especial 500 g",
                  "code": "CAF-500",
                  "unit": "94",
                  "quantity": "1",
                  "unit_price": "32000",
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
        var resultado = await trevol.CreditNotes.CorrectCreditNoteAsync(
            id: "cn_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b",
            noteInput: entrada
            // , idempotencyKey: "VENTA-2026-000123" // opcional: si falta, el SDK pone una y la repite en los reintentos
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
