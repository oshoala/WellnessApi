using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [FromQuery] string visitid = "",
        [FromQuery] string benefitid = "",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new { error = "fromDate and toDate parameters are required." });
            }

            if (!DateTime.TryParseExact(fromDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dtFromDate) ||
                !DateTime.TryParseExact(toDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dtToDate))
            {
                return BadRequest(new { error = "Invalid date format. Use yyyyMMdd format." });
            }

            try
            {
                _logger.LogInformation($"Executing GetPAIssuedEnrollee: {dtFromDate:yyyyMMdd} to {dtToDate:yyyyMMdd}, Page {pageNumber}, Size {pageSize}");

                var sql = $@"exec Api_spGet_PAIssued_Enrollee '{dtFromDate:yyyyMMdd}', '{dtToDate:yyyyMMdd}','{cifno}','{paStatus}','{visitid}','{benefitid}',{pageNumber},{pageSize}";

                var result = await _dbContext.ExecuteStoredProcedureDynamic(sql);

                _logger.LogInformation($"Query executed successfully. Returned {result?.Count ?? 0} rows");

                return Ok(new
                {
                    data = result,
                    count = result?.Count ?? 0,
                    pageNumber = pageNumber,
                    pageSize = pageSize
                });
            }
            catch (System.Data.SqlClient.SqlException ex) when (ex.Message.Contains("Timeout"))
            {
                _logger.LogError(ex, "Database timeout occurred");
                return StatusCode(504, new { error = "Database timeout", message = "Query took too long to execute. Try reducing the date range or page size." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetPAIssuedEnrollee: {Message}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
        [HttpGet("GetAllDepartments")]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                _logger.LogInformation("GetAllDepartments endpoint called");

                var sql = "EXEC dbo.apiget_discipline";
                var result = await _dbContext.ExecuteStoredProcedureDynamic(sql);

                if (result == null || !result.Any())
                {
                    _logger.LogWarning("No departments found");
                    return Ok(new { result = new List<object>(), message = "No departments found" });
                }

                return Ok(new { result = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetAllDepartments: {Message}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        // You can add more API endpoints here following the same pattern
    }
}