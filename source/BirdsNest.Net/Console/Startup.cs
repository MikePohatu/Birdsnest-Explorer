#region license
// Copyright (c) 2019-2023 "20Road"
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
using Console.Auth;
using Console.Helpers;
using Console.neo4jProxy;
using Console.Plugins;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;

namespace Console
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            InstallInfo.Instance.DbSvcAccount = Configuration.GetValue<string>("neo4jSettings:dbUsername");
            InstallInfo.Instance.RootPath = env.ContentRootPath.Replace("\\Console", "").Replace("\\console", "");
            InstallInfo.Instance.DbPath = RegistryHelpers.GetServiceInstallPath("neo4j").Split(new string[] { "\\bin" }, 2, StringSplitOptions.None)[0];

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (Configuration.GetValue<bool>("EnableHsts") == true)
            {
                int maxage = 24;
                try
                {
                    maxage = Configuration.GetValue<int>("HstsMaxAgeHours");
                }
                catch { }

                maxage = maxage < 1 ? 24 : maxage;

                services.AddHsts(options =>
                {
                    options.MaxAge = TimeSpan.FromHours(maxage);
                });
            }

            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });


            services.AddControllersWithViews();

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Types.BirdsNestAdminsPolicy, policy => policy.RequireClaim(Types.BirdsNestAdminsClaim, "True"));
                options.AddPolicy(Types.BirdsNestUsersPolicy, policy => policy.RequireClaim(Types.BirdsNestUsersClaim, "True"));
            });

            services.AddControllers(config =>
            {
                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                config.Filters.Add(new AuthorizeFilter(policy));
            }).AddNewtonsoftJson();

            services.AddSingleton<PluginManager>();
            services.AddSingleton<Neo4jService>();
            services.AddSingleton<AuthConfigurations>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                if (Configuration.GetValue<bool>("EnableHSTS") == true)
                {
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                if (Configuration.GetValue<bool>("AllowHTTP") != true)
                {
                    app.UseHttpsRedirection();
                }
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            if (env.IsDevelopment())
            {
                app.Use(next => context =>
                {
                    Debug.WriteLine($"Route: {context.GetEndpoint()?.DisplayName}");
                    return next(context);
                });
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                //if (env.IsDevelopment())
                //{
                //    endpoints.MapToVueCliProxy(
                //        "{*path}",
                //        new SpaOptions { SourcePath = "ClientApp" },
                //        npmScript: "serve",
                //        regex: "Compiled successfully");
                //}
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                //if (env.IsDevelopment())
                //{
                //    spa.UseProxyToSpaDevelopmentServer("http://localhost:8080");
                //}
            });

            //"warm up" neo4j, which also warms up PluginManager
            app.ApplicationServices.GetService<Neo4jService>();
        }
    }
}
