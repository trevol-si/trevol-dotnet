#nullable enable
using System.Text.RegularExpressions;

namespace Trevol
{
    /// <summary><c>Test</c> (llave tvl_test_, habilitación de la DIAN) o <c>Live</c> (tvl_live_, producción): el mismo vocabulario que <c>event.environment</c>.</summary>
    public enum TrevolEnvironment
    {
        Test,
        Live
    }

    /// <summary>La llave de la API: su formato y el entorno que se deduce de su prefijo (docs/api/43 §2.6.1).</summary>
    internal static class Llave
    {
        private static readonly Regex Formato = new Regex("^tvl_(test|live)_[0-9a-f]{64}$", RegexOptions.CultureInvariant);

        /// <summary>Valida la llave y devuelve su entorno. El mensaje NUNCA incluye la llave.</summary>
        internal static TrevolEnvironment Entorno(string? llave)
        {
            var m = llave == null ? null : Formato.Match(llave.Trim());
            if (m == null || !m.Success)
                throw new ConfigurationException("La llave de la API no tiene el formato esperado (tvl_test_… o tvl_live_… con 64 caracteres hexadecimales). " +
                    "Créala en https://platform.trevol.si/llaves y pásala completa o define TREVOL_API_KEY.");
            return m.Groups[1].Value == "test" ? TrevolEnvironment.Test : TrevolEnvironment.Live;
        }
    }
}
