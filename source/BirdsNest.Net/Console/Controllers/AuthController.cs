using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Console.Directory;
using System.Security.Claims;
using Console.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Console.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private DirectoryConfiguration _config;

        public AuthController(DirectoryConfiguration configuration)
        {
            this._config = configuration;
        }

        [HttpGet()]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel details)
        {
            using (var context = LdapAuthorizer.CreateContext(_config.Domain))
            {
                if (LdapAuthorizer.IsAuthenticated(context, details.UserName, details.Password))
                {
                    UserPrincipal user = LdapAuthorizer.GetUserPrincipal(context, details.UserName);

                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.GivenName,user.GivenName,_config.Domain),
                        new Claim(ClaimTypes.Name,user.GivenName,_config.Domain),
                        new Claim(ClaimTypes.Surname,user.Surname,_config.Domain),
                        new Claim(ClaimTypes.Sid,user.Sid.Value,_config.Domain)
                        };

                    if (LdapAuthorizer.IsMemberOf(context,user,this._config.UserGroup))
                    {
                        claims.Add(new Claim("BirdsNestUser","True",ClaimValueTypes.Boolean, _config.Domain));
                    }

                    if (LdapAuthorizer.IsMemberOf(context, user, this._config.AdminGroup))
                    {
                        claims.Add(new Claim("BirdsNestAdmin", "True", ClaimValueTypes.Boolean, _config.Domain));
                    }


                    var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var userPrincipal = new ClaimsPrincipal(userIdentity);
                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                        IsPersistent = false,
                        AllowRefresh = false
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);

                    return RedirectToAction("Index","Home");
                }
                else
                {
                    ViewData["Message"] = "Login failed";

                    return View();
                }
            }
        }

        [HttpGet()]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}