using System;
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
using Console.Auth.Directory;
using common;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Serialization;
using Console.Auth;
using Console.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Console.Plugins;

namespace Console
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IHostingEnvironment env, IConfiguration config,
            ILoggerFactory loggerFactory)
        {
            _env = env;
            _config = config;
            _loggerFactory = loggerFactory;
        }

        //public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var logger = this._loggerFactory.CreateLogger<Startup>();
            Stopwatch stopwatch = new Stopwatch();

            PluginManager.Reload();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Console.Auth.Types.BirdsNestAdminsPolicy, policy => policy.RequireClaim(Console.Auth.Types.BirdsNestAdminsClaim,"True"));
                options.AddPolicy(Console.Auth.Types.BirdsNestUsersPolicy, policy => policy.RequireClaim(Console.Auth.Types.BirdsNestUsersClaim, "True"));
            });

            services.AddMvc(config =>
            {
                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                config.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            logger.LogInformation("Loading neo4j configuration");
            Neo4jService neoservice;
            using (NeoConfiguration config = new NeoConfiguration())
            {
                config.DB_URI = _config["neo4jSettings:DB_URI"];
                config.DB_Username = _config["neo4jSettings:DB_Username"];
                config.DB_Password = _config["neo4jSettings:DB_Password"];
                neoservice = new Neo4jService(this._loggerFactory.CreateLogger<Neo4jService>(), config);
            }
            services.AddSingleton(neoservice);

            logger.LogInformation("Loading authentication configuration");
            services.AddSingleton(new AuthConfigurations(_config.GetSection("Authorization")));
            logger.LogDebug("Finished loading authentication configuration");

            stopwatch.Restart();
            neoservice.GetAllNodesCount();
            logger.LogInformation("Initialised connection in {elapsed}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
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
