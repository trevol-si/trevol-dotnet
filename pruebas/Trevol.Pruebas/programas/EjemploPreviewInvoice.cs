// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploPreviewInvoice
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<InvoiceInput>("""
            {
              "external_id": "VENTA-2026-000123",
              "customer": {
                "document_type": "31",
                "document_number": "900123456",
                "name": "COMERCIALIZADORA ANDINA SAS",
                "person_type": "legal",
                "email": "facturacion@comercializadoraandina.example",
                "address": "Calle 72 # 10-34, Oficina 501",
                "municipality_code": "11001"
              },
              "items": [
                {
                  "code": "LIC-ANUAL",
                  "description": "Licencia anual de software",
                  "quantity": "2",
                  "unit_price": "150000",
                  "taxes": [
                    {
                      "code": "01",
                      "rate": "19"
                    }
                  ]
                },
                {
                  "code": "SOP-MES",
                  "description": "Soporte técnico mensual",
                  "quantity": "1",
                  "unit_price": "80000",
                  "taxes": [
                    {
                      "code": "01",
                      "rate": "19"
                    }
                  ]
                }
              ],
              "payment": {
                "form": "credit",
                "term_days": 30
              },
              "notes": [
                "Gracias por su compra"
              ]
            }
            """);
        var resultado = await trevol.Invoices.PreviewInvoiceAsync(invoiceInput: entrada);
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
