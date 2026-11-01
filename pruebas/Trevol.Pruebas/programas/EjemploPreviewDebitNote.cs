// Generado por sdks/herramientas/fragmentos.mjs: el fragmento de la documentación, apuntando a Prism. No editar.
using System;
using System.Threading.Tasks;
using Trevol;
using Trevol.Generado.Model;

namespace Trevol.Pruebas.Programas { public static class EjemploPreviewDebitNote
{
    public static async Task Ejecutar()
    {
        var trevol = new TrevolClient(Environment.GetEnvironmentVariable("TREVOL_API_KEY"), new TrevolClientOptions { BaseUrl = Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")!, MaxRetries = 0 }); // tvl_test_… → trevol.Environment == TrevolEnvironment.Test

        var entrada = TrevolJson.Deserialize<NoteInput>("""
            {
              "external_id": "INT-2026-000012",
              "concept": "1",
              "description": "Intereses de mora de la factura de septiembre",
              "invoice": {
                "id": "inv_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b"
              },
              "items": [
                {
                  "description": "Intereses de mora",
                  "unit": "94",
                  "quantity": "1",
                  "unit_price": "45000"
                }
              ]
            }
            """);
        var resultado = await trevol.DebitNotes.PreviewDebitNoteAsync(noteInput: entrada);
        Console.WriteLine(TrevolJson.Serialize(resultado));
    }
} }
