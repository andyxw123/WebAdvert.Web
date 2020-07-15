using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebAdvert.Web
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
            services.AddControllersWithViews();

            //This injects all of the dependencies needed to use Cognito.
            //An "AWS" section has added to appsettings.json which contains the client key & secret.
            services.AddCognitoIdentity(config => {
                config.Password = new Microsoft.AspNetCore.Identity.PasswordOptions
                {
                    //Set the password options to match the options in Cognito (otherwise an error will occur if the password isn't complex enough).
                    //May not need to do this now as the AWS SDK may get the options from Cognito.
                    //NB. Wouldn't allow the password rules to be this loose for a production system.
                    RequireDigit = false,
                    RequiredLength = 6,
                    RequiredUniqueChars = 0,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false
                };
            });

            //Set the location of the login page (there are other options which can be set here as well
            //such as the ReturnUrlParameter etc)
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Accounts/Login";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            //This was added so that Cognito is used for Authentication... services.AddCognitoIdentity(); added above
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
