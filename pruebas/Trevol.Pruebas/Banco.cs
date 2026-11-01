using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Trevol.Pruebas;

/// <summary>
/// Un cliente contra una red falsa: un manejador que contesta con las respuestas dadas (la última se repite) y anota las peticiones. El
/// código generado y el <see cref="Transporte"/> del SDK son los de verdad; solo la red es falsa. El reloj y el azar están inyectados.
/// </summary>
public sealed class Banco
{
    public const string LlaveTest = "tvl_test_a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1";
    public const string LlaveLive = "tvl_live_b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2";
    public const string Hex = "3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b";
    public static readonly string IdDoc = "inv_" + Hex;
    public const string Documento = "{\"id\":\"inv_3f0c1a2b3f0c1a2b3f0c1a2b3f0c1a2b\",\"object\":\"invoice\",\"environment\":\"test\",\"status\":\"accepted\",\"number\":\"SETP990012345\",\"external_id\":\"VENTA-2026-000123\"}";
    public const string EntradaFactura = "{\"external_id\":\"VENTA-2026-000123\",\"customer\":{\"final_consumer\":true},\"items\":[{\"description\":\"Café\",\"quantity\":\"1\",\"unit_price\":\"1000\"}]}";

    /// <summary>Una petición anotada.</summary>
    public sealed record Pet(string Metodo, string Ruta, Dictionary<string, string> Cabeceras, string Cuerpo);

    /// <summary>Una respuesta del servidor falso: una función de la petición, un fallo de red o una respuesta fija.</summary>
    public sealed class Resp
    {
        public int Estado { get; init; }
        public byte[] Cuerpo { get; init; } = Array.Empty<byte>();
        public string Tipo { get; init; } = "application/json";
        public Dictionary<string, string> Cabeceras { get; init; } = new();
        public bool Cortar { get; init; }
    }

    public static Resp Json(int estado, string cuerpo) => new() { Estado = estado, Cuerpo = Encoding.UTF8.GetBytes(cuerpo) };

    public static Resp Crudo(int estado, byte[] cuerpo, string tipo) => new() { Estado = estado, Cuerpo = cuerpo, Tipo = tipo };

    public static Resp Cortar() => new() { Cortar = true };

    public static Resp Problema(int estado, string code, string? extraJson = null, Dictionary<string, string>? cabeceras = null)
    {
        var m = new JObject
        {
            ["type"] = $"https://platform.trevol.si/docs/errores#{code}", ["title"] = "Título", ["status"] = estado, ["code"] = code,
            ["retryable"] = false, ["detail"] = $"Detalle de {code}", ["request_id"] = "req_c3c3c3c3c3c3c3c3c3c3c3c3"
        };
        if (extraJson != null) foreach (var p in JObject.Parse(extraJson).Properties()) m[p.Name] = p.Value;
        return new Resp { Estado = estado, Cuerpo = Encoding.UTF8.GetBytes(m.ToString(Formatting.None)), Tipo = "application/problem+json", Cabeceras = cabeceras ?? new() };
    }

    private sealed class Falso : HttpMessageHandler
    {
        private readonly Banco _b;
        public Falso(Banco b) => _b = b;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var cab = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in request.Headers) cab[h.Key] = h.Key == "User-Agent" ? request.Headers.UserAgent.ToString() : string.Join(", ", h.Value);
            if (request.Content != null) foreach (var h in request.Content.Headers) cab[h.Key] = string.Join(", ", h.Value);
            var p = new Pet(request.Method.Method, request.RequestUri!.PathAndQuery, cab, request.Content == null ? "" : await request.Content.ReadAsStringAsync(ct));
            Resp r;
            lock (_b.Peticiones)
            {
                _b.Peticiones.Add(p);
                var x = _b._respuestas[Math.Min(_b.Peticiones.Count - 1, _b._respuestas.Length - 1)];
                r = x is Func<Pet, Resp> f ? f(p) : (Resp)x;
            }
            if (r.Cortar) throw new HttpRequestException("conexión cortada (prueba)");
            var resp = new HttpResponseMessage((HttpStatusCode)r.Estado) { Content = new ByteArrayContent(r.Cuerpo), RequestMessage = request };
            resp.Content.Headers.TryAddWithoutValidation("Content-Type", r.Tipo);
            foreach (var (k, v) in r.Cabeceras) resp.Headers.TryAddWithoutValidation(k, v);
            return resp;
        }
    }

    private readonly object[] _respuestas;
    private int _uuid;
    public List<Pet> Peticiones { get; } = new();
    public List<double> Esperas { get; } = new();
    public TrevolClient Trevol { get; }

    /// <summary>Cada elemento de <paramref name="respuestas"/> es una <see cref="Resp"/> o una <c>Func&lt;Pet, Resp&gt;</c>; la última se repite.</summary>
    public Banco(string llave, Action<TrevolClientOptions>? opciones, params object[] respuestas)
    {
        _respuestas = respuestas;
        var o = new TrevolClientOptions
        {
            BaseUrl = "http://127.0.0.1:9/v1", Handler = new Falso(this),
            Dormir = (s, _) => { Esperas.Add(s); return Task.CompletedTask; }, Azar = () => 1.0,
            Uuid = () => $"00000000-0000-4000-8000-{Interlocked.Increment(ref _uuid):D12}"
        };
        opciones?.Invoke(o);
        Trevol = new TrevolClient(llave, o);
    }

    public Banco(params object[] respuestas) : this(LlaveTest, null, respuestas) { }

    public static string Datos(string nombre)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "pruebas", "datos"))) dir = dir.Parent;
        return File.ReadAllText(Path.Combine(dir!.FullName, "pruebas", "datos", nombre), Encoding.UTF8);
    }

    public static JObject Leer(string nombre) => JObject.Parse(Datos(nombre));
}
