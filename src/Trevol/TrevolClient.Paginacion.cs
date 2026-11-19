#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trevol.Generado.Model;

namespace Trevol
{
    public sealed partial class TrevolClient
    {
        // Una sobrecarga por sobre de lista: el compilador elige la correcta por lo que devuelve tu lambda y `var f` ya es el modelo.
        // Si la API suma un listado, la prueba de cobertura falla hasta agregar su sobrecarga.
        /// <summary>Recorre todas las páginas de un listado de <c>BalanceTransaction</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<BalanceTransaction> PaginateAsync(Func<string?, Task<BalanceTransactionList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>Document</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<Document> PaginateAsync(Func<string?, Task<CreditNoteList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>CatalogCustomer</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<CatalogCustomer> PaginateAsync(Func<string?, Task<CustomerList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>Document</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<Document> PaginateAsync(Func<string?, Task<DebitNoteList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>DianDocument</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<DianDocument> PaginateAsync(Func<string?, Task<DianDocumentList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>DianSync</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<DianSync> PaginateAsync(Func<string?, Task<DianSyncList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>EquivalentDocumentAdjustment</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<EquivalentDocumentAdjustment> PaginateAsync(Func<string?, Task<EquivalentDocumentAdjustmentList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>EquivalentDocument</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<EquivalentDocument> PaginateAsync(Func<string?, Task<EquivalentDocumentList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>Event</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<Event> PaginateAsync(Func<string?, Task<EventList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>InvoiceBatch</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<InvoiceBatch> PaginateAsync(Func<string?, Task<InvoiceBatchList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>Document</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<Document> PaginateAsync(Func<string?, Task<InvoiceList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>NumberingRange</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<NumberingRange> PaginateAsync(Func<string?, Task<NumberingRangeList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>NumberingRequest</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<NumberingRequest> PaginateAsync(Func<string?, Task<NumberingRequestList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>PaperNumberingRange</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<PaperNumberingRange> PaginateAsync(Func<string?, Task<PaperNumberingRangeList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>PayrollAdjustment</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<PayrollAdjustment> PaginateAsync(Func<string?, Task<PayrollAdjustmentList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>Payroll</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<Payroll> PaginateAsync(Func<string?, Task<PayrollList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>CatalogProduct</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<CatalogProduct> PaginateAsync(Func<string?, Task<ProductList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>RadianEvent</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<RadianEvent> PaginateAsync(Func<string?, Task<RadianEventList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>ReportRun</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<ReportRun> PaginateAsync(Func<string?, Task<ReportRunList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>RequestLog</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<RequestLog> PaginateAsync(Func<string?, Task<RequestList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>SupportDocumentAdjustment</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<SupportDocumentAdjustment> PaginateAsync(Func<string?, Task<SupportDocumentAdjustmentList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>SupportDocument</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<SupportDocument> PaginateAsync(Func<string?, Task<SupportDocumentList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>WebhookDelivery</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<WebhookDelivery> PaginateAsync(Func<string?, Task<WebhookDeliveryList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);

        /// <summary>Recorre todas las páginas de un listado de <c>WebhookEndpoint</c>, perezosamente (<c>await foreach</c>).</summary>
        public IAsyncEnumerable<WebhookEndpoint> PaginateAsync(Func<string?, Task<WebhookEndpointList>> pagina, CancellationToken cancellationToken = default) =>
            Paginacion.Recorrer(pagina, p => p.Data, p => p.HasMore, p => p.NextCursor, d => d.Id, cancellationToken);
    }
}
