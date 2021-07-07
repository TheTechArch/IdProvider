﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdProvider.Models
{
    public class OidcAuthorizationModel
    {
        public string UserClaims { get; set; }

        public string Redirect_uri { get; set; }

        public string Scope { get; set; }

        public string Response_type { get; set; }

        public string Client_id { get; set; }

        public string State { get; set; }

        public string Nonce { get; set; }
        public string Acr_values { get; internal set; }
        public string Response_mode { get; internal set; }
        public string Ui_locales { get; internal set; }
        public string Prompt { get; internal set; }
        public string Code_challenge { get; internal set; }
        public string Code_challenge_method { get; internal set; }
        public string Login_hint { get; internal set; }
        public string Claims { get; internal set; }
        public string Request_uri { get; internal set; }
    }
}
