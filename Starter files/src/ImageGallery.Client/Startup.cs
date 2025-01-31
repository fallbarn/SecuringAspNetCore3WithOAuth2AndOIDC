﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                 .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            // create an HttpClient used for accessing the API
            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44366/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            // sle note: should image that this works with the [Authorisation] attribute (I can see this being passed into the constructor!!!)
            // sle note: here we configure what Kevin Dockx calls the middleware OpenID, using Microsoft.AspNetCore.Authentication.OpenIdConnect
            services.AddAuthentication(options =>
               {
                   options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                   options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
               }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,options =>
               {
                   options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                   options.Authority = "https://localhost:5001/"; // sle note: point to  Marvin.IDP
                   options.ClientId = "imagegalleryclient";
                   options.ResponseType = "code";
                   options.UsePkce = true;                  
                   options.Scope.Add("openid");
                   options.Scope.Add("profile");
                   options.SaveTokens = true;
                   options.ClientSecret = "secret";
                   //options.CallbackPath = new Microsoft.AspNetCore.Http.PathString("...");                  
               });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
 
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
                // The default HSTS value is 30 days. You may want to change this for
                // production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
