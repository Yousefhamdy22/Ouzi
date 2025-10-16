using Application;
using Application.Mapping;
using Application.Options;
using Application.Services;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WembyResturant
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            #region Sql Connection

            builder.Services.AddDbContext<RestaurantDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            #endregion

            #region IoC 

            builder.Services.AddInfrastructureDependencies().AddServiceDependencies()
                .AddModuleCoreDependencies().AddServiceRegisteration(builder.Configuration);

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.Configure<FawaterakOptions>(builder.Configuration.GetSection("Fawaterak"));


            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole(UserRole.Admin.ToString()));

                options.AddPolicy("TeacherOnly", policy =>
                    policy.RequireRole(UserRole.Customer.ToString()));

                options.AddPolicy("AdminOrTeacher", policy =>
                    policy.RequireRole(UserRole.Admin.ToString(), UserRole.User.ToString()));
            });

            #region Cors

            var Cors = "_cors";
            builder.Services.AddCors(options =>
              options.AddPolicy(name: Cors, builder =>

              {
                  builder.AllowAnyHeader();
                  builder.AllowAnyMethod();
                  builder.AllowAnyOrigin();
              }
                  )
            );

            #endregion

            #endregion

            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            // For controllers
          

            // Add AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<OrdersProfile>());


            builder.Services.AddHttpClient();
       
            builder.Services.AddControllers()
               .AddJsonOptions(options =>
               {
                   options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                   options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                   options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
               });


            #region Swagger Config

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "E-Learning API",
                    Version = "v1",
                    Description = "API for E-Learning Platform"
                });

                // Add JWT Bearer authentication support
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
              
                // Include XML comments if available
                try
                {
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                }
                catch
                {
                    // Ignore if XML comments aren't available
                }
            });
            #endregion

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                // Seed roles
                var roles = Enum.GetValues<UserRole>();
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role.ToString()))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role.ToString()));
                        logger.LogInformation("Role {Role} created", role.ToString());
                    }
                }
            }

            #region MiddleWare


            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Learning API V1");
                c.RoutePrefix = "swagger"; // This makes it available at /swagger

                // For production, you might want to hide the Swagger UI
                // but keep the JSON available for API consumers
                if (!app.Environment.IsDevelopment())
                {
                    c.DocumentTitle = "API Documentation - Production";
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(Cors);



            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();


            #endregion
        }
    }
}
