using backend.DTO.Common;
using backend.DTO.Referrals;
using backend.Services.Common;
using backend.Services.Referrals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Referrals
{
    [ApiController]
    [Route("api/v1/job-openings/{jobId:int}/share")]
    public class JobSharesController : ControllerBase
    {
        private readonly JobShareService _service;
        private readonly AuthService _auth;

        public JobSharesController(JobShareService service, AuthService auth)
        {
            _service = service;
            _auth = auth;
        }

        [Authorize(Roles = "Employee,Manager,HR")]
        [HttpPost]
        public async Task<IActionResult> Share(long jobId, [FromBody] JobShareRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUserId = _auth.GetUserId(User);
            if (currentUserId is null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Code = 401,
                    Error = "Invalid token, user not found."
                });
            }

            try
            {
                var result = await _service.ShareAsync(jobId, dto, currentUserId.Value);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Code = 200,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Code = 400,
                    Error = ex.Message
                });
            }
        }
    }
}