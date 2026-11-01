using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Trevol.Pruebas;

public class FuturasTests
{
    private static IEnumerable<JToken> Casos() => (JArray)Banco.Leer("respuestas-futuras.json")["casos"]!;

    public static IEnumerable<object[]> Respuestas() => Casos().Where(c => c["operacion"] != null).Select(c => new object[] { c.ToString(Formatting.None) });

    public static IEnumerable<object[]> Eventos() => Casos().Where(c => c["evento"] != null).Select(c => new object[] { c.ToString(Formatting.None) });

    [Theory]
    [MemberData(nameof(Respuestas))]
    public async Task RespuestasDelFuturoNoRompen(string json)
    {
        var caso = JObject.Parse(json);
        var estado = (int)caso["estado"]!;
        var tipo = estado >= 400 ? "application/problem+json" : "application/json";
        var cab = caso["cabeceras"] is JObject h ? h.Properties().ToDictionary(p => p.Name, p => (string)p.Value!) : new Dictionary<string, string>();
        var b = new Banco(Banco.LlaveTest, o => o.MaxRetries = 0, new Banco.Resp { Estado = estado, Cuerpo = Encoding.UTF8.GetBytes(caso["cuerpo"]!.ToString(Formatting.None)), Tipo = tipo, Cabeceras = cab });
        var e = (JObject)caso["esperado"]!;
        if (estado < 400)
        {
            if ((string)caso["operacion"]! == "listInvoices")
            {
                var l = await b.Trevol.Invoices.ListInvoicesAsync();
                Assert.Equal((int)e["cantidad"]!, l.Data.Count);
                Assert.Equal((bool)e["has_more"]!, l.HasMore);
            }
            else
            {
                var d = await b.Trevol.Invoices.RetrieveInvoiceAsync("inv_x");
                Assert.Equal((string)e["id"]!, d.Id);
                Assert.Equal((string)e["status"]!, d.Status);
            }
            return;
        }
        var x = await Assert.ThrowsAnyAsync<TrevolException>(() => b.Trevol.Invoices.RetrieveInvoiceAsync("inv_x"));
        Assert.Equal(((string)e["clase"]!) == "TrevolError" ? "TrevolException" : ((string)e["clase"]!).Replace("Error", "Exception"), x.GetType().Name);
        Assert.Equal((string)e["code"]!, x.Code);
        Assert.Equal((int)e["status"]!, x.Status);
        if (e["requestId"] != null) Assert.Equal((string)e["requestId"]!, x.RequestId);
    }

    [Theory]
    [MemberData(nameof(Eventos))]
    public void EventosDelFuturo(string json)
    {
        var caso = JObject.Parse(json);
        var clave = Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef");
        var secreto = "whsec_" + Convert.ToBase64String(clave);
        const long ts = 1793491200;
        var evento = (JObject)caso["evento"]!;
        var cuerpo = evento.ToString(Formatting.None);
        string firma;
        using (var hmac = new HMACSHA256(clave)) firma = "v1," + Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{(string)evento["id"]!}.{ts}.{cuerpo}")));
        var wh = new Webhook(secreto, () => ts);
        var e = wh.Verify(cuerpo, new Dictionary<string, string> { ["webhook-id"] = (string)evento["id"]!, ["webhook-timestamp"] = ts.ToString(), ["webhook-signature"] = firma });
        var esperado = (JObject)caso["esperado"]!;
        Assert.Equal((string)esperado["type"]!, e.Type);
        Assert.Equal((bool)esperado["tieneModelo"]!, wh.EventData(e) != null);
        Assert.Equal(((JObject)evento["data"]!).Count, e.Data.Count);
    }
}
