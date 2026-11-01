using System.Text;
using Newtonsoft.Json.Linq;
using Trevol.Generado.Model;
using Xunit;

namespace Trevol.Pruebas;

public class WebhookTests
{
    public static IEnumerable<object[]> Casos() => ((JArray)Banco.Leer("webhooks.json")["casos"]!).Select(c => new object[] { c.ToString(Newtonsoft.Json.Formatting.None) });

    private static JObject Ok() => (JObject)((JArray)Banco.Leer("webhooks.json")["casos"]!).First(c => (string)c["nombre"]! == "valido");

    private static Webhook En(JObject c) => new Webhook((string)c["secreto"]!, () => (long)c["ahora"]!);

    private static Dictionary<string, string> Cab(JObject c) => ((JObject)c["cabeceras"]!).Properties().ToDictionary(p => p.Name, p => (string)p.Value!);

    [Fact]
    public void LaToleranciaEsDe5Minutos()
    {
        Assert.Equal(300, (int)Banco.Leer("webhooks.json")["tolerancia"]!);
        Assert.Equal(300, Webhook.Tolerancia);
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void VectoresDePruebasDatosWebhooksJson(string json)
    {
        var caso = JObject.Parse(json);
        var wh = En(caso);
        if ((bool)caso["valido"]!)
        {
            var e = wh.Verify((string)caso["cuerpo"]!, Cab(caso));
            Assert.Equal((string)caso["evento"]!["id"]!, e.Id);
            Assert.Equal((string)caso["evento"]!["type"]!, e.Type);
        }
        else
        {
            var x = Assert.Throws<WebhookVerificationException>(() => wh.Verify((string)caso["cuerpo"]!, Cab(caso)));
            Assert.Equal((string)caso["motivo"]!, x.Reason);
        }
    }

    [Fact]
    public void BytesYCabecerasDeCualquierCajaConValoresEnLista()
    {
        var ok = Ok();
        var wh = En(ok);
        var c = Cab(ok);
        Assert.NotNull(wh.Verify(Encoding.UTF8.GetBytes((string)ok["cuerpo"]!), c).Id);
        var http = new Dictionary<string, IEnumerable<string>> { ["Webhook-Id"] = new[] { c["webhook-id"], "evt_otro" }, ["WEBHOOK-TIMESTAMP"] = new[] { c["webhook-timestamp"] }, ["webhook-signature"] = new[] { c["webhook-signature"] } };
        Assert.NotNull(wh.Verify((string)ok["cuerpo"]!, http).Id);
    }

    [Fact]
    public void UnCuerpoNuloOInvalidoUtf8SeRechazaDiciendoComoLeerElCrudo()
    {
        var ok = Ok();
        var e = Assert.Throws<WebhookVerificationException>(() => En(ok).Verify((string)null!, Cab(ok)));
        Assert.Contains("CRUDO", e.Message);
        Assert.Equal("cuerpo", e.Reason);
        Assert.Equal("cuerpo", Assert.Throws<WebhookVerificationException>(() => En(ok).Verify(new byte[] { 0xff, 0xfe }, Cab(ok))).Reason);
    }

    [Fact]
    public void UnSecretoVacioOConBasuraEsConfigurationExceptionYNoSeFiltra()
    {
        foreach (var malo in new[] { "", "   ", "whsec_" }) Assert.Throws<ConfigurationException>(() => new Webhook(malo));
        var e = Assert.Throws<ConfigurationException>(() => new Webhook("whsec_@@@@@"));
        Assert.DoesNotContain("@@@", e.Message);
    }

    [Fact]
    public void EventDataDaElModeloSegunElTypeYUnTypeNuevoDaNull()
    {
        var ok = Ok();
        var wh = En(ok);
        var e = wh.Verify((string)ok["cuerpo"]!, Cab(ok));
        var d = Assert.IsType<DocumentEventData>(wh.EventData(e));
        Assert.Equal("accepted", d.Status);
        e.Type = "invoice.archived";
        Assert.Null(wh.EventData(e));
    }

    [Fact]
    public void ElMensajeDeUnRechazoNoIncluyeElSecreto()
    {
        var ok = Ok();
        var e = Assert.Throws<WebhookVerificationException>(() => En(ok).Verify((string)ok["cuerpo"]! + " ", Cab(ok)));
        Assert.DoesNotContain(((string)ok["secreto"]!).Substring(6), e.Message + e.StackTrace);
    }
}
