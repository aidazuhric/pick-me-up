using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Services;
using PickMeUp.Services.Database;
using PickMeUp.Services.Validators;
using PickMeUp.WebAPI.Filters;
using PickMeUp.WebAPI.Services;
using PickMeUp.WebAPI.Services.AccessManager;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthenticatedUserAccessor, HttpAuthenticatedUserAccessor>();

builder.Services.AddControllers(
   options => options.Filters.Add<ExceptionFilter>()
);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PickMeUpDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddMapster();

TypeAdapterConfig<User, UserResponse>.NewConfig()
    .Map(dest => dest.Role, src => src.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? string.Empty)
    .IgnoreNullValues(true);
TypeAdapterConfig<UserUpdateRequest, User>.NewConfig().IgnoreNullValues(true);

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<IAccessManager, AccessManager>();

builder.Services.AddScoped<ICryptoService, CryptoService>();

builder.Services.AddScoped<IValidator<UserInsertRequest>, UserInsertValidator>();
builder.Services.AddScoped<IValidator<UserUpdateRequest>, UserUpdateValidator>();

builder.Services.AddOpenApi();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtToken:Issuer"],
        ValidAudience = builder.Configuration["JwtToken:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtToken:SecretKey"] ?? string.Empty)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Version = "v1",
            Title = "PickMeUp API",
            Description = "Template API providing base CRUD infrastructure and user/authentication management"
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PickMeUpDbContext>();
    db.Database.Migrate();
}

{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
