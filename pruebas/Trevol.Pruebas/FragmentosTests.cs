using System.Reflection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Trevol.Pruebas;

/// <summary>
/// Los fragmentos de la documentación, como programas (docs/api/43 §2.9): se COMPILAN (forman parte de este proyecto: un fragmento que no compila
/// rompe el build) y se CORREN contra Prism. Un fragmento que Prism rechaza porque el cuerpo no cumple la especificación rompe esta suite.
/// Los programas leen <c>TREVOL_API_KEY</c>; los de <c>Account</c> (que con una <c>tvl_</c> no salen a la red) corren con una <c>tva_test_</c>
/// falsa. Prism no distingue una de otra (las dos son Bearer). La colección evita que otra clase cambie esa variable mientras tanto.
/// </summary>
[Collection(LlaveDelEntorno.Coleccion)]
public class FragmentosTests
{
    private static readonly HashSet<string> DeAdministracion = new(
        ((JArray)JArray.Parse(Banco.Datos("operaciones.json"))).Where(o => (string?)o["llave"] == "administracion").Select(o => (string)o["operationId"]!), StringComparer.Ordinal);

    private static string Clase(string op) => "Trevol.Pruebas.Programas.Ejemplo" + char.ToUpperInvariant(op[0]) + op.Substring(1);

    public static IEnumerable<object[]> Operaciones() =>
        ((JObject)Banco.Leer("fragmentos.json")["fragmentos"]!).Properties().Select(p => p.Name).OrderBy(x => x, StringComparer.Ordinal).Select(o => new object[] { o });

    [Fact]
    public void LaEstructura()
    {
        var ops = Operaciones().Select(o => (string)o[0]).ToArray();
        Assert.Equal(159, ops.Length);
        var fragmentos = (JObject)Banco.Leer("fragmentos.json")["fragmentos"]!;
        foreach (var op in ops) Assert.Contains("new TrevolClient(", (string)fragmentos[op]!["csharp"]!);
        var presentes = typeof(FragmentosTests).Assembly.GetTypes().Where(t => t.Namespace == "Trevol.Pruebas.Programas" && t.DeclaringType == null && !t.Name.StartsWith("<")).Select(t => t.FullName).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        Assert.Equal(ops.Select(Clase).OrderBy(x => x, StringComparer.Ordinal).ToArray(), presentes);
    }

    [PrismTheory]
    [MemberData(nameof(Operaciones))]
    public async Task ElFragmentoCorreContraPrism(string op)
    {
        var tipo = typeof(FragmentosTests).Assembly.GetType(Clase(op))!;
        var anterior = Environment.GetEnvironmentVariable("TREVOL_API_KEY");
        if (DeAdministracion.Contains(op)) Environment.SetEnvironmentVariable("TREVOL_API_KEY", LlaveAdministracionTests.AdminTest);
        try
        {
            await (Task)tipo.GetMethod("Ejecutar", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null)!;
        }
        catch (ConfigurationException e)
        {
            Assert.Fail($"El fragmento no salió a la red: {e.Message}");
        }
        catch (TrevolException e)
        {
            var v = e.Headers.FirstOrDefault(h => h.Key.Equals("sl-violations", StringComparison.OrdinalIgnoreCase)).Value;
            Assert.Fail($"Prism rechazó la petición ({e.Status}): {(v == null ? e.Message : string.Join(",", v))}");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TREVOL_API_KEY", anterior);
        }
    }

    [Fact]
    public void LasOperacionesDeAdministracionSonLasDeAccount()
    {
        Assert.Equal(10, DeAdministracion.Count);
        Assert.All(DeAdministracion, op => Assert.Contains("Account", op));
    }
}

/// <summary>Las clases que leen o cambian <c>TREVOL_API_KEY</c> del proceso no corren en paralelo entre sí.</summary>
[CollectionDefinition(LlaveDelEntorno.Coleccion)]
public sealed class LlaveDelEntorno
{
    public const string Coleccion = "TREVOL_API_KEY";
}
