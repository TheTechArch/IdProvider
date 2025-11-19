using IdProvider.Models;
using IdProvider.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IdProvider.Controllers
{
    public class AuthorizeController : Controller
    {
        private readonly IToken _tokenService;

        public AuthorizeController(IToken tokenService)
        {
            _tokenService = tokenService;
        }


        /// <summary>
        /// OIDC login endpoint. It is really an authenticaiton controller but people that made this protocoll did not have a clue
        /// </summary>
        /// <param name="redirect_url"></param>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(
            [FromQuery] string response_type,
            [FromQuery] string client_id,
            [FromQuery] string redirect_uri,
            [FromQuery] string scope,
            [FromQuery] string state,
            [FromQuery] string nonce,
            [FromQuery] string acr_values,
            [FromQuery] string response_mode,
            [FromQuery] string ui_locales,
            [FromQuery] string prompt,
            [FromQuery] string code_challenge,
            [FromQuery] string code_challenge_method,
            [FromQuery] string login_hint,
            [FromQuery] string claims,
            [FromQuery] string request_uri
          )
        {
            OidcAuthorizationModel viewModel = new()
            {
                Response_type = response_type,
                Client_id = client_id,
                Redirect_uri = redirect_uri,
                Scope = scope,
                State = state,
                Nonce = nonce,
                Acr_values = acr_values,
                Response_mode = acr_values,
                Ui_locales = ui_locales,
                Prompt = prompt,
                Code_challenge = code_challenge,
                Code_challenge_method = code_challenge_method,
                Login_hint = login_hint,
                Claims = claims,
                Request_uri = request_uri
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Index(OidcAuthorizationModel viewModel)
        {
            string code = await _tokenService.GetAuthorizationCode(viewModel);

            UriBuilder baseUri = new(viewModel.Redirect_uri);
            if (baseUri.Query != null && baseUri.Query.Length > 1)
                baseUri.Query = baseUri.Query + "&" + "code=" + code;
            else
            {
                baseUri.Query = "code=" + code;
            }

            baseUri.Query = baseUri.Query + "&state=" + viewModel.State;

            return Redirect(baseUri.ToString());
        }
        private Uri BuildRedirectUri(OidcAuthorizationModel model, string authorizationCode)
        {

            Uri uri = new Uri(model.Redirect_uri);

            return uri;
        }
    }
}
