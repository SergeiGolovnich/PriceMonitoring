using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static PriceMonitorData.EnvHelper;
using Mobsites.AspNetCore.Identity.Cosmos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using PriceMonitorData;
using PriceMonitorData.Azure;
using PriceMonitorData.SQLite;

namespace PriceMonitorBlazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            switch(Configuration.GetValue<string>("DBProvider", "SQLite"))
            {
                case "SQLite":
                    ConfigureSQLite(services);
                    break;
                case "CosmosDB":
                    ConfigureCosmosDB(services);
                    break;
                default:
                    throw new Exception("Unknown Database Provider");
            }

            services.AddRazorPages();

            services.AddServerSideBlazor();
        }

        private void ConfigureCosmosDB(IServiceCollection services)
        {
            services.AddCosmosStorageProvider(options =>
            {
                options.ConnectionString = GetEnvironmentVariable("CosmosConnStr");
                options.CosmosClientOptions = new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        IgnoreNullValues = false
                    }
                };
                options.DatabaseId = "PriceMonitorIdentity";
                options.ContainerProperties = new ContainerProperties
                {
                    Id = "Users",
                    //PartitionKeyPath defaults to "/PartitionKey", which is what is desired for the default setup.
                };
            });

            services.AddDefaultCosmosIdentity(options =>
            {
                // User settings
                options.User.RequireUniqueEmail = true;

                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                // Lockout settings
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            }).AddRoles<Mobsites.AspNetCore.Identity.Cosmos.IdentityRole>()
        // Add other IdentityBuilder methods.
        .AddDefaultUI()
        .AddDefaultTokenProviders();

            services.AddScoped<ItemRepository, CosmosItemRepository>(_ => new CosmosItemRepository(GetEnvironmentVariable("CosmosConnStr")));
            services.AddScoped<PriceRepository, CosmosPriceRepository>(_ => new CosmosPriceRepository(GetEnvironmentVariable("CosmosConnStr")));
            services.AddScoped<UserRepository, CosmosUserRepository>(_ => new CosmosUserRepository(GetEnvironmentVariable("CosmosConnStr")));
        }

        private void ConfigureSQLite(IServiceCollection services)
        {
            
            services.AddDbContext<SQLiteIdentityContext>();

            services.AddDefaultIdentity<Microsoft.AspNetCore.Identity.IdentityUser>(options =>
            {
                // User settings
                options.User.RequireUniqueEmail = true;

                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                // Lockout settings
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            }).AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
              .AddEntityFrameworkStores<SQLiteIdentityContext>();

            services.AddScoped<ItemRepository, SQLiteItemRepository>(_ => new SQLiteItemRepository());
            services.AddScoped<PriceRepository, SQLitePriceRepository>(_ => new SQLitePriceRepository());
            services.AddScoped<UserRepository, SQLiteUserRepository>();

            if (Configuration.GetValue<bool>("InternalPriceChecker", true)) 
            {
                PriceCheckerService.Start(TimeSpan.FromHours(Configuration.GetValue<double>("PriceCheckingIntervalHours", 4)).TotalMilliseconds, new SQLiteItemRepository(), new SQLitePriceRepository());
                PriceCheckerService.CheckPrices(null, null);
            }            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IWebHostEnvironment env,
            RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager, 
            UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            // Add Admin role.
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Name = "Admin"
                }).Wait();
            }

            //Add Default Admin
            var adminInDBQuery = userManager.FindByEmailAsync("admin@pricemonitor.com");
            adminInDBQuery.Wait();
            if (adminInDBQuery.Result == null)
            {
                var admin = new Microsoft.AspNetCore.Identity.IdentityUser("admin@pricemonitor.com") 
                { 
                    Email = "admin@pricemonitor.com"
                };

                var result = userManager.CreateAsync(admin, "123456Abc!");
                result.Wait();

                userManager.AddToRoleAsync(admin, "Admin").Wait();
            }
        }
    }
}
