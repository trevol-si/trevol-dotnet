using Trevol.Generado.Model;
using Xunit;

namespace Trevol.Pruebas;

public class PaginacionTests
{
    private static Document Doc(string n) => new Document { Id = "inv_" + n };

    private static InvoiceList Pagina(bool hay, string? cursor, params string[] ids) =>
        new InvoiceList { Data = ids.Select(Doc).ToList(), HasMore = hay, NextCursor = cursor! };

    private static async Task<List<T>> Todo<T>(IAsyncEnumerable<T> it)
    {
        var l = new List<T>();
        await foreach (var x in it) l.Add(x);
        return l;
    }

    [Fact]
    public async Task RecorreTresPaginasFalsasConNextCursorYSeDetiene()
    {
        var pedidos = new List<string?>();
        var paginas = new[] { Pagina(true, "inv_2", "1", "2"), Pagina(true, "inv_4", "3", "4"), Pagina(false, null, "5") };
        var ids = (await Todo(new TrevolClient(Banco.LlaveTest).PaginateAsync(c => { pedidos.Add(c); return Task.FromResult(paginas[pedidos.Count - 1]); }))).Select(d => d.Id).ToArray();
        Assert.Equal(new[] { "inv_1", "inv_2", "inv_3", "inv_4", "inv_5" }, ids);
        Assert.Equal(new string?[] { null, "inv_2", "inv_4" }, pedidos);
    }

    [Fact]
    public async Task SiHasMoreVieneSinNextCursorUsaElIdDelUltimo()
    {
        var pedidos = new List<string?>();
        var paginas = new[] { Pagina(true, null, "1", "2"), Pagina(false, null, "3") };
        var n = (await Todo(new TrevolClient(Banco.LlaveTest).PaginateAsync(c => { pedidos.Add(c); return Task.FromResult(paginas[pedidos.Count - 1]); }))).Count;
        Assert.Equal(3, n);
        Assert.Equal(new string?[] { null, "inv_2" }, pedidos);
    }

    [Fact]
    public async Task EsPerezosa()
    {
        var llamadas = 0;
        await foreach (var _ in new TrevolClient(Banco.LlaveTest).PaginateAsync(c => { llamadas++; return Task.FromResult(Pagina(true, "inv_2", "1", "2")); }))
            break;
        Assert.Equal(1, llamadas);
    }

    [Fact]
    public async Task UnCursorQueNoAvanzaOUnaPaginaVaciaNoDejanElBucleGirando()
    {
        var t = new TrevolClient(Banco.LlaveTest);
        Assert.True((await Todo(t.PaginateAsync(c => Task.FromResult(Pagina(true, "inv_1", "1"))))).Count <= 3);
        Assert.Empty(await Todo(t.PaginateAsync(c => Task.FromResult(Pagina(true, "x")))));
    }

    [Fact]
    public async Task ClientPaginateConUnMetodoGeneradoYElServidorFalso()
    {
        var b = new Banco(
            Banco.Json(200, "{\"object\":\"list\",\"data\":[{\"id\":\"inv_1\"},{\"id\":\"inv_2\"}],\"has_more\":true,\"next_cursor\":\"inv_2\"}"),
            Banco.Json(200, "{\"object\":\"list\",\"data\":[{\"id\":\"inv_3\"}],\"has_more\":false,\"next_cursor\":null}"));
        var t = b.Trevol;
        var ids = new List<string>();
        await foreach (var f in t.PaginateAsync(cursor => t.Invoices.ListInvoicesAsync(limit: 2, startingAfter: cursor))) ids.Add(f.Id);
        Assert.Equal(new[] { "inv_1", "inv_2", "inv_3" }, ids);
        Assert.Equal("/v1/invoices?limit=2", b.Peticiones[0].Ruta);
        Assert.Equal("/v1/invoices?limit=2&starting_after=inv_2", b.Peticiones[1].Ruta);
    }
}
