#nullable enable
using System.Text.RegularExpressions;

namespace Trevol
{
    /// <summary><c>Test</c> (llave tvl_test_ o tva_test_, habilitación de la DIAN) o <c>Live</c> (tvl_live_ o tva_live_, producción): el mismo vocabulario que <c>event.environment</c>.</summary>
    public enum TrevolEnvironment
    {
        Test,
        Live
    }

    /// <summary>
    /// <c>Api</c>: la llave de una empresa (<c>tvl_…</c>). <c>Account</c>: una llave de administración (<c>tva_…</c>), que sirve en <c>trevol.Account</c>
    /// y, con una empresa (<see cref="TrevolClientOptions.Company"/> o <see cref="TrevolClient.WithCompany"/>), en las rutas de esa empresa.
    /// </summary>
    public enum TrevolKeyType
    {
        Api,
        Account
    }

    /// <summary>La llave de la API: su formato, su tipo y el entorno que se deduce de su prefijo (docs/api/43 §2.6.1). Y el id de una empresa.</summary>
    internal static class Llave
    {
        private static readonly Regex Formato = new Regex("^(tvl|tva)_(test|live)_[0-9a-f]{64}$", RegexOptions.CultureInvariant);
        private static readonly Regex FormatoEmpresa = new Regex("^com_[0-9a-f]{32}$", RegexOptions.CultureInvariant);

        /// <summary>Valida la llave y devuelve su tipo y su entorno. El mensaje NUNCA incluye la llave.</summary>
        internal static (TrevolKeyType Tipo, TrevolEnvironment Entorno) Leer(string? llave)
        {
            var m = llave == null ? null : Formato.Match(llave.Trim());
            if (m == null || !m.Success)
                throw new ConfigurationException("La llave de la API no tiene el formato esperado (tvl_test_…, tvl_live_…, tva_test_… o tva_live_… con 64 caracteres hexadecimales). " +
                    "La de una empresa se crea en https://platform.trevol.si/llaves y la de administración en " +
                    "https://platform.trevol.si/administracion; pásala completa o define TREVOL_API_KEY.");
            return (m.Groups[1].Value == "tva" ? TrevolKeyType.Account : TrevolKeyType.Api,
                m.Groups[2].Value == "test" ? TrevolEnvironment.Test : TrevolEnvironment.Live);
        }

        /// <summary>Valida el id de una empresa (<c>com_</c> y 32 hexadecimales en minúscula) para una llave de administración.</summary>
        internal static string Empresa(string? empresa, TrevolKeyType tipo)
        {
            if (tipo != TrevolKeyType.Account)
                throw new ConfigurationException("Company solo aplica a una llave de administración (tva_…): una llave tvl_ ya es de una empresa.");
            var e = empresa?.Trim();
            if (e == null || !FormatoEmpresa.IsMatch(e))
                throw new ConfigurationException("Company debe ser el id de una empresa de tu cuenta: com_ y 32 caracteres hexadecimales en minúscula (lo ves en trevol.Account.ListAccountCompaniesAsync).");
            return e;
        }
    }
}
