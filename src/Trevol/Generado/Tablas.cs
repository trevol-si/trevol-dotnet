// Generado por sdks/herramientas/tablas.mjs desde la especificación. No editar: se borra y se vuelve a escribir.
using System;
using System.Collections.Generic;

namespace Trevol.Generado
{
    public static class Tablas
    {
        public const string Version = "0.1.0";
        public const string ApiVersion = "2026-10-22";

        /// <summary>Una operación de la API. Reintento: «seguro» (GET, preview, PATCH y todo POST/PUT con Idempotency-Key) o «solo_429». Binario: «pdf», «xml», «zip» o null.</summary>
        public sealed class Operacion
        {
            public Operacion(string operationId, string metodo, string ruta, string recurso, bool idempotente, string reintento, string? binario)
            {
                OperationId = operationId; Metodo = metodo; Ruta = ruta; Recurso = recurso; Idempotente = idempotente; Reintento = reintento; Binario = binario;
            }

            public string OperationId { get; }
            public string Metodo { get; }
            public string Ruta { get; }
            public string Recurso { get; }
            public bool Idempotente { get; }
            public string Reintento { get; }
            public string? Binario { get; }
        }

        public static readonly IReadOnlyList<Operacion> Operaciones = new Operacion[]
        {
            new Operacion("correctCreditNote", "PUT", "/credit_notes/{id}", "CreditNotes", true, "seguro", null),
            new Operacion("correctDebitNote", "PUT", "/debit_notes/{id}", "DebitNotes", true, "seguro", null),
            new Operacion("correctEquivalentDocument", "PUT", "/equivalent_documents/{id}", "EquivalentDocuments", true, "seguro", null),
            new Operacion("correctEquivalentDocumentAdjustment", "PUT", "/equivalent_document_adjustments/{id}", "EquivalentDocumentAdjustments", true, "seguro", null),
            new Operacion("correctInvoice", "PUT", "/invoices/{id}", "Invoices", true, "seguro", null),
            new Operacion("correctPayroll", "PUT", "/payrolls/{id}", "Payrolls", true, "seguro", null),
            new Operacion("correctPayrollAdjustment", "PUT", "/payroll_adjustments/{id}", "PayrollAdjustments", true, "seguro", null),
            new Operacion("correctSupportDocument", "PUT", "/support_documents/{id}", "SupportDocuments", true, "seguro", null),
            new Operacion("correctSupportDocumentAdjustment", "PUT", "/support_document_adjustments/{id}", "SupportDocumentAdjustments", true, "seguro", null),
            new Operacion("createBalanceTopUp", "POST", "/balance/top_ups", "Balance", true, "seguro", null),
            new Operacion("createCreditNote", "POST", "/credit_notes", "CreditNotes", true, "seguro", null),
            new Operacion("createDebitNote", "POST", "/debit_notes", "DebitNotes", true, "seguro", null),
            new Operacion("createEquivalentDocument", "POST", "/equivalent_documents", "EquivalentDocuments", true, "seguro", null),
            new Operacion("createEquivalentDocumentAdjustment", "POST", "/equivalent_document_adjustments", "EquivalentDocumentAdjustments", true, "seguro", null),
            new Operacion("createInvoice", "POST", "/invoices", "Invoices", true, "seguro", null),
            new Operacion("createPayroll", "POST", "/payrolls", "Payrolls", true, "seguro", null),
            new Operacion("createPayrollAdjustment", "POST", "/payroll_adjustments", "PayrollAdjustments", true, "seguro", null),
            new Operacion("createRadianEvent", "POST", "/radian_events", "RadianEvents", true, "seguro", null),
            new Operacion("createSupportDocument", "POST", "/support_documents", "SupportDocuments", true, "seguro", null),
            new Operacion("createSupportDocumentAdjustment", "POST", "/support_document_adjustments", "SupportDocumentAdjustments", true, "seguro", null),
            new Operacion("createWebhookEndpoint", "POST", "/webhook_endpoints", "WebhookEndpoints", false, "solo_429", null),
            new Operacion("deleteWebhookEndpoint", "DELETE", "/webhook_endpoints/{id}", "WebhookEndpoints", false, "solo_429", null),
            new Operacion("downloadCreditNoteDianResponse", "GET", "/credit_notes/{id}/dian_response", "CreditNotes", false, "seguro", "xml"),
            new Operacion("downloadCreditNotePdf", "GET", "/credit_notes/{id}/pdf", "CreditNotes", false, "seguro", "pdf"),
            new Operacion("downloadCreditNoteXml", "GET", "/credit_notes/{id}/xml", "CreditNotes", false, "seguro", "xml"),
            new Operacion("downloadCreditNoteZip", "GET", "/credit_notes/{id}/zip", "CreditNotes", false, "seguro", "zip"),
            new Operacion("downloadDebitNoteDianResponse", "GET", "/debit_notes/{id}/dian_response", "DebitNotes", false, "seguro", "xml"),
            new Operacion("downloadDebitNotePdf", "GET", "/debit_notes/{id}/pdf", "DebitNotes", false, "seguro", "pdf"),
            new Operacion("downloadDebitNoteXml", "GET", "/debit_notes/{id}/xml", "DebitNotes", false, "seguro", "xml"),
            new Operacion("downloadDebitNoteZip", "GET", "/debit_notes/{id}/zip", "DebitNotes", false, "seguro", "zip"),
            new Operacion("downloadEquivalentDocumentAdjustmentDianResponse", "GET", "/equivalent_document_adjustments/{id}/dian_response", "EquivalentDocumentAdjustments", false, "seguro", "xml"),
            new Operacion("downloadEquivalentDocumentAdjustmentPdf", "GET", "/equivalent_document_adjustments/{id}/pdf", "EquivalentDocumentAdjustments", false, "seguro", "pdf"),
            new Operacion("downloadEquivalentDocumentAdjustmentXml", "GET", "/equivalent_document_adjustments/{id}/xml", "EquivalentDocumentAdjustments", false, "seguro", "xml"),
            new Operacion("downloadEquivalentDocumentAdjustmentZip", "GET", "/equivalent_document_adjustments/{id}/zip", "EquivalentDocumentAdjustments", false, "seguro", "zip"),
            new Operacion("downloadEquivalentDocumentDianResponse", "GET", "/equivalent_documents/{id}/dian_response", "EquivalentDocuments", false, "seguro", "xml"),
            new Operacion("downloadEquivalentDocumentPdf", "GET", "/equivalent_documents/{id}/pdf", "EquivalentDocuments", false, "seguro", "pdf"),
            new Operacion("downloadEquivalentDocumentXml", "GET", "/equivalent_documents/{id}/xml", "EquivalentDocuments", false, "seguro", "xml"),
            new Operacion("downloadEquivalentDocumentZip", "GET", "/equivalent_documents/{id}/zip", "EquivalentDocuments", false, "seguro", "zip"),
            new Operacion("downloadInvoiceDianResponse", "GET", "/invoices/{id}/dian_response", "Invoices", false, "seguro", "xml"),
            new Operacion("downloadInvoicePdf", "GET", "/invoices/{id}/pdf", "Invoices", false, "seguro", "pdf"),
            new Operacion("downloadInvoiceXml", "GET", "/invoices/{id}/xml", "Invoices", false, "seguro", "xml"),
            new Operacion("downloadInvoiceZip", "GET", "/invoices/{id}/zip", "Invoices", false, "seguro", "zip"),
            new Operacion("downloadPayrollAdjustmentDianResponse", "GET", "/payroll_adjustments/{id}/dian_response", "PayrollAdjustments", false, "seguro", "xml"),
            new Operacion("downloadPayrollAdjustmentXml", "GET", "/payroll_adjustments/{id}/xml", "PayrollAdjustments", false, "seguro", "xml"),
            new Operacion("downloadPayrollAdjustmentZip", "GET", "/payroll_adjustments/{id}/zip", "PayrollAdjustments", false, "seguro", "zip"),
            new Operacion("downloadPayrollDianResponse", "GET", "/payrolls/{id}/dian_response", "Payrolls", false, "seguro", "xml"),
            new Operacion("downloadPayrollXml", "GET", "/payrolls/{id}/xml", "Payrolls", false, "seguro", "xml"),
            new Operacion("downloadPayrollZip", "GET", "/payrolls/{id}/zip", "Payrolls", false, "seguro", "zip"),
            new Operacion("downloadRadianEventXml", "GET", "/radian_events/{id}/xml", "RadianEvents", false, "seguro", "xml"),
            new Operacion("downloadSupportDocumentAdjustmentDianResponse", "GET", "/support_document_adjustments/{id}/dian_response", "SupportDocumentAdjustments", false, "seguro", "xml"),
            new Operacion("downloadSupportDocumentAdjustmentXml", "GET", "/support_document_adjustments/{id}/xml", "SupportDocumentAdjustments", false, "seguro", "xml"),
            new Operacion("downloadSupportDocumentDianResponse", "GET", "/support_documents/{id}/dian_response", "SupportDocuments", false, "seguro", "xml"),
            new Operacion("downloadSupportDocumentXml", "GET", "/support_documents/{id}/xml", "SupportDocuments", false, "seguro", "xml"),
            new Operacion("getHealth", "GET", "/health", "General", false, "seguro", null),
            new Operacion("listBalanceTransactions", "GET", "/balance/transactions", "Balance", false, "seguro", null),
            new Operacion("listCreditNotes", "GET", "/credit_notes", "CreditNotes", false, "seguro", null),
            new Operacion("listDebitNotes", "GET", "/debit_notes", "DebitNotes", false, "seguro", null),
            new Operacion("listEquivalentDocumentAdjustments", "GET", "/equivalent_document_adjustments", "EquivalentDocumentAdjustments", false, "seguro", null),
            new Operacion("listEquivalentDocuments", "GET", "/equivalent_documents", "EquivalentDocuments", false, "seguro", null),
            new Operacion("listEvents", "GET", "/events", "Events", false, "seguro", null),
            new Operacion("listInvoices", "GET", "/invoices", "Invoices", false, "seguro", null),
            new Operacion("listPayrollAdjustments", "GET", "/payroll_adjustments", "PayrollAdjustments", false, "seguro", null),
            new Operacion("listPayrolls", "GET", "/payrolls", "Payrolls", false, "seguro", null),
            new Operacion("listRadianEvents", "GET", "/radian_events", "RadianEvents", false, "seguro", null),
            new Operacion("listRequests", "GET", "/requests", "Requests", false, "seguro", null),
            new Operacion("listSupportDocumentAdjustments", "GET", "/support_document_adjustments", "SupportDocumentAdjustments", false, "seguro", null),
            new Operacion("listSupportDocuments", "GET", "/support_documents", "SupportDocuments", false, "seguro", null),
            new Operacion("listWebhookEndpointDeliveries", "GET", "/webhook_endpoints/{id}/deliveries", "WebhookEndpoints", false, "seguro", null),
            new Operacion("listWebhookEndpoints", "GET", "/webhook_endpoints", "WebhookEndpoints", false, "seguro", null),
            new Operacion("previewCreditNote", "POST", "/credit_notes/preview", "CreditNotes", false, "seguro", null),
            new Operacion("previewDebitNote", "POST", "/debit_notes/preview", "DebitNotes", false, "seguro", null),
            new Operacion("previewEquivalentDocument", "POST", "/equivalent_documents/preview", "EquivalentDocuments", false, "seguro", null),
            new Operacion("previewEquivalentDocumentAdjustment", "POST", "/equivalent_document_adjustments/preview", "EquivalentDocumentAdjustments", false, "seguro", null),
            new Operacion("previewInvoice", "POST", "/invoices/preview", "Invoices", false, "seguro", null),
            new Operacion("previewPayroll", "POST", "/payrolls/preview", "Payrolls", false, "seguro", null),
            new Operacion("previewPayrollAdjustment", "POST", "/payroll_adjustments/preview", "PayrollAdjustments", false, "seguro", null),
            new Operacion("previewSupportDocument", "POST", "/support_documents/preview", "SupportDocuments", false, "seguro", null),
            new Operacion("previewSupportDocumentAdjustment", "POST", "/support_document_adjustments/preview", "SupportDocumentAdjustments", false, "seguro", null),
            new Operacion("resendEvent", "POST", "/events/{id}/resend", "Events", false, "solo_429", null),
            new Operacion("retrieveBalance", "GET", "/balance", "Balance", false, "seguro", null),
            new Operacion("retrieveCompany", "GET", "/company", "General", false, "seguro", null),
            new Operacion("retrieveCreditNote", "GET", "/credit_notes/{id}", "CreditNotes", false, "seguro", null),
            new Operacion("retrieveDebitNote", "GET", "/debit_notes/{id}", "DebitNotes", false, "seguro", null),
            new Operacion("retrieveDianAcquirer", "GET", "/dian/acquirer", "General", false, "seguro", null),
            new Operacion("retrieveEquivalentDocument", "GET", "/equivalent_documents/{id}", "EquivalentDocuments", false, "seguro", null),
            new Operacion("retrieveEquivalentDocumentAdjustment", "GET", "/equivalent_document_adjustments/{id}", "EquivalentDocumentAdjustments", false, "seguro", null),
            new Operacion("retrieveEvent", "GET", "/events/{id}", "Events", false, "seguro", null),
            new Operacion("retrieveInvoice", "GET", "/invoices/{id}", "Invoices", false, "seguro", null),
            new Operacion("retrieveInvoiceTacitAcceptance", "GET", "/invoices/{id}/tacit_acceptance", "Invoices", false, "seguro", null),
            new Operacion("retrievePayroll", "GET", "/payrolls/{id}", "Payrolls", false, "seguro", null),
            new Operacion("retrievePayrollAdjustment", "GET", "/payroll_adjustments/{id}", "PayrollAdjustments", false, "seguro", null),
            new Operacion("retrieveRadianEvent", "GET", "/radian_events/{id}", "RadianEvents", false, "seguro", null),
            new Operacion("retrieveSupportDocument", "GET", "/support_documents/{id}", "SupportDocuments", false, "seguro", null),
            new Operacion("retrieveSupportDocumentAdjustment", "GET", "/support_document_adjustments/{id}", "SupportDocumentAdjustments", false, "seguro", null),
            new Operacion("retrieveTacitAcceptanceByCufe", "GET", "/radian_events/tacit_acceptance", "RadianEvents", false, "seguro", null),
            new Operacion("retrieveWebhookEndpoint", "GET", "/webhook_endpoints/{id}", "WebhookEndpoints", false, "seguro", null),
            new Operacion("rotateWebhookEndpointSecret", "POST", "/webhook_endpoints/{id}/rotate_secret", "WebhookEndpoints", false, "solo_429", null),
            new Operacion("sendCreditNote", "POST", "/credit_notes/{id}/send", "CreditNotes", false, "solo_429", null),
            new Operacion("sendDebitNote", "POST", "/debit_notes/{id}/send", "DebitNotes", false, "solo_429", null),
            new Operacion("sendEquivalentDocument", "POST", "/equivalent_documents/{id}/send", "EquivalentDocuments", false, "solo_429", null),
            new Operacion("sendEquivalentDocumentAdjustment", "POST", "/equivalent_document_adjustments/{id}/send", "EquivalentDocumentAdjustments", false, "solo_429", null),
            new Operacion("sendInvoice", "POST", "/invoices/{id}/send", "Invoices", false, "solo_429", null),
            new Operacion("sendPayroll", "POST", "/payrolls/{id}/send", "Payrolls", false, "solo_429", null),
            new Operacion("sendPayrollAdjustment", "POST", "/payroll_adjustments/{id}/send", "PayrollAdjustments", false, "solo_429", null),
            new Operacion("sendRadianEvent", "POST", "/radian_events/{id}/send", "RadianEvents", false, "solo_429", null),
            new Operacion("sendSupportDocument", "POST", "/support_documents/{id}/send", "SupportDocuments", false, "solo_429", null),
            new Operacion("sendSupportDocumentAdjustment", "POST", "/support_document_adjustments/{id}/send", "SupportDocumentAdjustments", false, "solo_429", null),
            new Operacion("testWebhookEndpoint", "POST", "/webhook_endpoints/{id}/test", "WebhookEndpoints", false, "solo_429", null),
            new Operacion("updateWebhookEndpoint", "PATCH", "/webhook_endpoints/{id}", "WebhookEndpoints", false, "seguro", null)
        };

