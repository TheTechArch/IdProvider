using IdProvider.Models;
using IdProvider.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdProvider.Controllers
{
    [Route("token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IToken _tokenService;

        public TokenController(IToken tokenService)
        {
            _tokenService = tokenService;
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
            GrantResponse grantResponse = new GrantResponse
            {
                id_token = await _tokenService.GetTokenFromCode(code)
            };
            return Ok(grantResponse);
        }
    }
}
