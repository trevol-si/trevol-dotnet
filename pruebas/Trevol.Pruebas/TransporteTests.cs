using Newtonsoft.Json.Linq;
using Xunit;

namespace Trevol.Pruebas;

public class TransporteTests
{
    public static IEnumerable<object[]> Casos() => ((JArray)Banco.Leer("transporte.json")["casos"]!).Select(c => new object[] { c.ToString(Newtonsoft.Json.Formatting.None) });

    [Fact]
    public void HayCasos() => Assert.True(Casos().Count() >= 50);

    [Theory]
    [MemberData(nameof(Casos))]
    public void DecidirConPruebasDatosTransporteJson(string json)
    {
        var caso = JObject.Parse(json);
        var r = (JObject)caso["resultado"]!;
        var resultado = (string)r["tipo"]! == "red"
            ? ResultadoIntento.DeRed()
            : ResultadoIntento.DeRespuesta((int)r["estado"]!, (string?)r["code"], (bool?)r["retryable"], (double?)r["retryAfter"]);
        var d = Transporte.Decidir((int)caso["intento"]!, (string)caso["reintento"]!, (int)caso["maxReintentos"]!, (double)caso["maxRetryAfter"]!, (double)caso["azar"]!, resultado);
        var e = (JObject)caso["esperado"]!;
        Assert.Equal((bool)e["reintentar"]!, d.Reintentar);
        if (e.ContainsKey("espera")) Assert.Equal((double)e["espera"]!, d.Espera!.Value, 9);
        if (e.ContainsKey("motivo")) Assert.Equal((string)e["motivo"]!, d.Motivo);
        if (!(bool)e["reintentar"]!) Assert.Null(d.Espera);
    }

    [Fact]
    public void LaEsperaPropiaCreceConTopeDe8YUnFactorEntre075Y1()
    {
        Assert.Equal(new double[] { 0.5, 1, 2, 4, 8, 8, 8 }, new[] { 1, 2, 3, 4, 5, 6, 9 }.Select(n => Transporte.EsperaPropia(n, 1.0)).ToArray());
        Assert.Equal(0.375, Transporte.EsperaPropia(1, 0.0));
        Assert.Equal(3.0, Transporte.EsperaPropia(4, 0.0));
    }
}
