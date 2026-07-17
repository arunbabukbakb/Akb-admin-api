using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.IO;

using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Data;
using Data.Repository.IRepository;
using Models.ViewModels;
using Data.Repository;
using Infrastructure;
using Models.DtoMapper;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using adminapi.Middleware;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog to log to a file
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog(); // Use Serilog as the logging provider

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("akbpolicy",
            builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );
        });

        // Add services to the container.
        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            var Key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]);
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                ValidAudience = builder.Configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ClockSkew = TimeSpan.Zero // Prevents small time differences from allowing expired tokens
            };
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
        ));

        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("MySettings"));

        builder.Services.AddServiceLibrary();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddSingleton(new MobileService("Your Twilio Account SID", "Your Twilio Auth Token", "Your Twilio Phone Number"));
        builder.Services.AddSingleton(new MailService("Your SendGrid API Key"));
        builder.Services.AddScoped<INotificationService, NotificationService>();

        //Auto Mapper  
        builder.Services.AddAutoMapper(typeof(AutoMappingProfile).Assembly);

        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            // Enable JWT Bearer token support in Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter **only** your JWT token below (without 'Bearer ')"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
            });
        });
        // Initialize Firebase safely
        try
        {
            var webRoot = builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
            var notificationJsonPath = Path.Combine(webRoot, "files", "notification.json");
            if (File.Exists(notificationJsonPath))
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(notificationJsonPath),
                });
                Log.Information("FirebaseApp initialized successfully using notification.json.");
            }
            else
            {
                Log.Warning("Firebase credential file not found at: {Path}. Push notifications will fail if invoked.", notificationJsonPath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize FirebaseApp.");
        }



        var app = builder.Build();

        app.UseMiddleware<ExceptionMiddleware>();



        app.UseCors("akbpolicy");

        app.UseSwagger();
        app.UseSwaggerUI();

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.UseSwagger();
        //    app.UseSwaggerUI();
        //    //app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sonic App"); });
        //}

        //app.Use((context, next) =>
        //{
        //	if (context.Request.Method == "OPTIONS")
        //	{
        //		context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        //		context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        //		context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        //		context.Response.StatusCode = 200;
        //		return context.Response.CompleteAsync();
        //	}

        //	return next();
        //});

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseAuthentication();

        app.MapControllers();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            //endpoints.MapHub<NotificationHub>("/notificationHub");
            // Other endpoints...
        });

        app.Run();
    }
}