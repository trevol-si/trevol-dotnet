// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploUpdateCustomer
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<CatalogCustomerUpdate>("""
            {
              "email": "cartera@distribuidoralosandes.example",
              "defaults": {
                "term_days": 45
              }
            }
            """);
        var resultado = await trevol.Customers.UpdateCustomerAsync(
            id: "cus_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b",
            catalogCustomerUpdate: entrada
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
