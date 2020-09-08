using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddConfiguration(configuration)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
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
            
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("http://localhost:4200", "http://localhost:8080")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Converters.Add(new ObjectIdConverter());
                });
            services.AddControllers(options =>
                {
                    options.ModelBinderProviders.Insert(0, new ObjectIdModelBinderProvider());
                })
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
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            var hubPaths = new[]
                            {
                                "/updates/classes"
                            };
                            
                            if (!string.IsNullOrEmpty(accessToken) &&
                                hubPaths.Any(hubPath => path.StartsWithSegments(hubPath)))
                            {
                                context.Token = accessToken;
                            }
                            
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddGoogleIdToken(options =>
                {

                });

#if DEBUG
            var url = new MongoUrl(connectionString);
            var client = new MongoClient(url);
            var database = client.GetDatabase(url.DatabaseName);
            var users = database.GetCollection<User>("users");
            var danceHalls = database.GetCollection<DanceHall>("dance_halls");

            if (!users.AsQueryable().Any() || !danceHalls.AsQueryable().Any())
            {
                var teacherIds = new[] { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() };
                
                users.InsertMany(new []
                {
                    new User
                    {
                        Id = teacherIds[0],
                        Name = "Test Teacher 1",
                        Surname = "Test",
                        GivenName = "Teacher 1",
                        UserName = "TestTeacher1",
                        Email = "test.teacher.1@gmail.com"
                    },
                    new User
                    {
                        Id = teacherIds[1],
                        Name = "Test Teacher 2",
                        Surname = "Test",
                        GivenName = "Teacher 2",
                        UserName = "TestTeacher2",
                        Email = "test.teacher.2@gmail.com"
                    }
                });
                
                danceHalls.InsertOne(new DanceHall
                {
                    Address = "6724 Szeged",
                    Room = "215",
                    Classes = new List<Class>
                    {
                        new BallroomDanceClass
                        {
                            Id = ObjectId.GenerateNewId(),
                            Name = "Angol keringő",
                            DayOfWeek = DayOfWeek.Friday,
                            StartsAt = TimeSpan.FromHours(8),
                            EndsAt = TimeSpan.FromHours(10),
                            Teacher = teacherIds[0],
                            Pairs = new List<List<ObjectId>>
                            {
                                // new List<ObjectId> { ObjectId.GenerateNewId(), ObjectId.GenerateNewId() },
                                // new List<ObjectId> { ObjectId.GenerateNewId() }
                            }
                        },
                        new GroupDanceClass
                        {
                            Id = ObjectId.GenerateNewId(),
                            Name = "Hip-hop",
                            DayOfWeek = DayOfWeek.Wednesday,
                            StartsAt = TimeSpan.FromHours(15),
                            EndsAt = TimeSpan.FromHours(17),
                            Teacher = teacherIds[1],
                            Dancers = new List<ObjectId>
                            {
                                // ObjectId.GenerateNewId(),
                                // ObjectId.GenerateNewId(),
                                // ObjectId.GenerateNewId()
                            }
                        }
                    }
                });
            }
#endif
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
                endpoints.MapHub<DanceClassHub>("/updates/classes");
            });
        }
    }
}