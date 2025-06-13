using Helpers.DTOs.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentController> _logger; 

        public PaymentController(PayOS payOS, IHttpContextAccessor httpContextAccessor, ILogger<PaymentController> logger)
        {
            _payOS = payOS;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpPost("/create-payment-link")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                ItemData item = new ItemData("Mì tôm hảo hảo ly", 1, 1000);
                List<ItemData> items = new List<ItemData> { item };

                // Get the current request's base URL
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                PaymentData paymentData = new PaymentData(
                    orderCode,
                    1000,
                    "Thanh toan don hang",
                    items,
                    $"{baseUrl}/cancel",
                    $"{baseUrl}/success"
                );

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                //return Redirect(createPayment.checkoutUrl);
                return Ok(createPayment);
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(exception);
                return Redirect("/");
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder([FromRoute] int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderId);
                return Ok(paymentLinkInformation);
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok("faild");
            }

        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(orderId);
                return Ok(paymentLinkInformation);
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok("faild");
            }

        }

        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(ConfirmWebhook body)
        {
            try
            {
                _logger.LogInformation("this is in confirm-webhook api -> ");
                var result = await _payOS.confirmWebhook(body.webhook_url ?? "");
                return Ok(result);
            }
            catch (System.Exception exception)
            {

                Console.WriteLine(exception);
                return Ok("faild");
            }
        }

        [HttpPost("webhook-handler")]
        public IActionResult payOSTransferHandler(WebhookType body)
        {
            try
            {
                _logger.LogInformation("this is in webhook-handler api");
                _logger.LogInformation(body.data.description);
                _logger.LogInformation($"{body.data.amount}");
                WebhookData data = _payOS.verifyPaymentWebhookData(body);

                if (data.description == "Ma giao dich thu nghiem" || data.description == "VQRIO123")
                {
                    return Ok(data);
                }
                return Ok(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Ok("faild");
            }

        }




    }
}
