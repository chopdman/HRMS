using backend.DTO.Common;
using backend.DTO.Referrals;
using backend.Services.Common;
using backend.Services.Referrals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Referrals
{
    [ApiController]
    [Route("api/v1/job-openings")]
    public class JobOpeningsController : ControllerBase
    {
        private readonly JobOpeningService _service;
        private readonly AuthService _auth;

        public JobOpeningsController(JobOpeningService service, AuthService auth)
        {
            _service = service;
            _auth = auth;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool activeOnly = true)
        {
            var result = await _service.GetOpeningsAsync(activeOnly);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }

        [Authorize]
        [HttpGet("{jobId:int}")]
        public async Task<IActionResult> GetById(long jobId)
        {
            try
            {
                var result = await _service.GetByIdAsync(jobId);
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
        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Create([FromForm] JobOpeningCreateDto dto)
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
                var result = await _service.CreateAsync(dto, currentUserId.Value);
                return CreatedAtAction(nameof(GetById), new { jobId = result.JobId }, new ApiResponse<object>
                {
                    Success = true,
                    Code = 201,
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
        [HttpPut("{jobId:int}")]
        public async Task<IActionResult> Update(long jobId, [FromBody] JobOpeningUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var result = await _service.UpdateAsync(jobId, dto);
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