using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdProvider.Models
{
    public class GrantResponse
    {
        public string client_id { get; set; }

        public string id_token { get; set; }
    }
}
