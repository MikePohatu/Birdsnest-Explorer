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
    public class AccountController : Controller
    {
        private DirectoryConfiguration _config;

        public AccountController(DirectoryConfiguration configuration)
        {
            this._config = configuration;
        }

        [HttpGet()]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel details, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var context = LdapAuthorizer.CreateContext(_config.Domain,_config.ContainerDN, _config.SSL))
                    {
                        if (LdapAuthorizer.IsAuthenticated(context, details.UserName, details.Password))
                        {
                            bool authorized = false;
                            UserPrincipal user = LdapAuthorizer.GetUserPrincipal(context, details.UserName);

                            var claims = new List<Claim> {
                                new Claim(ClaimTypes.GivenName,user.GivenName,_config.Domain),
                                new Claim(ClaimTypes.Name,user.GivenName,_config.Domain),
                                new Claim(ClaimTypes.Surname,user.Surname,_config.Domain),
                                new Claim(ClaimTypes.Sid,user.Sid.Value,_config.Domain)
                            };

                            if (LdapAuthorizer.IsMemberOf(context, user, this._config.UserGroup))
                            {
                                claims.Add(new Claim("BirdsNestUser", "True", ClaimValueTypes.Boolean, _config.Domain));
                                authorized = true;
                            }

                            if (LdapAuthorizer.IsMemberOf(context, user, this._config.AdminGroup))
                            {
                                claims.Add(new Claim("BirdsNestAdmin", "True", ClaimValueTypes.Boolean, _config.Domain));
                                authorized = true;
                            }

                            if (authorized == false)
                            {
                                ViewData["Message"] = "You are not authorised to use BirdsNest. Please contact your administrator";
                                return View();
                            }

                            var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var userPrincipal = new ClaimsPrincipal(userIdentity);
                            var authProperties = new AuthenticationProperties
                            {
                                ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                                IsPersistent = false,
                                AllowRefresh = true
                            };

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);

                            if (!string.IsNullOrEmpty(returnUrl) || Url.IsLocalUrl(returnUrl)) { return Redirect(returnUrl); }
                            else { return RedirectToAction("Index", "Home"); }

                        }
                        else
                        {
                            ViewData["Message"] = "Login failed";
                            return View();
                        }
                    }  
                }
                catch (Exception e)
                {
                    ViewBag.Errors = e.Message;
                    ViewData["Message"] = "There was an error logging in." ;
                    return View();
                }
            }
            else
            {
                return View();
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