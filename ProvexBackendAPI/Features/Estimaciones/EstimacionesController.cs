using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;

namespace ProvexBackendAPI.Features.Estimaciones
{
    [Route("api/v{version:apiVersion}/estimaciones")]
    [ApiController]
    [ApiVersionNeutral]
    //[Authorize]
    public class EstimacionesController : ControllerBase
    {
        private readonly IEstimacionesService _estimacionesService;
        public EstimacionesController(IEstimacionesService estimacionesService) => _estimacionesService = estimacionesService;

    }
}
