// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploRotateWebhookEndpointSecret
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<WebhookEndpointRotateInput>("""
            {
              "grace_period_hours": 24
            }
            """);
        var resultado = await trevol.WebhookEndpoints.RotateWebhookEndpointSecretAsync(
            id: "we_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b",
            webhookEndpointRotateInput: entrada
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
