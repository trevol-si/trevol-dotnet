// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploUpdateWebhookEndpoint
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<WebhookEndpointUpdate>("""
            {
              "description": "Pausado mientras migramos el servidor",
              "status": "paused"
            }
            """);
        var resultado = await trevol.WebhookEndpoints.UpdateWebhookEndpointAsync(
            id: "we_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b",
            webhookEndpointUpdate: entrada
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
