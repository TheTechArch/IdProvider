using IdProvider.Models;
using IdProvider.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdProvider.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IToken _tokenService;

        public TokenController(IToken tokenService)
        {
            _tokenService = tokenService;
        }

        [Consumes("application/x-www-form-urlencoded")]
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
            GrantResponse grantResponse = new GrantResponse();
            grantResponse.id_token = await _tokenService.GetTokenFromCode(code);
            return Ok(grantResponse);
        }
    }
}
