using IdProvider.Configuration;
using IdProvider.Models;
using IdProvider.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace IdProvider.Controllers
{
    [Route("token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IToken _tokenService;
        private readonly GeneralSettings _generalSettings;

        public TokenController(IToken tokenService, IOptions<GeneralSettings> generalSettings)
        {
            _tokenService = tokenService;
            _generalSettings = generalSettings.Value;
        }

        [Consumes("application/x-www-form-urlencoded")]
        [HttpPost]
        public async Task<ActionResult> Index(
            [FromForm] string client_id, 
            [FromForm] string grant_type, 
            [FromForm] string code, 
            [FromForm] string redirect_uri,
            [FromForm] string code_verifier,
            [FromForm] string client_assertion_type,
            [FromForm] string client_assertion,
            [FromForm] string assertion,
            [FromForm] string refresh_token)
        {
            if (!_generalSettings.TestIdpEnabled)
            {
                return NotFound();
            }

            GrantResponse grantResponse = new GrantResponse
            {
                id_token = await _tokenService.GetTokenFromCode(code),
                access_token = await _tokenService.GetTokenFromCode(code),
                token_type = "Bearer",
                expires_in = 3600,
                refresh_token = "ADFSFDSFSDFDSFDSF"
            };
            return Ok(grantResponse);
        }
    }
}
