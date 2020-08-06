using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Auth
{
    public class AuthResults
    {
        public bool IsAuthenticated { get; set; } = false;
        public bool IsAuthorized { get; set; } = false;
        public bool IsAdmin { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public string Message { get; set; }
    }
}
