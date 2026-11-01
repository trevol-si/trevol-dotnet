// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;

namespace Trevol.Pruebas.Programas { public static class EjemploListWebhookEndpoints
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var lista = await trevol.WebhookEndpoints.ListWebhookEndpointsAsync();
        foreach (var item in lista.Data)
        {
            Console.WriteLine(item.Id);
        }
    }
} }
