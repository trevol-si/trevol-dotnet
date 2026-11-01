// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;

namespace Trevol.Pruebas.Programas { public static class EjemploSendSupportDocument
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var resultado = await trevol.SupportDocuments.SendSupportDocumentAsync(id: "sd_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b");
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
