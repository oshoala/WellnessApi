using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace WellnessApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeadwayAppsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<LeadwayAppsController> _logger;

        public LeadwayAppsController(AppDbContext dbContext, ILogger<LeadwayAppsController> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("GetPAIssuedEnrollee")]
        public async Task<IActionResult> GetPAIssuedEnrollee(
            [FromQuery] string fromDate,
            [FromQuery] string toDate,
            [FromQuery] string cifno = "",
            [FromQuery] string paStatus = "",
            [FromQuery] string visitid = "")
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest("fromDate and toDate parameters are required.");
            }

            // Validate date format
            if (!DateTime.TryParseExact(fromDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dtFromDate) ||
                !DateTime.TryParseExact(toDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dtToDate))
            {
                return BadRequest("Invalid date format. Use yyyyMMdd format.");
            }

            try
            {
                // Option 1: Using dynamic list (recommended)
                var sql = $@"exec Api_spGet_PAIssued_Enrollee '{dtFromDate:yyyyMMdd}', '{dtToDate:yyyyMMdd}','{cifno}','{paStatus}','{visitid}'";
                var result = await _dbContext.ExecuteStoredProcedureDynamic(sql);

                // Option 2: Using parameters (safer - prevents SQL injection)
                // var parameters = new[]
                // {
                //     new SqlParameter("@FromDate", dtFromDate.ToString("yyyyMMdd")),
                //     new SqlParameter("@ToDate", dtToDate.ToString("yyyyMMdd")),
                //     new SqlParameter("@CifNo", cifno ?? ""),
                //     new SqlParameter("@PAStatus", paStatus ?? ""),
                //     new SqlParameter("@VisitId", visitid ?? "")
                // };
                // var result = await _dbContext.ExecuteStoredProcedureWithParams("Api_spGet_PAIssued_Enrollee", parameters);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetPAIssuedEnrollee: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // You can add more API endpoints here following the same pattern
    }
}