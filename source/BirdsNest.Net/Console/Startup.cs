﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Console.neo4jProxy;
using Console.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Console.Directory;
using common;
using Microsoft.AspNetCore.Identity;

namespace Console
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = new PathString("/auth");
                opt.AccessDeniedPath = new PathString("/auth");
                opt.LogoutPath = new PathString("/");
            });

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsBirdsNestAdmin", policy => policy.RequireClaim("BirdsNestAdmin","True"));
                options.AddPolicy("IsBirdsNestUser", policy => policy.RequireClaim("BirdsNestUser", "True"));
            });

            services.AddMvc(config =>
            {
                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                config.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton(serviceProvider =>
            {
                Neo4jService neoservice;
                using (NeoConfiguration config = new NeoConfiguration())
                {
                    config.DB_URI = Configuration["neo4jSettings:DB_URI"];
                    config.DB_Username = Configuration["neo4jSettings:DB_Username"];
                    config.DB_Password = Configuration["neo4jSettings:DB_Password"];
                    neoservice = new Neo4jService(config);
                }

                return neoservice;
            });

            services.AddSingleton(serviceProvider =>
            {
                DirectoryConfiguration config = new DirectoryConfiguration();
                config.Domain = Configuration["ActiveDirectorySettings:Domain"];
                config.AdminGroup = Configuration["ActiveDirectorySettings:AdminGroup"];
                config.UserGroup = Configuration["ActiveDirectorySettings:UserGroup"];
                return config;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvcWithDefaultRoute();
        }
    }
}
