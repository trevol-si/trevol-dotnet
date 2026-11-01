// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploCreateEquivalentDocument
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<EquivalentDocumentInput>("""
            {
              "external_id": "TIQ-1",
              "customer": {
                "final_consumer": true
              },
              "pos": {
                "register": {
                  "plate": "CJ-01",
                  "location": "Local 1, Jamundí",
                  "cashier": "PEREZ ANA",
                  "type": "Caja principal"
                }
              },
              "numbering": {
                "prefix": "POS2"
              },
              "payment": {
                "form": "cash",
                "methods": [
                  {
                    "code": "10"
                  }
                ]
              },
              "items": [
                {
                  "description": "Gaseosa 400 ml",
                  "code": "GAS-1",
                  "unit": "94",
                  "quantity": "2",
                  "unit_price": "3000",
                  "taxes": [
                    {
                      "code": "01",
                      "rate": "19"
                    }
                  ]
                },
                {
                  "description": "Almuerzo",
                  "unit": "94",
                  "quantity": "1",
                  "unit_price": "18000"
                }
              ]
            }
            """);
        var resultado = await trevol.EquivalentDocuments.CreateEquivalentDocumentAsync(
            equivalentDocumentInput: entrada
            // , idempotencyKey: "VENTA-2026-000123" // opcional: si falta, el SDK pone una y la repite en los reintentos
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
