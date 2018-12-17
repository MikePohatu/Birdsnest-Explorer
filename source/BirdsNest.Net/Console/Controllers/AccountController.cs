using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Console.Auth.Directory;
using System.Security.Claims;
using Console.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Console.Auth;
using Microsoft.Extensions.Logging;

namespace Console.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        //private DirectoryConfiguration _config;
        private readonly ILogger<AccountController> _logger;
        private AuthConfigurations _configlist;

        public AccountController(ILogger<AccountController> logger, AuthConfigurations configuration)
        {
            this._configlist = configuration;
            this._logger = logger;
            //this._config = configuration;
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
                IAuthConfiguration conf = this._configlist.GetAuthConfiguration(details.Provider);
                ILogin login = conf.GetLogin(details.UserName, details.Password);
                //ILogin login = new DirectoryLogin(this._config, details.UserName, details.Password);
                if (login.IsAuthenticated)
                { 
                    if (login.IsAuthorised == false)
                    {
                        ViewData["Message"] = "You are not authorised to use BirdsNest. Please contact your administrator";
                        this._logger.LogWarning("Login not authorised: {username}",details.UserName);
                        return false;
                    }

                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.GivenName,login.GivenName,conf.Name),
                        new Claim(ClaimTypes.Name,login.GivenName,conf.Name),
                        new Claim(ClaimTypes.Surname,login.Surname,conf.Name),
                        new Claim(ClaimTypes.Sid,login.ID,conf.Name)
                    };

                    if (login.IsUser)
                    {
                        claims.Add(new Claim("BirdsNestUser", "True", ClaimValueTypes.Boolean, conf.Name));
                    }

                    if (login.IsAdmin)
                    {
                        claims.Add(new Claim("BirdsNestAdmin", "True", ClaimValueTypes.Boolean, conf.Name));
                    }

                    var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var userPrincipal = new ClaimsPrincipal(userIdentity);
                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddSeconds(login.TimeoutSeconds),
                        IsPersistent = false,
                        AllowRefresh = true
                    };

                    //SignInResult signin = new SignInResult(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);
                    //await signin.ExecuteResultAsync(ControllerContext);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authProperties);
                    this._logger.LogInformation("Login successful: {username}", details.UserName);
                    return true;
                }
                else
                {
                    ViewData["Message"] = "Login failed";
                    return false;
                }
            }
            catch (Exception e)
            {
                ViewBag.Errors = e.Message;
                ViewData["Message"] = "There was an error logging in.";
                this._logger.LogInformation("Login error: {username}. Error: {error}", details.UserName, e.Message);
                return false;
            }
        }
    }
}