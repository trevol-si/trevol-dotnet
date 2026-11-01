// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploPreviewSupportDocument
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<SupportDocumentInput>("""
            {
              "external_id": "OC-991",
              "seller": {
                "resident": true,
                "person_type": "natural",
                "document_type": "31",
                "document_number": "1020304050",
                "name": "PEREZ GOMEZ ANA MARIA",
                "address": "Vereda El Silencio, finca La Esperanza",
                "municipality_code": "25053",
                "postal_code": "252007",
                "tax_responsibilities": [
                  "R-99-PN"
                ]
              },
              "items": [
                {
                  "description": "Leche cruda",
                  "unit": "LTR",
                  "quantity": "120",
                  "unit_price": "1800"
                }
              ],
              "payment": {
                "form": "cash",
                "methods": [
                  {
                    "code": "10"
                  }
                ]
              },
              "notes": [
                "Compra de la semana 42 de octubre"
              ]
            }
            """);
        var resultado = await trevol.SupportDocuments.PreviewSupportDocumentAsync(supportDocumentInput: entrada);
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
