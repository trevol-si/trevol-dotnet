#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Trevol
{
    /// <summary>Paginación con iteradores (docs/api/43 §2.6.7): la API pagina por cursor (<c>starting_after</c>).</summary>
    public static class Paginacion
    {
        /// <summary>
        /// Recorre todas las páginas de un listado, perezosamente. <paramref name="pagina"/>(cursor) pide UNA página
        /// (<c>starting_after = cursor</c>; null la primera). Sigue con <c>next_cursor</c> mientras <c>has_more</c>; si la página no lo trae, usa
        /// el <c>id</c> del último elemento (así lo arma la API). Se detiene si el cursor no avanza: una API que repitiera el mismo cursor no
        /// puede dejar el bucle girando para siempre.
        /// </summary>
        public static async IAsyncEnumerable<TItem> Recorrer<TPage, TItem>(
            Func<string?, Task<TPage>> pagina,
            Func<TPage, IList<TItem>?> datos,
            Func<TPage, bool?> hayMas,
            Func<TPage, string?> siguiente,
            Func<TItem, string?> id,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string? cursor = null;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var p = await pagina(cursor).ConfigureAwait(false);
                var lista = datos(p) ?? new List<TItem>();
                foreach (var d in lista) yield return d;
                if (hayMas(p) != true || lista.Count == 0) yield break;
                var s = siguiente(p);
                if (string.IsNullOrEmpty(s)) s = id(lista[lista.Count - 1]);
                if (string.IsNullOrEmpty(s) || s == cursor) yield break;
                cursor = s;
            }
        }
    }
}
