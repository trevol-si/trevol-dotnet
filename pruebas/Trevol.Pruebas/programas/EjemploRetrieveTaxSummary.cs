// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;

namespace Trevol.Pruebas.Programas { public static class EjemploRetrieveTaxSummary
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var resultado = await trevol.Reports.RetrieveTaxSummaryAsync(
            issuedFrom: "2026-09-01",
            issuedTo: "2026-10-31"
        );
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
