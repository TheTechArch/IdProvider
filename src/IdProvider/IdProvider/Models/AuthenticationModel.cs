using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdProvider.Models
{
    public class AuthenticationModel
    {
        public string Redirect_uri { get; set; }

        public string Scope { get; set; }

        public string Response_type { get; set; }

        public string Client_id { get; set; }

        public string State { get; set; }

        public string Nonce { get; set; }
    }
}
