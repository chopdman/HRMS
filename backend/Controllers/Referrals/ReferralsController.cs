using backend.DTO.Common;
using backend.DTO.Referrals;
using backend.Services.Common;
using backend.Services.Referrals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Referrals
{
    [ApiController]
    [Route("api/v1/referrals")]
    public class ReferralsController : ControllerBase
    {
        private readonly ReferralService _service;
        private readonly AuthService _auth;

        public ReferralsController(ReferralService service, AuthService auth)
        {
            _service = service;
            _auth = auth;
        }

        [Authorize(Roles = "Employee,Manager,HR")]
        [HttpPost("{jobId:int}")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Refer(long jobId, [FromForm] ReferralCreateDto dto)
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
                var result = await _service.CreateAsync(jobId, dto, currentUserId.Value);
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

        [Authorize(Roles = "HR")]
        [HttpGet("job/{jobId:int}")]
        public async Task<IActionResult> GetByJob(long jobId)
        {
            var result = await _service.GetByJobAsync(jobId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }

        [Authorize(Roles = "HR")]
        [HttpGet("{referralId:int}/logs")]
        public async Task<IActionResult> GetLogs(long referralId)
        {
            var result = await _service.GetLogsAsync(referralId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }

        [Authorize(Roles = "HR")]
        [HttpPut("{referralId:int}/status")]
        public async Task<IActionResult> UpdateStatus(long referralId, [FromBody] ReferralStatusUpdateDto dto)
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
                var result = await _service.UpdateStatusAsync(referralId, dto, currentUserId.Value);
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

        [Authorize(Roles = "HR")]
        [HttpGet("config/default-hr-email")]
        public async Task<IActionResult> GetDefaultHrEmail()
        {
            var result = await _service.GetDefaultHrEmailAsync();
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }

        [Authorize(Roles = "HR")]
        [HttpPut("config/default-hr-email")]
        public async Task<IActionResult> UpdateDefaultHrEmail([FromBody] DefaultHrEmailUpdateDto dto)
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
                var result = await _service.UpdateDefaultHrEmailAsync(dto.Email, currentUserId.Value);
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

        [Authorize(Roles = "HR")]
        [HttpGet("config/anjum-hr-email")]
        public async Task<IActionResult> GetAnjumHrEmail()
        {
            var result = await _service.GetAnjumHrEmailAsync();
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }

        [Authorize(Roles = "HR")]
        [HttpPut("config/anjum-hr-email")]
        public async Task<IActionResult> UpdateAnjumHrEmail([FromBody] AnjumHrEmailUpdateDto dto)
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
                var result = await _service.UpdateAnjumHrEmailAsync(dto.Email, currentUserId.Value);
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