using IdProvider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdProvider.Services.Interface
{
    public interface IToken
    {
        Task<string> GetAuthorizationCode(OidcAuthorizationModel oidcAuthorizationModel);

        Task<string> GetTokenFromCode(string code);

    }
}
