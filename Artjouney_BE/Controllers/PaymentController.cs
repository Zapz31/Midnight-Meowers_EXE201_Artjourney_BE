// Artjouney_BE/Controllers/PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Net.payOS.Types; // Or  custom DTOs
using Net.payOS;
using System.Threading.Tasks;
using Helpers.DTOs; // For webhook payload DTO if you create one
using System.IO; // For reading webhook body
using System.Text;
using DAOs; // For reading webhook body
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPayOSService _payOSService;
        private readonly ILogger<PaymentController> _logger; // Add logging
        private readonly ApplicationDbContext _context;


        // Uncomment if you have an order service to manage orders
        // private readonly IOrderService _orderService;

        public PaymentController(IPayOSService payOSService, ILogger<PaymentController> logger, ApplicationDbContext context /*, IOrderService orderService */)
        {
            _payOSService = payOSService;
            _logger = logger;
            _context = context;
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

    var signatureHeader = Request.Headers["x-payos-signature"].FirstOrDefault();
    _logger.LogInformation("Webhook received. Payload: {Payload}, Signature: {Signature}", jsonPayload, signatureHeader);

    if (string.IsNullOrEmpty(signatureHeader))
    {
        _logger.LogWarning("Webhook received without signature.");
        return BadRequest("Missing signature.");
    }

    try
    {
        WebhookData? verifiedData = null;
        try
        {
            var webhookObject = System.Text.Json.JsonSerializer.Deserialize<WebhookType>(
                jsonPayload,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (webhookObject == null)
            {
                _logger.LogWarning("Webhook deserialization failed.");
                return BadRequest("Invalid webhook payload.");
            }

            var payOSClient = new PayOS(
                _payOSService.GetSettings().ClientId,
                _payOSService.GetSettings().ApiKey,
                _payOSService.GetSettings().ChecksumKey);

            verifiedData = payOSClient.verifyPaymentWebhookData(webhookObject);
        }
        catch (Exception verificationEx)
        {
            _logger.LogError(verificationEx, "Webhook signature verification failed.");
            return BadRequest("Webhook signature verification failed.");
        }

        if (verifiedData != null)
        {
            _logger.LogInformation("Webhook verified successfully for orderCode: {OrderCode}, Status: {Status}",
                verifiedData.orderCode, verifiedData.code);

            if (verifiedData.code == "00" && verifiedData.desc == "PAID")
            {
                _logger.LogInformation("Payment successful for order: {OrderCode}", verifiedData.orderCode);

                try
                {
                    // Description format: "<user_id>_<course_id>_<order_code>"
                    var parts = verifiedData.description?.Split('_');

                    if (parts == null || parts.Length != 3)
                    {
                        _logger.LogWarning("Invalid description format: {Description}", verifiedData.description);
                        return BadRequest("Invalid description format.");
                    }

                    long userId = long.Parse(parts[0]);
                    long courseId = long.Parse(parts[1]);
                    long orderCodeFromDescription = long.Parse(parts[2]);

                    if (orderCodeFromDescription != verifiedData.orderCode)
                    {
                        _logger.LogWarning("Mismatch between description orderCode and webhook orderCode.");
                        return BadRequest("Order code mismatch.");
                    }

                    // Prevent duplicate order insert
                    bool exists = await _context.Orders.AnyAsync(o => o.OrderCode == verifiedData.orderCode);
                    if (exists)
                    {
                        _logger.LogInformation("Order already exists: {OrderCode}", verifiedData.orderCode);
                        return Ok(); // Already processed
                    }

                    var order = new BusinessObjects.Models.Order
                    {
                        UserId = userId,
                        CourseId = courseId,
                        UserPremiumInfoId = null,
                        OrderCode = verifiedData.orderCode
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Order inserted to DB: {OrderCode}", verifiedData.orderCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to insert order into database.");
                    return StatusCode(500, "Database insert failed.");
                }
            }

            return Ok(); // Acknowledge webhook
        }
        else
        {
            _logger.LogWarning("Webhook data could not be verified.");
            return BadRequest("Verification failed.");
        }
    }
    catch (JsonException jsonEx)
    {
        _logger.LogError(jsonEx, "JSON parse error.");
        return BadRequest("Invalid JSON.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error.");
        return StatusCode(500, "Internal error.");
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