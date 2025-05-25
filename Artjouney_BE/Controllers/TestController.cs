using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public async Task<IActionResult> ping()
        {
            return Ok("pong");
        }
    }
}
