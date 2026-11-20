// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploUpdateCompanyLogo
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<CompanyLogoInput>("""
            {
              "image_base64": "iVBORw0KGgoAAAANSUhEUgAAAGAAAAAgAQMAAADXDu8XAAAABlBMVEX///8KZKiBydPRAAAAJ0lEQVR42mNgIAPw////AcZmQpagAcf+/////////0BDe+jlH3IAAC07DVBS46jVAAAAAElFTkSuQmCC"
            }
            """);
        var resultado = await trevol.CompanySettings.UpdateCompanyLogoAsync(companyLogoInput: entrada);
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
