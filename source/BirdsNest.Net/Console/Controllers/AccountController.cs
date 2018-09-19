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
        public IActionResult LogonExpiredForm(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Message"] = "Your session has expired";
            return PartialView("Login");
        }

        [HttpGet()]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PartialLogin(LoginViewModel details, string returnUrl = null)
        {
            if (ModelState.IsValid)
            { 
                bool result = await Auth(details);

                if (result)
                {
                    if (!string.IsNullOrEmpty(returnUrl) || Url.IsLocalUrl(returnUrl)) { return Redirect(returnUrl); }
                    else { return Ok(); }
                }
                else
                {
                    return PartialView("Login");
                }
            }
            else
            {
                return PartialView("Login");
            }
        }


        

        [HttpPost()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel details, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                bool result = await Auth(details);

                if (result)
                {
                    if (!string.IsNullOrEmpty(returnUrl) || Url.IsLocalUrl(returnUrl)) { return Redirect(returnUrl); }
                    else { return RedirectToAction("Index", "Home"); }
                }
                else
                {
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

        private async Task<bool> Auth(LoginViewModel details)
        {
            try
            {
                using (var context = LdapAuthorizer.CreateContext(this._config.Domain, this._config.ContainerDN, this._config.SSL))
                {
                    if (LdapAuthorizer.IsAuthenticated(context, details.UserName, details.Password))
                    {
                        bool authorized = false;
                        UserPrincipal user = LdapAuthorizer.GetUserPrincipal(context, details.UserName);

                        if (user == null)
                        {
                            ViewData["Message"] = "Could not retrieve user details";
                            return false;
                        }

                        var claims = new List<Claim> {
                                new Claim(ClaimTypes.GivenName,user.GivenName,this._config.Domain),
                                new Claim(ClaimTypes.Name,user.GivenName,this._config.Domain),
                                new Claim(ClaimTypes.Surname,user.Surname,this._config.Domain),
                                new Claim(ClaimTypes.Sid,user.Sid.Value,this._config.Domain)
                            };

                        if (LdapAuthorizer.IsMemberOf(context, user, this._config.UserGroup))
                        {
                            claims.Add(new Claim("BirdsNestUser", "True", ClaimValueTypes.Boolean, this._config.Domain));
                            authorized = true;
                        }

                        if (LdapAuthorizer.IsMemberOf(context, user, this._config.AdminGroup))
                        {
                            claims.Add(new Claim("BirdsNestAdmin", "True", ClaimValueTypes.Boolean, this._config.Domain));
                            authorized = true;
                        }

                        if (authorized == false)
                        {
                            ViewData["Message"] = "You are not authorised to use BirdsNest. Please contact your administrator";
                            return false;
                        }

                        var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var userPrincipal = new ClaimsPrincipal(userIdentity);
                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTime.UtcNow.AddSeconds(this._config.TimeoutSeconds),
                            IsPersistent = false,
                            AllowRefresh = true
                        };

                        //SignInResult signin = new SignInResult(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);
                        //await signin.ExecuteResultAsync(ControllerContext);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);

                        return true;
                    }
                    else
                    {
                        ViewData["Message"] = "Login failed";
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Errors = e.Message;
                ViewData["Message"] = "There was an error logging in.";
                return false;
            }
        }
    }
}