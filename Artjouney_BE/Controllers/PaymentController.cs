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
using Services.Implements;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.Models;
using Repositories.Interfaces;
using Helpers.DTOs.UserCourseInfo;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPayOSService _payOSService;
        private readonly ILogger<PaymentController> _logger; // Add logging
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserCourseInfoService _userCourseInfoService;


        // Uncomment if you have an order service to manage orders
        // private readonly IOrderService _orderService;

        public PaymentController(IPayOSService payOSService, ILogger<PaymentController> logger, 
            ICurrentUserService currentUserService,
            IOrderRepository orderRepository,
            ApplicationDbContext context /*, IOrderService orderService */, 
            IUserCourseInfoService userCourseInfoService)
        {
            _payOSService = payOSService;
            _logger = logger;
            _context = context;
            _currentUserService = currentUserService;
            _orderRepository = orderRepository;
            _userCourseInfoService = userCourseInfoService;
            // _orderService = orderService;
        }

        [HttpPost("create-payment-link")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            if (string.IsNullOrWhiteSpace(orderRequest.Description) || !(("thanh toan khoa hoc".Equals(orderRequest.Description)) || ("thanh toan premium".Equals(orderRequest.Description))))
            {
                return StatusCode(400, "Invalid description");
            }
            var userId = _currentUserService.AccountId;

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Use system's unique order identifier
            //int amount = orderRequest.Amount; // Example: Get amount from request
            //string description = $"Payment for {orderCode}"; // Customize, 25 characters max

            List<ItemData> items = new List<ItemData>(); // Populate from orderRequest or  order service
            if (orderRequest.Items != null)
            {
                foreach (var itemDto in orderRequest.Items)
                {
                    items.Add(new ItemData(itemDto.Name, itemDto.Quantity, itemDto.Price));
                }
            }

            int amount = 0;
            foreach(var itemData in items)
            {
                var totalPrice = itemData.price * itemData.quantity;
                amount += totalPrice;
            }

            // For local development, might need to use a tunneling service like ngrok.
            string cancelUrl = "https://-frontend-domain.com/payment/cancel"; //  frontend cancel URL
            string returnUrl = "https://-frontend-domain.com/payment/success"; //  frontend success/return URL

            var paymentData = new PaymentData(orderCode, amount, orderRequest.Description, items, cancelUrl, returnUrl)
            {
                // Optional: buyerName, buyerEmail, buyerPhone, buyerAddress
                buyerName = orderRequest.BuyerName,
                buyerEmail = orderRequest.BuyerEmail,
                // buyerPhone = orderRequest.BuyerPhone
            };

            var paymentResult = await _payOSService.CreatePaymentLinkAsync(paymentData);
            
            //Th1: thanh toan khoa hoc
            if("thanh toan khoa hoc".Equals(orderRequest.Description.ToLower()))
            {
                Order order = new()
                {
                    OrderCode = orderCode,
                    UserId = userId,
                    CourseId = orderRequest.Items[0].CourseId
                };
                await _orderRepository.CreateOrder(order);
                
            }
            //Th2: thanh toan premium


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

    //if (string.IsNullOrEmpty(signatureHeader))
    //{
    //    _logger.LogWarning("Webhook received without signature.");
    //    return BadRequest("Missing signature.");
    //}

    try
    {
        WebhookData? verifiedData = null;
        WebhookType? realWebhookObject = null;
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

            //var payOSClient = new PayOS(
            //    _payOSService.GetSettings().ClientId,
            //    _payOSService.GetSettings().ApiKey,
            //    _payOSService.GetSettings().ChecksumKey);

            verifiedData = _payOSService.VerifyPaymentWebhookData(webhookObject);
            realWebhookObject = webhookObject;
        }
        catch (Exception verificationEx)
        {
            _logger.LogError(verificationEx, "Webhook signature verification failed: {ex}", verificationEx.Message);
            return BadRequest("Webhook signature verification failed.");
        }

        if (verifiedData != null)
        {
            _logger.LogInformation("Webhook verified successfully for orderCode: {OrderCode}, Status: {Status}",
                verifiedData.orderCode, verifiedData.code);
                    _logger.LogInformation("Check code: {code}", verifiedData.code);
                    _logger.LogInformation("Check success: {success}", realWebhookObject.success);
            if (verifiedData.code == "00" && realWebhookObject.success)
            {
                _logger.LogInformation("Payment successful for order: {OrderCode}", verifiedData.orderCode);

                try
                {
                            // Get order by order code for checking if the transaction has actually been paid successfully
                            var order = await _orderRepository.GetOrderByOrderCodeAsync(verifiedData.orderCode);
                            if (order == null)
                            {
                                throw new Exception("Order has not exist");
                            }
                    //Th1: thanh toan khoa hoc
                    if ("thanh toan khoa hoc".Equals(verifiedData.description))
                    {
                        BasicCreateUserCourseInfoRequestDTO basicCreateUserCourseInfoRequestDTO = new() 
                        {
                            EnrollmentStatus = BusinessObjects.Enums.CourseEnrollmentStatus.Enrolled,
                            LearningStatus = BusinessObjects.Enums.CourseLearningStatus.InProgress,
                            UserId = order.UserId,
                            CourseId = order.CourseId ?? 0
                        };
                                await _userCourseInfoService.CreateUserCourseInfo(basicCreateUserCourseInfoRequestDTO);
                    }

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
        [HttpGet("/api/order{orderId}")]
        public async Task<IActionResult> GetOrder([FromRoute] int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOSService.GetPaymentLinkInformationAsync(orderId);
                return StatusCode(200, paymentLinkInformation);
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return StatusCode(500, "false");
            }

        }
    }

    //  DTO for the create-payment-link endpoint 
    public class OrderCreationDto
    {
        //public int Amount { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

    }

    public class OrderItemDto
    {
        public long? CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}