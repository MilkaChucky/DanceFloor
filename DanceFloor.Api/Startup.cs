using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DanceFloor.Api
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
            var connectionString = Configuration.GetConnectionString("DanceFloor");

            services.AddSingleton(new MongoUrl(connectionString));
            services.AddTransient<IMongoClient>(provider =>
            {
                var mongoUrl = provider.GetService<MongoUrl>();
                return new MongoClient(mongoUrl);
            });
            services.AddTransient(provider =>
            {
                var mongoUrl = provider.GetService<MongoUrl>();
                return provider.GetService<IMongoClient>().GetDatabase(mongoUrl.DatabaseName);
            });

            services.AddTransient(provider =>
                provider.GetService<IMongoDatabase>().GetCollection<User>("users"));
            services.AddTransient(provider =>
                provider.GetService<IMongoDatabase>().GetCollection<DanceHall>("dance_halls"));
            
            services.AddControllers();
            services.AddSwaggerGen();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DanceFloor API v1");
                options.RoutePrefix = string.Empty;
            });
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}