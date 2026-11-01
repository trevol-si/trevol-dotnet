using Xunit;

namespace Trevol.Pruebas;

/// <summary>Una prueba que necesita Prism (<c>TREVOL_PRISM_URL</c>): sin él se omite.</summary>
public sealed class PrismFactAttribute : FactAttribute
{
    public PrismFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")))
            Skip = "necesita Prism: sdks/herramientas/prism.sh -- bash -c 'cd sdks/csharp && dotnet test pruebas/Trevol.Pruebas'";
    }
}

public sealed class PrismTheoryAttribute : TheoryAttribute
{
    public PrismTheoryAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TREVOL_PRISM_URL")))
            Skip = "necesita Prism: sdks/herramientas/prism.sh -- bash -c 'cd sdks/csharp && dotnet test pruebas/Trevol.Pruebas'";
    }
}
