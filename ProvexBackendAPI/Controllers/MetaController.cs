using System;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ProvexBackendAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MetaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MetaController> _logger;
        private readonly IWebHostEnvironment _environment;

        public MetaController(IConfiguration configuration, ILogger<MetaController> logger, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
        }

        [HttpGet("healthz")]
        [Produces("application/json")]
        public IActionResult Healthz()
        {
            var pipelineVersion =
                // Prefer values propagated in web.config by pipeline
                Environment.GetEnvironmentVariable("DEPLOY_VERSION")
                // Fallbacks
                ?? Environment.GetEnvironmentVariable("PIPELINE_VERSION")
                ?? Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER")
                ?? _configuration["Deployment:PipelineVersion"]
                ?? "unknown";

            var commitHash =
                // Prefer values propagated in web.config by pipeline
                Environment.GetEnvironmentVariable("GIT_SHA_SHORT")
                // Fallbacks
                ?? Environment.GetEnvironmentVariable("COMMIT_HASH")
                ?? Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION")
                ?? _configuration["Deployment:CommitHash"]
                          ?? "unknown";

            var environmentName =
                _environment?.EnvironmentName
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";
            var isLocal = string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(environmentName, "Local", StringComparison.OrdinalIgnoreCase);

            var response = new
            {
                status = "ok",
                pipelineVersion,
                commitHash,
                environment = environmentName,
                isLocal
            };

            return Ok(response);
        }
    }
}

