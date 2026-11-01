// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploCreateInvoice
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<InvoiceInput>("""
            {
              "external_id": "EXP-77",
              "document_type": "02",
              "currency": {
                "code": "USD",
                "exchange_rate": "3950.12",
                "rate_date": "2026-10-21"
              },
              "delivery_terms": {
                "incoterm": "FOB",
                "conditions": "Puerto de Cartagena"
              },
              "customer": {
                "document_type": "50",
                "document_number": "US123456789",
                "name": "ACME INC",
                "country": "US",
                "city": "Miami",
                "region": "Florida",
                "postal_code": "33131",
                "address": "200 S Biscayne Blvd"
              },
              "items": [
                {
                  "description": "Consultoría",
                  "quantity": "1",
                  "unit_price": "1000",
                  "taxes": [
                    {
                      "code": "01",
                      "excluded": true
                    }
                  ]
                }
              ],
              "payment": {
                "form": "credit",
                "term_days": 30
              }
            }
            """);
        var resultado = await trevol.Invoices.CreateInvoiceAsync(
            invoiceInput: entrada
            // , idempotencyKey: "VENTA-2026-000123" // opcional: si falta, el SDK pone una y la repite en los reintentos
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