        /// <summary>El nombre de cada etiqueta del SDK (una propiedad del cliente por etiqueta).</summary>
        public static readonly IReadOnlyList<string> Etiquetas = new string[] { "Balance", "CreditNotes", "DebitNotes", "EquivalentDocumentAdjustments", "EquivalentDocuments", "Events", "General", "Invoices", "PayrollAdjustments", "Payrolls", "RadianEvents", "Requests", "SupportDocumentAdjustments", "SupportDocuments", "WebhookEndpoints" };

        /// <summary>code de problem+json -> clase de error.</summary>
        public static readonly IReadOnlyDictionary<string, string> ClasePorCode = new Dictionary<string, string>
        {
            ["authentication_required"] = "AuthenticationError",
            ["invalid_api_key"] = "AuthenticationError",
            ["permission_denied"] = "PermissionDeniedError",
            ["not_found"] = "NotFoundError",
            ["method_not_allowed"] = "InvalidRequestError",
            ["unsupported_version"] = "InvalidRequestError",
            ["invalid_json"] = "InvalidRequestError",
            ["invalid_request"] = "InvalidRequestError",
            ["idempotency_key_required"] = "InvalidRequestError",
            ["idempotency_key_reused"] = "IdempotencyError",
            ["validation_error"] = "ValidationError",
            ["external_id_in_use"] = "ConflictError",
            ["invalid_state"] = "ConflictError",
            ["duplicate_document"] = "ConflictError",
            ["request_in_progress"] = "ConflictError",
            ["previous_event_pending"] = "ConflictError",
            ["file_unavailable"] = "ConflictError",
            ["webhook_limit_reached"] = "ConflictError",
            ["company_not_enabled"] = "PreconditionError",
            ["numbering_unavailable"] = "PreconditionError",
            ["certificate_problem"] = "PreconditionError",
            ["tacit_acceptance_not_due"] = "PreconditionError",
            ["dian_rejected"] = "DianRejectedError",
            ["insufficient_balance"] = "BalanceError",
            ["top_up_below_minimum"] = "BalanceError",
            ["top_up_above_maximum"] = "BalanceError",
            ["top_ups_unavailable"] = "BalanceError",
            ["rate_limited"] = "RateLimitError",
            ["dian_unavailable"] = "DianUnavailableError",
            ["internal_error"] = "InternalServerError"
        };

