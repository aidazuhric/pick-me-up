using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using PickMeUp.Common.Services.Configuration;
using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Responses;
using PickMeUp.Services;
using PickMeUp.Services.Database;
using PickMeUp.Services.Messaging;
using PickMeUp.Services.Notifications;
using PickMeUp.Services.Orders;
using PickMeUp.Services.Payments;
using PickMeUp.Services.Recommendations;
using PickMeUp.Services.Reports;
using PickMeUp.Services.Storage;
using PickMeUp.Services.Validators;
using PickMeUp.WebAPI.Filters;
using PickMeUp.WebAPI.Hubs;
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
using System.Text.Json.Serialization;

EnvironmentConfigurationExtensions.LoadEnvFile();

var builder = WebApplication.CreateBuilder(args);

var startupOptions = builder.Services.AddEnvironmentConfiguration();

const string CorsPolicyName = "PickMeUpCors";

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddScoped<IAuthenticatedUserAccessor, HttpAuthenticatedUserAccessor>();

builder.Services.AddControllers(
   options => options.Filters.Add<ExceptionFilter>()
).AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(startupOptions.Api.CorsAllowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<PickMeUpDbContext>(options =>
    options.UseNpgsql(startupOptions.Database.ConnectionString)
);

builder.Services.AddMapster();

TypeAdapterConfig<User, UserResponse>.NewConfig()
    .Map(dest => dest.Role, src => src.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? string.Empty)
    .Map(dest => dest.ProfileImageUrl, src => src.ProfileImagePath == null ? null : "Users/" + src.Id + "/Photo")
    .Map(dest => dest.TwoFactorEnabled, src => src.TwoFactorMode != TwoFactorModeEnum.Off)
    .Map(dest => dest.CityName, src => src.City == null ? null : src.City.Name)
    .IgnoreNullValues(true);
TypeAdapterConfig<User, UserSensitiveResponse>.NewConfig()
    .Inherits<User, UserResponse>();
TypeAdapterConfig<UserUpdateRequest, User>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<TaxiCompany, TaxiCompanyResponse>.NewConfig()
    .Map(dest => dest.CityName, src => src.City.Name)
    .Map(dest => dest.LogoImageUrl, src => src.LogoImagePath == null ? null : "TaxiCompanies/" + src.Id + "/Logo")
    .Ignore(dest => dest.InitialLoginUsername!);
TypeAdapterConfig<TaxiCompanyUpdateRequest, TaxiCompany>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<Driver, DriverResponse>.NewConfig()
    .Map(dest => dest.FullName, src => src.User.FirstName + " " + src.User.LastName)
    .Map(dest => dest.Email, src => src.User.Email)
    .Map(dest => dest.PhoneNumber, src => src.User.PhoneNumber)
    .Map(dest => dest.CompanyName, src => src.TaxiCompany.Name)
    .Ignore(dest => dest.InitialLoginUsername!);
TypeAdapterConfig<Car, CarResponse>.NewConfig()
    .Map(dest => dest.CategoryName, src => src.VehicleCategory.Name)
    .Map(dest => dest.CompanyName, src => src.TaxiCompany.Name)
    .Map(dest => dest.PhotoImageUrl, src => src.PhotoImagePath == null ? null : "Cars/" + src.Id + "/Photo");
TypeAdapterConfig<CarUpdateRequest, Car>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<SupportTicketReply, SupportTicketReplyResponse>.NewConfig()
    .Map(dest => dest.AuthorName, src => src.Author.FirstName + " " + src.Author.LastName);
TypeAdapterConfig<SupportTicket, SupportTicketResponse>.NewConfig()
    .Map(dest => dest.SubmitterName, src => src.Submitter.FirstName + " " + src.Submitter.LastName)
    .Map(dest => dest.Status, src => src.SupportTicketStatus.Name)
    .Map(dest => dest.TargetUserName, src => src.TargetUser != null ? src.TargetUser.FirstName + " " + src.TargetUser.LastName : null)
    .Map(dest => dest.ResolvedByName, src => src.ResolvedByUser != null ? src.ResolvedByUser.FirstName + " " + src.ResolvedByUser.LastName : null)
    .Map(dest => dest.Replies, src => src.Replies.OrderBy(r => r.CreatedAt).ToList());
TypeAdapterConfig<Review, ReviewResponse>.NewConfig()
    .Map(dest => dest.AuthorName, src => src.Passenger.FirstName + " " + src.Passenger.LastName)
    .Map(dest => dest.CompanyName, src => src.TargetCompany != null ? src.TargetCompany.Name : null)
    .Map(dest => dest.DriverName, src => src.TargetDriver != null ? src.TargetDriver.User.FirstName + " " + src.TargetDriver.User.LastName : null);
TypeAdapterConfig<PaymentRefund, PaymentRefundResponse>.NewConfig();
TypeAdapterConfig<Payment, PaymentResponse>.NewConfig()
    .Map(dest => dest.PassengerName, src => src.Order.Passenger.FirstName + " " + src.Order.Passenger.LastName)
    .Map(dest => dest.CompanyName, src => src.Order.TaxiCompany != null ? src.Order.TaxiCompany.Name : null)
    .Map(dest => dest.DriverName, src => src.Order.Driver != null ? src.Order.Driver.User.FirstName + " " + src.Order.Driver.User.LastName : null)
    .Map(dest => dest.Status, src => src.PaymentStatus.Name)
    .Map(dest => dest.RefundedTotal, src => src.Refunds.Sum(r => r.RefundedAmount));
TypeAdapterConfig<OrderStatusHistory, OrderStatusHistoryResponse>.NewConfig()
    .Map(dest => dest.FromStatus, src => src.FromStatus != null ? src.FromStatus.Name : null)
    .Map(dest => dest.ToStatus, src => src.ToStatus.Name)
    .Map(dest => dest.ChangedBy, src => src.ChangedByUser.FirstName + " " + src.ChangedByUser.LastName);
TypeAdapterConfig<Order, OrderResponse>.NewConfig()
    .Map(dest => dest.PassengerName, src => src.Passenger.FirstName + " " + src.Passenger.LastName)
    .Map(dest => dest.CompanyName, src => src.TaxiCompany != null ? src.TaxiCompany.Name : null)
    .Map(dest => dest.DriverName, src => src.Driver != null ? src.Driver.User.FirstName + " " + src.Driver.User.LastName : null)
    .Map(dest => dest.CarPlate, src => src.Car != null ? src.Car.Plate : null)
    .Map(dest => dest.Status, src => src.OrderStatus.Name)
    .Map(dest => dest.IsGeneralOrder, src => src.TaxiCompanyId == null);
TypeAdapterConfig<ShiftBreak, ShiftBreakResponse>.NewConfig()
    .Map(dest => dest.IsOngoing, src => src.EndedAt == null)
    .AfterMapping((src, dest) =>
        dest.DurationMinutes = (int)((src.EndedAt ?? DateTime.UtcNow) - src.StartedAt).TotalMinutes);
TypeAdapterConfig<Shift, ShiftResponse>.NewConfig()
    .Map(dest => dest.DriverName, src => src.Driver.User.FirstName + " " + src.Driver.User.LastName)
    .Map(dest => dest.CompanyName, src => src.Driver.TaxiCompany.Name)
    .Map(dest => dest.IsOngoing, src => src.EndedAt == null)
    .AfterMapping((src, dest) =>
    {
        dest.IsOnBreak = src.Breaks.Any(b => b.EndedAt == null);
        dest.IsDispatchEligible = src.EndedAt == null && !dest.IsOnBreak;
        dest.DurationMinutes = (int)((src.EndedAt ?? DateTime.UtcNow) - src.StartedAt).TotalMinutes;
        dest.BreakMinutes = src.Breaks.Sum(b => (int)((b.EndedAt ?? DateTime.UtcNow) - b.StartedAt).TotalMinutes);
        dest.WorkedMinutes = Math.Max(dest.DurationMinutes - dest.BreakMinutes, 0);
    });
TypeAdapterConfig<CarAssignment, CarAssignmentResponse>.NewConfig()
    .Map(dest => dest.CarPlate, src => src.Car.Plate)
    .Map(dest => dest.CarMake, src => src.Car.Make)
    .Map(dest => dest.CarModel, src => src.Car.Model)
    .Map(dest => dest.CategoryName, src => src.Car.VehicleCategory.Name)
    .Map(dest => dest.CompanyName, src => src.Car.TaxiCompany.Name)
    .Map(dest => dest.DriverName, src => src.Driver.User.FirstName + " " + src.Driver.User.LastName)
    .Map(dest => dest.IsOngoing, src => src.AssignedTo == null);
TypeAdapterConfig<CityUpdateRequest, City>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<VehicleCategoryUpdateRequest, VehicleCategory>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<OrderStatusUpdateRequest, OrderStatus>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<SupportTicketStatusUpdateRequest, SupportTicketStatus>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<PaymentStatusUpdateRequest, PaymentStatus>.NewConfig().IgnoreNullValues(true);

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ITaxiCompanyService, TaxiCompanyService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<ICarAssignmentService, CarAssignmentService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderNotificationDispatcher, OrderNotificationDispatcher>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentNotificationDispatcher, PaymentNotificationDispatcher>();
builder.Services.AddSingleton<IReportDocumentBuilder, ReportDocumentBuilder>();
builder.Services.AddScoped<IReportService, ReportService>();
if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(EnvironmentReader.Optional("STRIPE_SECRET_KEY")))
{
    builder.Services.AddSingleton<IStripeGateway, DevelopmentStripeGateway>();
}
else
{
    builder.Services.AddSingleton<IStripeGateway, StripeGateway>();
}
builder.Services.AddSingleton<IOrderStateMachine, OrderStateMachine>();
builder.Services.AddSingleton<IFareCalculator, FareCalculator>();
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();
builder.Services.AddScoped<INotificationRecorder, NotificationRecorder>();
builder.Services.AddScoped<IInteractionRecorder, InteractionRecorder>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IImageFileStore, ImageFileStore>();

builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IVehicleCategoryService, VehicleCategoryService>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<ISupportTicketStatusService, SupportTicketStatusService>();
builder.Services.AddScoped<IPaymentStatusService, PaymentStatusService>();

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAccessTokenRevocationService, AccessTokenRevocationService>();

builder.Services.AddScoped<IAccessManager, AccessManager>();

builder.Services.AddScoped<ICryptoService, CryptoService>();

builder.Services.AddScoped<IValidator<UserInsertRequest>, UserInsertValidator>();
builder.Services.AddScoped<IValidator<UserUpdateRequest>, UserUpdateValidator>();
builder.Services.AddScoped<IValidator<UserProfileUpdateRequest>, UserProfileUpdateValidator>();
builder.Services.AddScoped<IValidator<UserPasswordChangeRequest>, UserPasswordChangeValidator>();
builder.Services.AddScoped<IValidator<DriverInsertRequest>, DriverInsertValidator>();
builder.Services.AddScoped<IValidator<DriverUpdateRequest>, DriverUpdateValidator>();
builder.Services.AddScoped<IValidator<DriverLocationUpdateRequest>, DriverLocationUpdateValidator>();
builder.Services.AddScoped<IValidator<SupportTicketInsertRequest>, SupportTicketInsertValidator>();
builder.Services.AddScoped<IValidator<SupportTicketReplyRequest>, SupportTicketReplyValidator>();
builder.Services.AddScoped<IValidator<ReviewInsertRequest>, ReviewInsertValidator>();
builder.Services.AddScoped<IValidator<ReviewUpdateRequest>, ReviewUpdateValidator>();
builder.Services.AddScoped<IValidator<OrderInsertRequest>, OrderInsertValidator>();
builder.Services.AddScoped<IValidator<OrderCancelRequest>, OrderCancelValidator>();
builder.Services.AddScoped<IValidator<CarAssignmentInsertRequest>, CarAssignmentInsertValidator>();
builder.Services.AddScoped<IValidator<CarAssignmentUpdateRequest>, CarAssignmentUpdateValidator>();
builder.Services.AddScoped<IValidator<CarInsertRequest>, CarInsertValidator>();
builder.Services.AddScoped<IValidator<CarUpdateRequest>, CarUpdateValidator>();
builder.Services.AddScoped<IValidator<TaxiCompanyInsertRequest>, TaxiCompanyInsertValidator>();
builder.Services.AddScoped<IValidator<TaxiCompanyUpdateRequest>, TaxiCompanyUpdateValidator>();
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

    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken) &&
                context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var (tokenId, expiresAt) = context.SecurityToken switch
            {
                JsonWebToken jsonWebToken => (jsonWebToken.Id, jsonWebToken.ValidTo),
                JwtSecurityToken jwtToken => (jwtToken.Id, jwtToken.ValidTo),
                _ => (null, default(DateTime))
            };

            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return;
            }

            var revocations = context.HttpContext.RequestServices
                .GetRequiredService<IAccessTokenRevocationService>();

            if (await revocations.IsRevokedAsync(tokenId, context.HttpContext.RequestAborted))
            {
                context.Fail("This token was signed out.");
                return;
            }

            context.HttpContext.Items[TokenClaims.TokenIdItem] = tokenId;
            context.HttpContext.Items[TokenClaims.TokenExpiresAtItem] = expiresAt;
        }
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
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
