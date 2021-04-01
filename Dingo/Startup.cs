using Dingo.Areas.Identity;
using Dingo.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using DingoDataAccess.Account;
using DingoDataAccess;
using DingoDataAccess.Models.Friends;
using Dingo.Data.Validators;
using Dingo.Data.GeneralModels;
using FluentValidation;
using Dingo.Data.UserInfo;
using Microsoft.AspNetCore.Components.Server.Circuits;
using DingoDataAccess.OAuth;
using DingoAuthentication.Encryption;

namespace Dingo
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
            services.AddSingleton<CircuitHandler, TrackedCircuitHandler>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            services.AddSingleton<IEmailSender, DingoDataAccess.Email.EmailSender>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

            services.AddSingleton<WeatherForecastService>();

            // access to databases
            services.AddTransient<ISqlDataAccess, SqlDataAccess>();

            services.AddTransient<ITopLevelObjects, TopLevelObjects>();


            services.AddSingleton<IFullDisplayNameModel, FullDisplayNameModel>();

            // defines a friend that holds information about users in general
            services.AddTransient<IFriendModel, FriendModel>();

            // object caches queries in memory and needs to be a singleton
            services.AddSingleton(typeof(IDisplayNameHandler), typeof(DisplayNameHandler<FullDisplayNameModel>));

            // gets and sets friend lists
            services.AddTransient(typeof(IFriendListHandler), typeof(FriendListHandler<FriendModel>));

            // object caches queries in memory and needs to be a singleton
            services.AddSingleton(typeof(IFriendHandler), typeof(FriendHandler<FriendModel>));

            // handles db queries to get and set the online status of accounts
            services.AddTransient<IStatusHandler, StatusHandler>();

            // validates user data
            services.AddTransient(typeof(IValidator<DisplayNameModel>), typeof(DisplayNameValidator));

            services.AddTransient(typeof(IValidator<SingleSearchTermModel>), typeof(SingleSearchTermValidator));

            // gets and sets the Oath api key for users
            services.AddTransient<IOAuthHandler, OAuthHandler>();

            // gets and sets the messages between users
            services.AddTransient<IMessageHandler, MessageHandler>();

            services.AddTransient(typeof(ISymmetricHandler<EncryptedDataModel>), typeof(SymmetricHandler<EncryptedDataModel>));

            services.AddTransient<IDiffieHellmanHandler, DiffieHellmanHandler>();

            services.AddTransient<IKeyDerivationFunction, KeyDerivationFunction>();

            services.AddTransient(typeof(IKeyDerivationRatchet<EncryptedDataModel>), typeof(KeyDerivationRatchet<EncryptedDataModel>));

            services.AddTransient(typeof(IKeyBundleModel<SignedKeyModel>), typeof(KeyBundleModel<SignedKeyModel>));

            services.AddTransient<ISignedKeyModel, SignedKeyModel>();

            services.AddTransient<ISignatureHandler, SignatureHandler>();

            services.AddTransient<IDiffieHellmanRatchet, DiffieHellmanRatchet>();

            // gets and sets the states for users
            services.AddTransient<IEncryptedClientStateHandler, EncryptedClientStateHandler>();

            services.AddTransient(typeof(IKeyAndBundleHandler<KeyBundleModel<SignedKeyModel>, SignedKeyModel>), typeof(KeyAndBundleHandler<KeyBundleModel<SignedKeyModel>, SignedKeyModel>));

            services.AddTransient(typeof(IEncryptionClient<EncryptedDataModel, SignedKeyModel>), typeof(EncryptionClient<EncryptedDataModel, KeyBundleModel<SignedKeyModel>, SignedKeyModel>));

            services.AddTransient(typeof(IBundleProcessor), typeof(BundleProcessor<KeyBundleModel<SignedKeyModel>, EncryptedDataModel, SignedKeyModel>));

            services.AddTransient<IAvatarHandler, AvatarHandler>();

            services.AddTransient(typeof(IConcurrentTimerDictionary), typeof(ConcurrentTimerDictionary<Microsoft.Extensions.Logging.ILogger<TopLevelObjects>>));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // make sure the page requests are logged with serilog since we disabled this in the appsettings.json
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
