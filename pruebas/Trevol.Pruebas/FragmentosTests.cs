using System.Reflection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Trevol.Pruebas;

/// <summary>
/// Los fragmentos de la documentación, como programas (docs/api/43 §2.9): se COMPILAN (forman parte de este proyecto: un fragmento que no compila
/// rompe el build) y se CORREN contra Prism. Un fragmento que Prism rechaza porque el cuerpo no cumple la especificación rompe esta suite.
/// </summary>
public class FragmentosTests
{
    private static string Clase(string op) => "Trevol.Pruebas.Programas.Ejemplo" + char.ToUpperInvariant(op[0]) + op.Substring(1);

    public static IEnumerable<object[]> Operaciones() =>
        ((JObject)Banco.Leer("fragmentos.json")["fragmentos"]!).Properties().Select(p => p.Name).OrderBy(x => x, StringComparer.Ordinal).Select(o => new object[] { o });

    [Fact]
    public void LaEstructura()
    {
        var ops = Operaciones().Select(o => (string)o[0]).ToArray();
        Assert.Equal(109, ops.Length);
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
        try
        {
            await (Task)tipo.GetMethod("Ejecutar", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null)!;
        }
        catch (TrevolException e)
        {
            var v = e.Headers.FirstOrDefault(h => h.Key.Equals("sl-violations", StringComparison.OrdinalIgnoreCase)).Value;
            Assert.Fail($"Prism rechazó la petición ({e.Status}): {(v == null ? e.Message : string.Join(",", v))}");
        }
    }
}
