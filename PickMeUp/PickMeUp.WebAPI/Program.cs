using PickMeUp.Common.Services.Configuration;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

EnvironmentConfigurationExtensions.LoadEnvFile();

var builder = WebApplication.CreateBuilder(args);

var startupOptions = builder.Services.AddEnvironmentConfiguration();

const string CorsPolicyName = "PickMeUpCors";

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthenticatedUserAccessor, HttpAuthenticatedUserAccessor>();

builder.Services.AddControllers(
   options => options.Filters.Add<ExceptionFilter>()
);

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(startupOptions.Api.CorsAllowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<PickMeUpDbContext>(options =>
    options.UseNpgsql(startupOptions.Database.ConnectionString)
);

builder.Services.AddMapster();

TypeAdapterConfig<User, UserResponse>.NewConfig()
    .Map(dest => dest.Role, src => src.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? string.Empty)
    .Map(dest => dest.ProfileImageUrl, src => src.ProfileImagePath)
    .IgnoreNullValues(true);
TypeAdapterConfig<UserUpdateRequest, User>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<CityUpdateRequest, City>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<VehicleCategoryUpdateRequest, VehicleCategory>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<OrderStatusUpdateRequest, OrderStatus>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<SupportTicketStatusUpdateRequest, SupportTicketStatus>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<PaymentStatusUpdateRequest, PaymentStatus>.NewConfig().IgnoreNullValues(true);

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IVehicleCategoryService, VehicleCategoryService>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<ISupportTicketStatusService, SupportTicketStatusService>();
builder.Services.AddScoped<IPaymentStatusService, PaymentStatusService>();

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<IAccessManager, AccessManager>();

builder.Services.AddScoped<ICryptoService, CryptoService>();

builder.Services.AddScoped<IValidator<UserInsertRequest>, UserInsertValidator>();
builder.Services.AddScoped<IValidator<UserUpdateRequest>, UserUpdateValidator>();
builder.Services.AddScoped<IValidator<CityInsertRequest>, CityInsertValidator>();
builder.Services.AddScoped<IValidator<CityUpdateRequest>, CityUpdateValidator>();
builder.Services.AddScoped<IValidator<VehicleCategoryInsertRequest>, VehicleCategoryInsertValidator>();
builder.Services.AddScoped<IValidator<VehicleCategoryUpdateRequest>, VehicleCategoryUpdateValidator>();
builder.Services.AddScoped<IValidator<OrderStatusInsertRequest>, OrderStatusInsertValidator>();
builder.Services.AddScoped<IValidator<OrderStatusUpdateRequest>, OrderStatusUpdateValidator>();
builder.Services.AddScoped<IValidator<SupportTicketStatusInsertRequest>, SupportTicketStatusInsertValidator>();
builder.Services.AddScoped<IValidator<SupportTicketStatusUpdateRequest>, SupportTicketStatusUpdateValidator>();
builder.Services.AddScoped<IValidator<PaymentStatusInsertRequest>, PaymentStatusInsertValidator>();
builder.Services.AddScoped<IValidator<PaymentStatusUpdateRequest>, PaymentStatusUpdateValidator>();

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
        ValidIssuer = startupOptions.Jwt.Issuer,
        ValidAudience = startupOptions.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(startupOptions.Jwt.SecretKey)),
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
