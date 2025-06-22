// Services/Interfaces/IPayOSService.cs
using Net.payOS.Types; // Add this if you're using types like CreatePaymentResult, PaymentLinkInformation from the SDK directly in the interface
using Helpers.DTOs; // If you create custom DTOs for requests/responses
using Helpers.HelperClasses;

namespace Services.Interfaces
{
    public interface IPayOSService
    {
        Task<CreatePaymentResult?> CreatePaymentLinkAsync(PaymentData paymentData); // Using SDK's PaymentData and CreatePaymentResult
        // Or using custom DTOs:
        // Task<CustomPaymentLinkResponseDto?> CreatePaymentLinkAsync(CustomPaymentLinkRequestDto request);

        Task<PaymentLinkInformation?> GetPaymentLinkInformationAsync(long orderCode);
        public WebhookData VerifyPaymentWebhookData(WebhookType webhookBody);
        Task<PaymentLinkInformation?> CancelPaymentLinkAsync(long orderCode, string? cancellationReason = null);
        bool VerifyWebhookSignature(string jsonPayload, string signatureHeader); // Or handle webhook verification internally
        // Add methods for handling webhook data processing if needed
        PayOSConfig GetSettings();
    }
}