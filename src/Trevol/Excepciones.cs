#nullable enable
using System;
using System.Collections.Generic;

namespace Trevol
{
    /// <summary>Una de las <c>errors[]</c> de un 422: qué campo falló (<see cref="Pointer"/>) y por qué.</summary>
    public sealed class FieldError
    {
        public FieldError(string? field, string? pointer, string? message, IReadOnlyDictionary<string, object?>? extra = null)
        {
            Field = field;
            Pointer = pointer;
            Message = message;
            Extra = extra ?? new Dictionary<string, object?>();
        }

        public string? Field { get; }
        public string? Pointer { get; }
        public string? Message { get; }

        /// <summary>Los campos que el SDK no conoce todavía.</summary>
        public IReadOnlyDictionary<string, object?> Extra { get; }

        public override string ToString() => $"{Pointer} {Message}";
    }

    /// <summary>
    /// La base de todos los errores del SDK (docs/api/43 §2.6.5): un <c>catch (TrevolException)</c> ya lo atrapa todo. Toda respuesta que no es
    /// 2xx llega como una subclase, según el <c>code</c> del problem+json (el mismo vocabulario que https://platform.trevol.si/docs/errores).
    /// Ninguna lleva la llave ni las cabeceras de la PETICIÓN.
    /// </summary>
    public class TrevolException : Exception
    {
        public TrevolException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null,
            string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null,
            string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null,
            IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null)
            : base(message, innerException)
        {
            Status = status;
            Code = code;
            Type = type;
            Title = title;
            Detail = detail;
            RequestId = requestId;
            Retryable = retryable;
            RetryAfter = retryAfter;
            Errors = errors ?? Array.Empty<FieldError>();
            IdempotencyKey = idempotencyKey;
            Problem = problem;
            Headers = headers ?? new Dictionary<string, IReadOnlyList<string>>();
        }

        /// <summary>El estado HTTP (null si no hubo respuesta).</summary>
        public int? Status { get; }
        /// <summary>El <c>code</c> del problem+json.</summary>
        public string? Code { get; }
        public string? Type { get; }
        public string? Title { get; }
        public string? Detail { get; }
        public string? RequestId { get; }
        public bool? Retryable { get; }
        /// <summary>Segundos que pidió la API esperar (cabecera <c>Retry-After</c> o <c>retry_after</c> del cuerpo).</summary>
        public double? RetryAfter { get; }
        /// <summary>Las <c>errors[]</c> de un 422.</summary>
        public IReadOnlyList<FieldError> Errors { get; }
        /// <summary>La Idempotency-Key de la llamada: úsala para reintentar más tarde con la MISMA clave.</summary>
        public string? IdempotencyKey { get; }
        /// <summary>El cuerpo problem+json completo, con los campos extra (<c>existing_id</c>, <c>reason</c>, <c>document</c>, <c>rules</c>…).</summary>
        public IReadOnlyDictionary<string, object?>? Problem { get; }
        /// <summary>Las cabeceras de la RESPUESTA.</summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; }

        /// <summary>Clase, code, estado, detalle y request_id; nunca la llave ni cabeceras de la petición.</summary>
        public override string ToString()
        {
            var partes = new List<string> { GetType().Name };
            if (Code != null) partes.Add(Code);
            if (Status != null) partes.Add("HTTP " + Status);
            var cola = RequestId != null ? $" [request_id={RequestId}]" : "";
            return $"{string.Join(" ", partes)}: {Detail ?? Title ?? Message}{cola}";
        }
    }

    internal static class Excepcion
    {
        internal static string Describir(Exception e) => e.Message.Length > 200 ? e.Message.Substring(0, 200) : e.Message;
    }

    // Las 13 clases por code (docs/api/43 §2.6.5). Cada una pasa todo a la base.
    public class AuthenticationException : TrevolException { public AuthenticationException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class PermissionDeniedException : TrevolException { public PermissionDeniedException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class NotFoundException : TrevolException { public NotFoundException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class InvalidRequestException : TrevolException { public InvalidRequestException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class IdempotencyException : TrevolException { public IdempotencyException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    /// <summary>422 <c>validation_error</c>: <see cref="TrevolException.Errors"/> dice qué campo (<c>Pointer</c>) y por qué.</summary>
    public class ValidationException : TrevolException { public ValidationException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class ConflictException : TrevolException { public ConflictException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class PreconditionException : TrevolException { public PreconditionException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    /// <summary>422 <c>dian_rejected</c>: <c>Problem["document"]</c> trae el documento y <c>Problem["rules"]</c> las reglas que la DIAN aplicó.</summary>
    public class DianRejectedException : TrevolException { public DianRejectedException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class BalanceException : TrevolException { public BalanceException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class RateLimitException : TrevolException { public RateLimitException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class DianUnavailableException : TrevolException { public DianUnavailableException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }
    public class InternalServerException : TrevolException { public InternalServerException(string message, int? status = null, string? code = null, string? type = null, string? title = null, string? detail = null, string? requestId = null, bool? retryable = null, double? retryAfter = null, IReadOnlyList<FieldError>? errors = null, string? idempotencyKey = null, IReadOnlyDictionary<string, object?>? problem = null, IReadOnlyDictionary<string, IReadOnlyList<string>>? headers = null, Exception? innerException = null) : base(message, status, code, type, title, detail, requestId, retryable, retryAfter, errors, idempotencyKey, problem, headers, innerException) { } }

    /// <summary>No hubo respuesta: red, DNS, TLS o tiempo agotado. Lleva la Idempotency-Key para reintentar sin duplicar.</summary>
    public class ApiConnectionException : TrevolException
    {
        public ApiConnectionException(string message, string? idempotencyKey = null, Exception? innerException = null)
            : base(message, idempotencyKey: idempotencyKey, innerException: innerException) { }
    }

    /// <summary>Mal uso del SDK, antes de llamar: llave mal formada, entorno que no coincide. Nunca incluye la llave.</summary>
    public class ConfigurationException : TrevolException
    {
        public ConfigurationException(string message) : base(message) { }
    }

    /// <summary>La firma, la fecha o las cabeceras de un webhook no son válidas. <see cref="Reason"/>: «firma», «fecha», «cabeceras» o «cuerpo».</summary>
    public class WebhookVerificationException : TrevolException
    {
        public WebhookVerificationException(string message, string reason) : base(message) { Reason = reason; }

        public string Reason { get; }
    }
}
