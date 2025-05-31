// Services/Implements/PayOSService.cs
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using Services.Interfaces;
using Helpers.HelperClasses; 
using System.Threading.Tasks;
using System.Security.Cryptography; // For webhook signature verification if manual
using System.Text; // For webhook signature verification if manual

namespace Services.Implements
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;
        private readonly PayOSConfig _PayOSConfig; // Store for checksum key if needed separately
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(IOptions<PayOSConfig> PayOSConfig, ILogger<PayOSService> logger)
        {
            _PayOSConfig = PayOSConfig.Value; // _PayOSConfig is initialized here
            _logger = logger;
            // Validate configuration

            if (string.IsNullOrEmpty(_PayOSConfig.ClientId) ||
                string.IsNullOrEmpty(_PayOSConfig.ApiKey) ||
                string.IsNullOrEmpty(_PayOSConfig.ChecksumKey))
            {
                _logger.LogError("PayOS ClientId, ApiKey, or ChecksumKey is missing from configuration!");

            }

            _payOS = new PayOS(_PayOSConfig.ClientId, _PayOSConfig.ApiKey, _PayOSConfig.ChecksumKey);
        }

        public async Task<CreatePaymentResult?> CreatePaymentLinkAsync(PaymentData paymentData)
        {
            try
            {
                return await _payOS.createPaymentLink(paymentData);
            }
            catch (System.Exception ex)
            {
                // Log the exception (ex.Message, ex.StackTrace)
                Console.WriteLine($"Error creating payment link: {ex.Message}");
                return null;
            }
        }

        public async Task<PaymentLinkInformation?> GetPaymentLinkInformationAsync(long orderCode)
        {
            try
            {
                return await _payOS.getPaymentLinkInformation(orderCode);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error getting payment link info for order {orderCode}: {ex.Message}");
                return null;
            }
        }

        public async Task<PaymentLinkInformation?> CancelPaymentLinkAsync(long orderCode, string? cancellationReason = null)
        {
            try
            {
                return await _payOS.cancelPaymentLink(orderCode, cancellationReason);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error cancelling payment link for order {orderCode}: {ex.Message}"); 
                return null;
            }
        }

        public bool VerifyWebhookSignature(string jsonPayload, string signatureHeader)
        {
            
            var keyBytes = Encoding.UTF8.GetBytes(_PayOSConfig.ChecksumKey);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var dataBytes = Encoding.UTF8.GetBytes(jsonPayload);
                var computedHash = hmac.ComputeHash(dataBytes);
                var computedHashString = BitConverter.ToString(computedHash).Replace("-", "").ToLower();
                return computedHashString.Equals(signatureHeader.ToLower());
            }
        }

        public PayOSConfig GetSettings()
        {
            return _PayOSConfig;
        }
    }
}