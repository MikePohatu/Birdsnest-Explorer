using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Auth
{
    public class AuthDetails
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Provider { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(this.Username) || string.IsNullOrWhiteSpace(this.Password) || string.IsNullOrWhiteSpace(this.Provider))
            { return false; }
            else
            { return true; }
        }
    }
}
