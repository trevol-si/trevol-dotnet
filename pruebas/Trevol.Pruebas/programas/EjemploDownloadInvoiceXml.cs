// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.IO;
using System.Threading.Tasks;
using Trevol;

namespace Trevol.Pruebas.Programas { public static class EjemploDownloadInvoiceXml
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var archivo = await trevol.Invoices.DownloadInvoiceXmlAsync(id: "inv_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b");
        await using var salida = Stream.Null;
        await archivo.Content.CopyToAsync(salida); // el XML llega como un Stream
    }
} }
