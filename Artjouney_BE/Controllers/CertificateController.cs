using Helpers.DTOs.Certificate;
using Helpers.HelperClasses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;

        public CertificateController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        /// <summary>
        /// Create a new certificate template for a course (Admin only)
        /// </summary>
        /// <param name="request">Certificate creation request with image file and course ID</param>
        /// <returns>Created certificate information</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateCertificate([FromForm] CreateCertificateRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _certificateService.CreateCertificateAsync(request);
            
            return result.Code switch
            {
                201 => CreatedAtAction(nameof(GetUserCertificatesByUserId), new { }, result),
                400 => BadRequest(result),
                401 => Unauthorized(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Delete a certificate template (Admin only)
        /// </summary>
        /// <param name="certificateId">Certificate ID to delete</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{certificateId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteCertificate([Required] long certificateId)
        {
            var result = await _certificateService.DeleteCertificateAsync(certificateId);
            
            return result.Code switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get all certificates earned by the current authenticated user (for completed courses only)
        /// </summary>
        /// <returns>List of user certificates</returns>
        [HttpGet("user")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserCertificatesByUserId()
        {
            var result = await _certificateService.GetUserCertificatesForCurrentUserAsync();
            
            return result.Code switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }



        /// <summary>
        /// Get all certificates for all users (Admin only)
        /// </summary>
        /// <returns>List of all user certificates</returns>
        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllUserCertificates()
        {
            var result = await _certificateService.GetAllUserCertificatesAsync();
            
            return result.Code switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get all certificates earned by users for a specific course (Admin only)
        /// </summary>
        /// <param name="courseId">Course ID to get certificates for</param>
        /// <returns>List of certificates for the course</returns>
        [HttpGet("course/{courseId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetUserCertificatesByCourseId([Required] long courseId)
        {
            var result = await _certificateService.GetUserCertificatesByCourseIdAsync(courseId);
            
            return result.Code switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Get certificate details by user certificate ID (Admin or certificate owner only)
        /// </summary>
        /// <param name="userCertificateId">User Certificate ID to retrieve details for (from user_certificate_infos table)</param>
        /// <returns>Certificate details</returns>
        [HttpGet("{userCertificateId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCertificateDetailsById([Required] long userCertificateId)
        {
            var result = await _certificateService.GetCertificateDetailsByIdAsync(userCertificateId);
            
            return result.Code switch
            {
                200 => Ok(result),
                403 => StatusCode(403, result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Award a certificate to a user for completing a course (Internal use / Admin)
        /// </summary>
        /// <param name="userId">User ID to award certificate to</param>
        /// <param name="courseId">Course ID that was completed</param>
        /// <returns>Awarded certificate information</returns>
        [HttpPost("award")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AwardCertificateToUser([Required] long userId, [Required] long courseId)
        {
            var result = await _certificateService.AwardCertificateToUserAsync(userId, courseId);
            
            return result.Code switch
            {
                201 => CreatedAtAction(nameof(GetUserCertificatesByUserId), new { }, result),
                400 => BadRequest(result),
                401 => Unauthorized(result),
                404 => NotFound(result),
                500 => StatusCode(500, result),
                _ => StatusCode(500, result)
            };
        }
    }
}
