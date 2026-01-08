using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProvexBackendAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IAntiforgery antiforgery;

        public TokenController(IAntiforgery antiforgery)
        {
            this.antiforgery = antiforgery;
        }

        
        [HttpGet("token")]
        [AllowAnonymous]
        public async Task<string> GetToken()
        {
            var tokens = antiforgery.GetTokens(HttpContext);
            return tokens.RequestToken ?? "";
        }
    }
}
