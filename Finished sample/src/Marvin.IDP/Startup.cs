// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Marvin.IDP
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
           

            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            // sle note: configure middleware with Users. This IDP does not provide an interface to create  NEW users??
            var builder = services.AddIdentityServer()
                //.AddInMemoryIdentityResources(Config.Ids)
                //.AddInMemoryApiResources(Config.Apis)
                //.AddInMemoryClients(Config.Clients)
                .AddTestUsers(TestUsers.Users);

            // sle note: switch to developer to test without certificates installed!!
            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
            //builder.AddSigningCredential(LoadCertificateFromStore());

            var migrationsAssembly = typeof(Startup)
                .GetTypeInfo().Assembly.GetName().Name;


            // sle note: to initialise the EF database provided by the IdentityServer4.EntityFramework nuget package
            //  In this directory: run manually powershell "add-migration -InitialServerIdentityServerConfigurationDBMigration -context ConfigurationDbContext
            //     to create the database. Location: C:\Users\fallb\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB
            var marvinIDPDataDBConnectionString =
               "Server=(localdb)\\mssqllocaldb;Database=MarvinIDPDataDB;Trusted_Connection=True;";

            builder.AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = (builder) => {
                    builder.UseSqlServer(
                        marvinIDPDataDBConnectionString, (options) => 
                        { 
                            options.MigrationsAssembly(migrationsAssembly); 
                        }
                    );
                };
            });

            builder.AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder => 
                    builder.UseSqlServer(marvinIDPDataDBConnectionString,
                    options => options.MigrationsAssembly(migrationsAssembly));
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            InitializeDatabase(app);

            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        public X509Certificate2 LoadCertificateFromStore()
        {
            string thumbPrint = "d4d681b3de4cd26fc030292aeea170e553810bdb";

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint,
                    thumbPrint, true);
                if (certCollection.Count == 0)
                {
                    throw new Exception("The specified certificate wasn't found.");
                }
                return certCollection[0];
            }
        }


        // sle note: to initialise the EF database provided by the IdentityServer4.EntityFramework nuget package
        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetService<IServiceScopeFactory>().CreateScope())
            {
                // sle note: In the following lines there appears to be two dbContexts sharinng the same database: GRANT AND CONFIGURATION.

                // sle note: Create the GRANT database if not already created. 
                serviceScope.ServiceProvider
                    .GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider
                    .GetRequiredService<ConfigurationDbContext>(); // sle note: ConfigurationDbContext is in the Indentity4Service nuget package

                // sle note: Create the CONFIGURATION database if not already created.
                context.Database.Migrate();


                if (!context.Clients.Any())
                {
                    // sle note: replaces the commented-out code  '//.AddInMemoryIdentityResources(Config.Ids)'
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    // sle note: replaces the commented-out code
                    foreach (var resource in Config.Ids)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    // sle note: replaces the commented-out code
                    foreach (var resource in Config.Apis)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }


    }
}
