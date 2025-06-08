using Helpers.DTOs.Module;
using Helpers.DTOs.SubModule;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ISubModuleService _subModuleService;
        public ModuleController(IModuleService moduleService, ISubModuleService subModuleService)
        {
            _moduleService = moduleService;
            _subModuleService = subModuleService;
        }

        [HttpPost("/modules")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateModuleAsync([FromBody] CreateModuleRequestDTO requestDTO)
        {
            var response = await _moduleService.CreateModuleAsync(requestDTO);
            return StatusCode(response.Code, response);
        }

        [HttpPost("/modules/sub-modules")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateSubModuleAsync([FromBody] CreateSubModuleRequestDTO requestDTO)
        {
            var response = await _subModuleService.CreateSubModuleAsync(requestDTO);
            return StatusCode(response.Code, response);
        }
    }
}
