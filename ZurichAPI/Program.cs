using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ZurichAPI.Data.SQL;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ZurichAPI.Data.SQL.Implementations;
using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Infrastructure.Implementations;
using ZurichAPI.Infrastructure.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionLocal"),
       o => o.UseCompatibilityLevel(120).CommandTimeout(Convert.ToInt32(TimeSpan.FromMinutes(10).TotalSeconds))));

// Core
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IClientRepository, ClientRepository>();

// Infraestructure
builder.Services.AddTransient<IDataAccessLogs, DataAccessLogs>();
builder.Services.AddTransient<IDataAccessUser, DataAccessUser>();
builder.Services.AddTransient<IDataAccessClient, DataAccessClient>();

// Add services to the container.

builder.Services.AddControllers();

var corsPolicy = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("profile.self.edit", policy =>
        policy.RequireClaim("perm", "profile.self.edit"));

    options.AddPolicy("policies.self.view", policy =>
        policy.RequireClaim("perm", "policies.self.view"));

    options.AddPolicy("policies.self.cancel", policy =>
        policy.RequireClaim("perm", "policies.self.cancel"));

    options.AddPolicy("clients.manage", policy =>
        policy.RequireClaim("perm", "clients.manage"));

    options.AddPolicy("policies.manage", policy =>
        policy.RequireClaim("perm", "policies.manage"));
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Zurich API ", Version = "v1" });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Encabezado de autorizacion JSON Web Token utilizando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
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
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ZurichAPI:";
});

builder.Services.AddSingleton<ICacheService, RedisCacheService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
