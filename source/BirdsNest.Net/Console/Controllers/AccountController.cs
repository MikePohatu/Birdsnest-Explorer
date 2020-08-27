#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Console.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Console.Plugins;

namespace Console.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private const string TRUE_STRING = "True";
        private readonly ILogger<AccountController> _logger;
        private AuthConfigurations _configlist;
        private AuthConfigurations _authconfigs;

        public AccountController(ILogger<AccountController> logger, AuthConfigurations configuration, AuthConfigurations authconfigs)
        {
            this._configlist = configuration;
            this._logger = logger;
            this._authconfigs = authconfigs;
        }

        public object Index()
        {
            return "hello";
        }

        [Authorize]
        [HttpGet("ping")]
        public async Task<object> Ping()
        {
            AuthResults result = new AuthResults();
            IEnumerable<Claim> claims = this.HttpContext.User.Claims;
            foreach (Claim claim in claims)
            {
                if (claim.Type == Types.BirdsNestAdminsClaim && claim.Value == TRUE_STRING)
                {
                    result.IsAdmin = true;
                    result.IsAuthenticated = true;
                    result.IsAuthorized = true;
                    result.Message = "OK";
                    break;
                }
                else if (claim.Type == Types.BirdsNestUsersClaim && claim.Value == TRUE_STRING)
                {
                    result.IsAuthenticated = true;
                    result.IsAuthorized = true;
                    result.Message = "OK";
                }
                else if (claim.Type == ClaimTypes.GivenName)
                {
                    result.Name = claim.Value;
                }
            }
            
            if (result.IsAuthorized == false) { await Logout(); }
            return result;
        }

        [HttpGet("providers")]
        public object Providers()
        {
            return this._authconfigs.ConfigurationNames;
        }

        [HttpGet("logout")]
        public async Task<object> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            foreach (var cookie in HttpContext.Request.Cookies)
            {
                HttpContext.Response.Cookies.Delete(cookie.Key);
            }

            AuthResults result = new AuthResults();
            result.Message = "Logged out";
            return result;
        }

        [ValidateAntiForgeryToken]
        [HttpPost("login")]
        public async Task<object> Login([FromForm] AuthDetails details)
        {
            this._logger.LogInformation("Login requested: {0} - {1}", details.Username, details.Provider);
            AuthResults result = null;
            if (details.IsValid())
            {
                result = await Auth(details);
            }
            else
            {
                result = new AuthResults();
                result.Message = "Invalid login data";
            }
            return result;
        }

        private async Task<AuthResults> Auth(AuthDetails details)
        {
            AuthResults result = new AuthResults();
            try
            {
                IAuthConfiguration conf = this._configlist.GetAuthConfiguration(details.Provider);
                if (conf == null)
                {
                    throw new ArgumentException("Provider not found");
                }
                ILogin login = conf.GetLogin(details.Username, details.Password);

                if (login.IsAuthenticated)
                {
                    result.Name = login.GivenName;
                    result.IsAuthenticated = true;
                    if (login.IsAuthorised == false)
                    {
                        result.Message = "You are not authorised to use BirdsNest. Please contact your administrator";
                        this._logger.LogWarning("Login not authorised: {username}", details.Username);
                        return result;
                    }

                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.GivenName,login.GivenName,conf.Name),
                        new Claim(ClaimTypes.Name,login.Name,conf.Name),
                        new Claim(ClaimTypes.Surname,login.Surname,conf.Name),
                        new Claim(ClaimTypes.Sid,login.ID,conf.Name)
                    };

                    if (login.IsUser)
                    {
                        claims.Add(new Claim(Types.BirdsNestUsersClaim, TRUE_STRING, ClaimValueTypes.Boolean, conf.Name));
                    }

                    if (login.IsAdmin)
                    {
                        result.IsAdmin = true;
                        claims.Add(new Claim(Types.BirdsNestAdminsClaim, TRUE_STRING, ClaimValueTypes.Boolean, conf.Name));
                    }

                    var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var userPrincipal = new ClaimsPrincipal(userIdentity);
                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddSeconds(login.TimeoutSeconds),
                        IsPersistent = false,
                        AllowRefresh = true
                    };

                    await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);
                    result.IsAuthorized = true;
                    result.Message = "OK";
                    this._logger.LogInformation("Login successful: {username}", details.Username);
                    return result;
                }
                else
                {
                    result.Message = "Login failed";
                    
                }
            }
            catch (Exception e)
            {
                result.Message = "There was an error logging in: " + e.Message;
                this._logger.LogInformation("Login error: {username}. Error: {error}", details.Username, e.Message);
            }
            return result;
        }
    }
}