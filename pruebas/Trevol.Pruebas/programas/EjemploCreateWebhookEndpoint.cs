// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploCreateWebhookEndpoint
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<WebhookEndpointInput>("""
            {
              "url": "https://tu-servidor.co/webhooks/trevol",
              "description": "Producción: facturación",
              "events": [
                "invoice.accepted",
                "invoice.rejected",
                "credit_note.accepted"
              ]
            }
            """);
        var resultado = await trevol.WebhookEndpoints.CreateWebhookEndpointAsync(webhookEndpointInput: entrada);
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
