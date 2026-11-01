// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;

namespace Trevol.Pruebas.Programas { public static class EjemploListCreditNotes
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        await foreach (var item in trevol.PaginateAsync(cursor => trevol.CreditNotes.ListCreditNotesAsync(limit: 100, startingAfter: cursor)))
        {
            Console.WriteLine(item.Id);
        }
    }
} }
