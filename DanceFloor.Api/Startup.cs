using System;
using System.Collections.Generic;
using System.Text;
using AspNetCore.Identity.MongoDbCore.Models;
using DanceFloor.Api.Converters;
using DanceFloor.Api.Hubs;
using DanceFloor.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository;

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

#if DEBUG
            var url = new MongoUrl(connectionString);
            var client = new MongoClient(url);
            var database = client.GetDatabase(url.DatabaseName);
            var danceHalls = database.GetCollection<DanceHall>("dance_halls");

            if (!danceHalls.AsQueryable().Any())
            {
                danceHalls.InsertOne(new DanceHall
                {
                    Address = "6724 Szeged",
                    Room = "215",
                    Lessons = new List<Lesson>
                    {
                        new BallroomDanceLesson
                        {
                            Id = ObjectId.GenerateNewId(),
                            StartsAt = TimeSpan.FromHours(8),
                            EndsAt = TimeSpan.FromHours(10),
                            Teacher = ObjectId.GenerateNewId(),
                            Pairs = new List<List<ObjectId>>
                            {
                                new List<ObjectId> { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() },
                                new List<ObjectId> { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() }
                            }
                        },
                        new DanceLesson
                        {
                            Id = ObjectId.GenerateNewId(),
                            StartsAt = TimeSpan.FromHours(15),
                            EndsAt = TimeSpan.FromHours(17),
                            Teacher = ObjectId.GenerateNewId(),
                            Dancers = new List<ObjectId>
                            {
                                ObjectId.GenerateNewId(),
                                ObjectId.GenerateNewId(),
                                ObjectId.GenerateNewId()
                            }
                        }
                    }
                });
            }

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
#endif

            services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Converters.Add(new ObjectIdConverter());
                });
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new ObjectIdConverter());
                });
            services.AddSwaggerGen();

            var mongoDbContext = new MongoDbContext(connectionString);
            
            services.AddIdentity<User, MongoIdentityRole<ObjectId>>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddMongoDbStores<IMongoDbContext>(mongoDbContext)
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var jwtSection = Configuration.GetSection("Jwt");

                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.Audience = jwtSection["Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtSection["Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"])),
                        ClockSkew = TimeSpan.Zero,
                        RequireExpirationTime = false,
                    };
                })
                .AddGoogleIdToken(options =>
                {

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

            app.UseCors("CorsPolicy");
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<LessonHub>("/updates/lessons");
            });
        }
    }
}