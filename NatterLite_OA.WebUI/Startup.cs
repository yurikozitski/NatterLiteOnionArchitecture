using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using NatterLite_OA.Infrastructure.Data;
using NatterLite_OA.Infrastructure.Repositories;
using NatterLite_OA.Infrastructure.Services;
using NatterLite_OA.Core.Models;
using NatterLite_OA.Core.RepositoryInterfaces;
using NatterLite_OA.Core.ServiceInterfaces;
using NatterLite_OA.WebUI.Filters;
using NatterLite_OA.WebUI.SignalR;


namespace NatterLite_OA.WebUI
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
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>(opts =>
            {
                opts.Password.RequiredLength = 8;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
                opts.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789'-@";
            })
                .AddEntityFrameworkStores<ApplicationContext>();
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Account/Login");
            });

            services.AddResponseCompression();

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddSingleton<IPicturesProvider, DefaultUserPicturesProvider>();

            services.AddSingleton<ICountryList, GetCountryList>();

            services.AddSingleton<IImageValidator, ImageValidator>();

            services.AddSingleton<IDataInitializer, DataInitializer>();

            services.AddTransient<IUserRepository,UserRepository>();

            services.AddTransient<IChatRepository,ChatRepository>();

            services.AddSignalR();

            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            services.AddControllersWithViews();

            services.AddScoped<IsBannedFilter>();
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
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseResponseCompression();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Chat}/{action=ChatMenu}");
                endpoints.MapHub<ChatHub>("/chat");
            });
        }
    }
}
