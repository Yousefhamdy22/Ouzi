using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;


namespace Infrastructure
{
    public static class ServicesOfRegisteration
    {
        public static IServiceCollection AddServiceRegisteration(this IServiceCollection services,
                                                                      IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.Bind("jwtSettings", jwtSettings);
            services.AddSingleton(jwtSettings);

            // Identity setup
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters
                = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<RestaurantDbContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication setup
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //sechma
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //.AddJwtBearer(options =>
            //{
            //    options.RequireHttpsMetadata = false;
            //    options.SaveToken = true;
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        //ValidateIssuer = jwtSettings.ValidateIssuer,
            //        ValidIssuer = jwtSettings.Issuer,
            //        ValidateAudience = jwtSettings.ValidateAudience,
            //        ValidAudience = jwtSettings.Audience,
            //        ValidateLifetime = jwtSettings.ValidateLifetime,
            //        ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
            //        NameClaimType = ClaimTypes.NameIdentifier, // This is important!
            //        RoleClaimType = ClaimTypes.Role,
            //        ClockSkew = TimeSpan.Zero,
            //        RequireExpirationTime = true,
            //        LifetimeValidator = (notBefore, expires, token, parameters) =>
            //        {
            //            return expires != null && expires > DateTime.UtcNow;
            //        },
            //        SaveSigninToken = true
            //    };

            //    // Comprehensive JWT Bearer Events
            //    options.Events = new JwtBearerEvents
            //    {
            //        // Handle 401 - Unauthorized (invalid/missing token)
            //        OnChallenge = async context =>
            //        {
            //            context.HandleResponse();
            //            context.Response.StatusCode = 401;
            //            context.Response.ContentType = "application/json";

            //            var response = new
            //            {
            //                error = "Unauthorized",
            //                message = "Authentication failed. Token is invalid, expired, or missing.",
            //                statusCode = 401,
            //                timestamp = DateTime.UtcNow
            //            };

            //            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            //        },

            //        // Handle authentication failures
            //        OnAuthenticationFailed = async context =>
            //        {
            //            context.Response.StatusCode = 401;
            //            context.Response.ContentType = "application/json";

            //            var response = new
            //            {
            //                error = "Authentication Failed",
            //                message = "Token validation failed: " + context.Exception.Message,
            //                statusCode = 401,
            //                timestamp = DateTime.UtcNow
            //            };

            //            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            //        },

            //        // Handle forbidden access (token valid but insufficient permissions)
            //        OnForbidden = async context =>
            //        {
            //            context.Response.StatusCode = 403;
            //            context.Response.ContentType = "application/json";

            //            var response = new
            //            {
            //                error = "Forbidden",
            //                message = "Access denied. You don't have permission to access this resource.",
            //                statusCode = 403,
            //                timestamp = DateTime.UtcNow
            //            };

            //            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            //        }
            //    };
            //});
            // 1. FIXED JWT Configuration
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // FIXED: Enable issuer validation
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true, // Use your config value
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)), // FIXED: Use UTF8
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };

                // Enhanced debugging events
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        Console.WriteLine($"[JWT] Authorization header: {authHeader ?? "MISSING"}");

                        if (authHeader?.StartsWith("Bearer ") == true)
                        {
                            var token = authHeader.Substring("Bearer ".Length);
                            Console.WriteLine($"[JWT] Token extracted: {token.Substring(0, Math.Min(50, token.Length))}...");
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[JWT] Authentication failed: {context.Exception.Message}");
                        Console.WriteLine($"[JWT] Exception type: {context.Exception.GetType().Name}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("[JWT] Token validated successfully");
                        var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var roles = context.Principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                        Console.WriteLine($"[JWT] User: {userId}, Roles: [{string.Join(", ", roles)}]");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"[JWT] Challenge: {context.Error} - {context.ErrorDescription}");
                        Console.WriteLine($"[JWT] Request path: {context.Request.Path}");
                        return Task.CompletedTask;
                    }
                };
            });

            // Authorization policies with better error handling
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Create", policy =>
                {
                    policy.RequireClaim("Create", "True");
                });

                options.AddPolicy("Delete", policy =>
                {
                    policy.RequireClaim("Delete", "True");
                });

                options.AddPolicy("Edit", policy =>
                {
                    policy.RequireClaim("Edit", "True");
                });

                // Add role-based policies as examples
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole("Admin");
                });

                options.AddPolicy("UserOrAdmin", policy =>
                {
                    policy.RequireRole("User", "Admin");
                });

                //// Custom authorization requirement example
                //options.AddPolicy("MinimumAge", policy =>
                //{
                //    policy.Requirements.Add(new MinimumAgeRequirement(18));
                //});

                //// Set default policy
                //options.DefaultPolicy = new AuthorizationPolicyBuilder()
                //    .RequireAuthenticatedUser()
                //    .Build();
            });

            return services;
        }
    }

}
