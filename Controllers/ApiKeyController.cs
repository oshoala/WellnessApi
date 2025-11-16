using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WellnessApis.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                // Try to get the API key from the header
                if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
                {
                    HandleUnauthorized(context, "API Key is missing");
                    return;
                }

                var apiKey = extractedApiKey.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    HandleUnauthorized(context, "API Key is empty");
                    return;
                }

                // Get configuration
                var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                // Validate the API key
                if (!IsValidApiKey(apiKey, configuration))
                {
                    HandleUnauthorized(context, "Invalid API Key");
                    return;
                }

                // API key is valid, allow the request to proceed
                await next();
            }
            catch (Exception ex)
            {
                HandleUnauthorized(context, "Authorization error: " + ex.Message);
            }
        }

        private bool IsValidApiKey(string apiKey, IConfiguration configuration)
        {
            // Get the valid API key from appsettings.json
            var validApiKey = configuration["ApiKey"];

            if (string.IsNullOrWhiteSpace(validApiKey))
            {
                throw new InvalidOperationException("API Key not configured in appsettings.json");
            }

            // Compare the provided key with the valid key
            return apiKey.Equals(validApiKey, StringComparison.Ordinal);
        }

        private void HandleUnauthorized(ActionExecutingContext context, string message)
        {
            context.Result = new JsonResult(new
            {
                success = false,
                message = message,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = 401
            };
        }
    }
}