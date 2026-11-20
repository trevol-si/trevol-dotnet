// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploUpdatePdfSettings
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<PdfSettingsUpdate>("""
            {
              "letter_design_id": "pdd_7d3e9a107d3e9a107d3e9a107d3e9a10",
              "receipt_design_id": "pdd_41b2c6f041b2c6f041b2c6f041b2c6f0"
            }
            """);
        var resultado = await trevol.CompanySettings.UpdatePdfSettingsAsync(pdfSettingsUpdate: entrada);
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
