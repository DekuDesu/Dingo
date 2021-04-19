using System;
using Dingo.Data;
using DingoDataAccess;
using DingoDataAccess.Account;
using DingoDataAccess.Models.Friends;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using DingoAuthentication.Encryption;

[assembly: HostingStartup(typeof(Dingo.Areas.Identity.IdentityHostingStartup))]
namespace Dingo.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                // this replaces the default hashing with bycrypt SHA512 hashing
                services.AddTransient(typeof(IPasswordHasher<IdentityUser>), typeof(DingoAuthentication.PasswordHasher<IdentityUser>));
                services.AddTransient<ISqlDataAccess, SqlDataAccess>();
                services.AddTransient<IFullDisplayNameModel, FullDisplayNameModel>();
                services.AddSingleton(typeof(IDisplayNameHandler), typeof(DisplayNameHandler<FullDisplayNameModel>));
                services.AddSingleton(typeof(IAccountHandler), typeof(AccountHandler));
                services.AddTransient<IStatusHandler, StatusHandler>();
            });
        }
    }
}