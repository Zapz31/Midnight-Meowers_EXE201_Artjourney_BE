// Artjouney_BE/Controllers/PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Net.payOS.Types; // Or  custom DTOs
using Net.payOS;
using System.Threading.Tasks;
using Helpers.DTOs; // For webhook payload DTO if you create one
using System.IO; // For reading webhook body
using System.Text; // For reading webhook body

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPayOSService _payOSService;
        private readonly ILogger<PaymentController> _logger; // Add logging

        // Uncomment if you have an order service to manage orders
        // private readonly IOrderService _orderService;

        public PaymentController(IPayOSService payOSService, ILogger<PaymentController> logger /*, IOrderService orderService */)
        {
            _payOSService = payOSService;
            _logger = logger;
            // _orderService = orderService;
        }

        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] OrderCreationDto orderRequest) // Replace OrderCreationDto with  actual request model
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Potentially create or retrieve  order from  system
            // long InternalOrderCode = await _orderService.CreateOrderAsync(orderRequest);
            // decimal amount = await _orderService.GetOrderAmountAsync(InternalOrderCode);
            // List<ItemData> items = await _orderService.GetOrderItemsAsync(InternalOrderCode);

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Use system's unique order identifier
            int amount = orderRequest.Amount; // Example: Get amount from request
            string description = $"Payment for {orderCode}"; // Customize, 25 characters max

            List<ItemData> items = new List<ItemData>(); // Populate from orderRequest or  order service
            if (orderRequest.Items != null)
            {
                foreach (var itemDto in orderRequest.Items)
                {
                    items.Add(new ItemData(itemDto.Name, itemDto.Quantity, itemDto.Price));
                }
            }


            
            // For local development, might need to use a tunneling service like ngrok.
            string cancelUrl = "https://-frontend-domain.com/payment/cancel"; //  frontend cancel URL
            string returnUrl = "https://-frontend-domain.com/payment/success"; //  frontend success/return URL

            var paymentData = new PaymentData(orderCode, amount, description, items, cancelUrl, returnUrl)
            {
                // Optional: buyerName, buyerEmail, buyerPhone, buyerAddress
                buyerName = orderRequest.BuyerName,
                buyerEmail = orderRequest.BuyerEmail,
                // buyerPhone = orderRequest.BuyerPhone
            };

            var paymentResult = await _payOSService.CreatePaymentLinkAsync(paymentData);

            if (paymentResult == null)
            {
                _logger.LogError("Failed to create payment link for order code {OrderCode}", orderCode);
                return StatusCode(500, "Failed to create payment link.");
            }

            _logger.LogInformation("Payment link created for order code {OrderCode}. PaymentLinkID: {PaymentLinkId}", orderCode, paymentResult.paymentLinkId);
            

            return Ok(paymentResult); // Send checkoutUrl, paymentLinkId, etc., to the client
        }
        
        [HttpGet("payment-return")] 
        public async Task<IActionResult> PaymentReturn(
            [FromQuery] string code,
            [FromQuery] string id, // paymentLinkId
            [FromQuery] bool cancel,
            [FromQuery] string status,
            [FromQuery(Name = "orderCode")] long orderCode // Ensure query param name matches if different
        )
        {
            _logger.LogInformation("Payment return received for orderCode: {OrderCode}, paymentLinkId: {PaymentLinkId}, status: {Status}, cancelled: {Cancel}",
                                   orderCode, id, status, cancel);

            // IMPORTANT: The 'status' on return URL is for user experience.
            // ALWAYS verify the transaction status with payOS backend (via getPaymentLinkInformation or webhook)
            // before confirming the order or providing services.

            var paymentInfo = await _payOSService.GetPaymentLinkInformationAsync(orderCode);

            if (paymentInfo == null)
            {
                _logger.LogWarning("Could not retrieve payment info for orderCode {OrderCode} on return.", orderCode);
                // Redirect to a generic error/pending page on  frontend
                return Redirect($"https://-frontend-domain.com/payment/pending?orderCode={orderCode}&error=retrieval_failed");
            }

            // Process paymentInfo.status (PAID, PENDING, CANCELLED, EXPIRED, etc.)
            

            _logger.LogInformation("Verified payment status for orderCode {OrderCode}: {VerifiedStatus}", orderCode, paymentInfo.status);

            // Redirect user to appropriate frontend page
            if (paymentInfo.status == "PAID")
            {
                return Redirect($"https://-frontend-domain.com/payment/confirmation?orderCode={orderCode}&status=success");
            }
            else if (paymentInfo.status == "CANCELLED")
            {
                return Redirect($"https://-frontend-domain.com/payment/confirmation?orderCode={orderCode}&status=cancelled");
            }
            else
            {
                return Redirect($"https://-frontend-domain.com/payment/confirmation?orderCode={orderCode}&status={paymentInfo.status.ToLower()}");
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PayOSWebhook()
        {
            string jsonPayload;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                jsonPayload = await reader.ReadToEndAsync();
            }

            var signatureHeader = Request.Headers["x-payos-signature"].FirstOrDefault(); // Or whatever header payOS uses
            _logger.LogInformation("Webhook received. Payload: {Payload}, Signature: {Signature}", jsonPayload, signatureHeader);


            if (string.IsNullOrEmpty(signatureHeader))
            {
                _logger.LogWarning("Webhook received without signature.");
                return BadRequest("Missing signature.");
            }

           
            try
            {
                
                
                WebhookData? verifiedData = null; // This is a type from Net.payOS.Types
                try
                {
                    
                    var webhookObject = System.Text.Json.JsonSerializer.Deserialize<WebhookType>(jsonPayload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (webhookObject == null)
                    {
                        _logger.LogWarning("Webhook deserialization failed.");
                        return BadRequest("Invalid webhook payload.");
                    }

                    // The verifyPaymentWebhookData is on the PayOS instance itself
                    PayOS payOSClient = new PayOS(_payOSService.GetSettings().ClientId, _payOSService.GetSettings().ApiKey, _payOSService.GetSettings().ChecksumKey); // Temporary, inject PayOS directly or use settings from service
                    verifiedData = payOSClient.verifyPaymentWebhookData(webhookObject); // This should use the checksum key internally
                }
                catch (System.Exception verificationEx)
                {
                    _logger.LogError(verificationEx, "Webhook signature verification failed.");
                    return BadRequest("Webhook signature verification failed.");
                }


                if (verifiedData != null)
                {
                    _logger.LogInformation("Webhook verified successfully for orderCode: {OrderCode}, Status: {Status}", verifiedData.orderCode, verifiedData.code);
                    // Process the verified webhook data
                    // e.g., await _orderService.UpdateOrderStatusBasedOnWebhookAsync(verifiedData);
                    

                    // Example: If payment is successful
                    if (verifiedData.code == "00" && verifiedData.desc == "PAID") // Check payOS docs for actual success codes/descriptions
                    {
                        // Handle successful payment logic
                        _logger.LogInformation("Payment successful for order: {OrderCode}", verifiedData.orderCode);
                    }
                    else
                    {
                        _logger.LogInformation("Payment status for order {OrderCode}: {Status} - {Description}", verifiedData.orderCode, verifiedData.code, verifiedData.desc);
                    }

                    return Ok(); // Important to return 2xx to payOS to acknowledge receipt
                }
                else
                {
                    _logger.LogWarning("Webhook data could not be verified (verifiedData is null).");
                    return BadRequest("Webhook verification failed.");
                }
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error deserializing webhook payload.");
                return BadRequest("Invalid JSON payload.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook.");
                return StatusCode(500, "Internal server error during webhook processing.");
            }
        }
    }

    //  DTO for the create-payment-link endpoint 
    public class OrderCreationDto
    {
        public int Amount { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

    }

    public class OrderItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}