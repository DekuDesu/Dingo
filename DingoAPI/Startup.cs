using DingoAuthentication.Encryption;
using DingoDataAccess;
using DingoDataAccess.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingoAPI
{
    public class Startup
    {
        internal static byte[] X509IdentityKey { get; set; }
        internal static byte[] PrivateIdentityKey { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Loads Keys
            X509IdentityKey = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(Configuration["Server:X509IdentityKey"]);
            PrivateIdentityKey = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(Configuration["Server:PrivateIdentityKey"]);

            services.AddTransient<ISqlDataAccess, SqlDataAccess>();

            services.AddTransient(typeof(ISymmetricHandler<EncryptedDataModel>), typeof(SymmetricHandler<EncryptedDataModel>));
            services.AddTransient<IDiffieHellmanHandler, DiffieHellmanHandler>();
            services.AddTransient<IKeyDerivationFunction, KeyDerivationFunction>();
            services.AddTransient(typeof(IKeyDerivationRatchet<EncryptedDataModel>), typeof(KeyDerivationRatchet<EncryptedDataModel>));
            services.AddTransient<IKeyBundleModel, KeyBundleModel>();
            services.AddTransient<ISignedKeyModel, SignedKeyModel>();
            services.AddTransient<ISignatureHandler, SignatureHandler>();

            services.AddTransient<IDiffieHellmanRatchet, DiffieHellmanRatchet>();

            services.AddTransient<IEncryptedMessageModel, EncryptedMessageModel>();

            services.AddTransient<IOAuthHandler, OAuthHandler>();

            // the type(dynamic) provided here is not actually used FYI it can be anything, but it MUST be injected using whatever you put here as a type argument
            services.AddTransient(typeof(IPasswordHasher<dynamic>), typeof(DingoAuthentication.PasswordHasher<dynamic>));

            services.AddCors(options =>
            {
                options.AddPolicy(name: "DefaultPolicy",
                    builder => { builder.AllowAnyOrigin(); });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DingoAPI", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DingoAPI v1"));
            }

            app.UseHttpsRedirection();

            // make sure the page requests are logged with serilog since we disabled this in the appsettings.json
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseCors("DefaultPolicy");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (env.IsDevelopment())
            {
                Serilog.Log.Information("Development Secrets Loaded IdentityKey: {X509IdentityKey} PrivateKey: {PrivateKey}", X509IdentityKey, PrivateIdentityKey);
            }
        }
    }
}
