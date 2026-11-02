// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploCreateRadianEvent
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<RadianEventInput>("""
            {
              "external_id": "ERP-ACUSE-DA1234",
              "event_code": "030",
              "invoice": {
                "cufe": "cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd",
                "number": "DA1234",
                "type": "01",
                "issuer": {
                  "nit": "900373427",
                  "check_digit": 6,
                  "document_type": "31",
                  "name": "DISTRIBUIDORA LOS ANDES SAS"
                }
              }
            }
            """);
        var resultado = await trevol.RadianEvents.CreateRadianEventAsync(
            radianEventInput: entrada
            // , idempotencyKey: "VENTA-2026-000123" // opcional: si falta, el SDK pone una y la repite en los reintentos
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
