// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploCreateCustomer
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<CatalogCustomerInput>("""
            {
              "external_id": "ERP-CLI-0042",
              "document_type": "31",
              "document_number": "900373427",
              "name": "DISTRIBUIDORA LOS ANDES SAS",
              "person_type": "legal",
              "tax_responsibilities": [
                "O-13"
              ],
              "email": "facturacion@distribuidoralosandes.example",
              "phone": "6045550142",
              "address": "Carrera 43A # 1-50, Oficina 302",
              "city": "Medellín",
              "municipality_code": "05001",
              "defaults": {
                "payment_form": "credit",
                "term_days": 30,
                "payment_method": "47",
                "withholding_agent": true
              }
            }
            """);
        var resultado = await trevol.Customers.CreateCustomerAsync(
            catalogCustomerInput: entrada
            // , idempotencyKey: "VENTA-2026-000123" // opcional: si falta, el SDK pone una y la repite en los reintentos
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