        /// <summary>Estado HTTP -> clase de error (cuando no hay un code conocido; 5xx en general).</summary>
        public static readonly IReadOnlyDictionary<string, string> ClasePorEstado = new Dictionary<string, string>
        {
            ["400"] = "InvalidRequestError",
            ["401"] = "AuthenticationError",
            ["402"] = "BalanceError",
            ["403"] = "PermissionDeniedError",
            ["404"] = "NotFoundError",
            ["409"] = "ConflictError",
            ["422"] = "ValidationError",
            ["429"] = "RateLimitError",
            ["5xx"] = "InternalServerError"
        };

        /// <summary>Event.type -> el modelo de su data.</summary>
        public static readonly IReadOnlyDictionary<string, Type> EventoModelos = new Dictionary<string, Type>
        {
            ["balance.low"] = typeof(Model.BalanceEventData),
            ["certificate.expiring"] = typeof(Model.CertificateEventData),
            ["credit_note.accepted"] = typeof(Model.DocumentEventData),
            ["credit_note.delivered"] = typeof(Model.DocumentEventData),
            ["credit_note.email_bounced"] = typeof(Model.DocumentEventData),
            ["credit_note.email_failed"] = typeof(Model.DocumentEventData),
            ["credit_note.error"] = typeof(Model.DocumentEventData),
            ["credit_note.rejected"] = typeof(Model.DocumentEventData),
            ["debit_note.accepted"] = typeof(Model.DocumentEventData),
            ["debit_note.delivered"] = typeof(Model.DocumentEventData),
            ["debit_note.email_bounced"] = typeof(Model.DocumentEventData),
            ["debit_note.email_failed"] = typeof(Model.DocumentEventData),
            ["debit_note.error"] = typeof(Model.DocumentEventData),
            ["debit_note.rejected"] = typeof(Model.DocumentEventData),
            ["equivalent_document.accepted"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document.delivered"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document.email_bounced"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document.email_failed"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document.error"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document.rejected"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document_adjustment.accepted"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document_adjustment.delivered"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document_adjustment.email_bounced"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document_adjustment.email_failed"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document_adjustment.error"] = typeof(Model.EquivalentDocumentEventData),
            ["equivalent_document_adjustment.rejected"] = typeof(Model.EquivalentDocumentEventData),
            ["invoice.accepted"] = typeof(Model.DocumentEventData),
            ["invoice.delivered"] = typeof(Model.DocumentEventData),
            ["invoice.email_bounced"] = typeof(Model.DocumentEventData),
            ["invoice.email_failed"] = typeof(Model.DocumentEventData),
            ["invoice.error"] = typeof(Model.DocumentEventData),
            ["invoice.rejected"] = typeof(Model.DocumentEventData),
            ["numbering.expiring"] = typeof(Model.NumberingExpiringEventData),
            ["numbering.low"] = typeof(Model.NumberingLowEventData),
            ["payroll.accepted"] = typeof(Model.PayrollEventData),
            ["payroll.error"] = typeof(Model.PayrollEventData),
            ["payroll.rejected"] = typeof(Model.PayrollEventData),
            ["payroll_adjustment.accepted"] = typeof(Model.PayrollEventData),
            ["payroll_adjustment.error"] = typeof(Model.PayrollEventData),
            ["payroll_adjustment.rejected"] = typeof(Model.PayrollEventData),
            ["radian_event.accepted"] = typeof(Model.RadianEventData),
            ["radian_event.error"] = typeof(Model.RadianEventData),
            ["radian_event.rejected"] = typeof(Model.RadianEventData),
            ["support_document.accepted"] = typeof(Model.SupportDocumentEventData),
            ["support_document.error"] = typeof(Model.SupportDocumentEventData),
            ["support_document.rejected"] = typeof(Model.SupportDocumentEventData),
            ["support_document_adjustment.accepted"] = typeof(Model.SupportDocumentEventData),
            ["support_document_adjustment.error"] = typeof(Model.SupportDocumentEventData),
            ["support_document_adjustment.rejected"] = typeof(Model.SupportDocumentEventData),
            ["webhook.test"] = typeof(Model.TestEventData)
        };
    }
}
