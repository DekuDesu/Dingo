using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingoAPI
{
    public class AuthenticationRequestModel : IAuthenticationRequestModel
    {
        public string Id { get; set; }
        public string OAuth { get; set; }
    }
}
