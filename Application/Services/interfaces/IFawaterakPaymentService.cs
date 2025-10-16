using Application.DTOs.Payment;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.Payment.EInvoiceResponseModel;


namespace Application.Services.interfaces
{

    public interface IFawaterakPaymentService
    {
        // Create EInvoice Link
        Task<EInvoiceResponseDataModel?> CreateEInvoiceAsync(EInvoiceRequestModel eInvoice);

        // Payment Integration
        Task<IList<PaymentMethodsResponse.PaymentMethod>?> GetPaymentMethods();
        Task<BasePaymentDataResponse?> GeneralPay(EInvoiceRequestModel invoice);

        // WebHook Verification
        bool VerifyWebhook(WebHookModel webHook);
        bool VerifyCancelTransaction(CancelTransactionModel cancelTransaction);
        bool VerifyApiKeyTransaction(string apiKey);

        // HashKey
        string GenerateHashKeyForIFrame(string domain);
    }
}
