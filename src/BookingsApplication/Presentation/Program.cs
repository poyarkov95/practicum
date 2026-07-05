using System.Reflection;
using System.Security.Claims;
using System.Text;
using Application;
using Application.Abstractions.Services.Interface;
using Application.Common.DTOs;
using Common.Settings;
using Confluent.Kafka;
using Presentation.Extensions;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    // Добавляем кнопку для ввода токена в Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите токен в формате: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers(
        options =>  options.SuppressAsyncSuffixInActionNames = false)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
        options.InvalidModelStateResponseFactory = context =>
        { 
            var errors = context.ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
                .ToList();
            
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError($"Validation errors: {string.Join(", ", errors)}");
            
            return new BadRequestObjectResult(new ErrorResponse
            {
                Message = "Ошибка валидации",
                Errors = errors
            });
        };
    });


builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddSingleton<IProducer<Null, string>>(_ =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["KafkaConfiguration:BootstrapServers"],
        Acks = Acks.All, 
        EnableIdempotence = true,
        MaxInFlight = 1 
    };
    return new ProducerBuilder<Null, string>(config).Build();
});

builder.Services.AddSingleton<IBookingProducer, BookingProducer>();

builder.Services.AddSingleton<IConsumer<Ignore, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["KafkaConfiguration:BootstrapServers"],
        GroupId = builder.Configuration["KafkaConfiguration:GroupId"],
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
    };
    return new ConsumerBuilder<Ignore, string>(config).Build();
});

builder.Services.AddSingleton<IBookingConsumer, BookingConsumer>();

var tokenConfiguration = builder.Configuration.GetSection("TokenMetadata");
builder.Services.Configure<TokenMetadata>(tokenConfiguration);

var kafkaConfiguration = builder.Configuration.GetSection("KafkaConfiguration");
builder.Services.Configure<KafkaConfiguration>(kafkaConfiguration);

var kafkaConsumerConfiguration = builder.Configuration.GetSection("KafkaConfiguration");
builder.Services.Configure<KafkaConsumerConfiguration>(kafkaConsumerConfiguration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenMetadata = tokenConfiguration.Get<TokenMetadata>();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenMetadata.Secret));
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = key,
            ValidIssuer = tokenMetadata.Issuer,
            ValidAudience = tokenMetadata.Audience,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };
    }); 

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
} 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseHttpsRedirection();
app.AddExceptionMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();