using Amazon.Lambda.AspNetCoreServer.Hosting;
using Application.Configuration;
using Infrastructure.Messaging.Configuration;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using WebApi.Authentication;
using WebApi.Common;
using WebApi.Mappers;
using WebApi.Scalar;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("AppSettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"AppSettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddDatabase(configuration);

services.AddProblemDetails();
services.AddValidation();

services.ConfigureAuthentication(configuration);
services.AddAuthorization();

services.AddOpenApi();
services.AddS3Service(configuration);
services.AddStripeService(configuration);
services.AddMassTransitClient(configuration);
services.AddApplicationMediator(configuration);

services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, CurrentUser>();
services.AddScoped<IGameApplicationMapper, GameApplicationMapper>();
services.AddScoped<IGameApplicationBuildMapper, GameBuildApplicationMapper>();
services.AddScoped<IGenreApplicationMapper, GenreApplicationMapper>();
services.AddScoped<IUserApplicationMapper, UserApplicationMapper>();
services.AddScoped<IAdminGameApplicationMapper, AdminGameApplicationMapper>();
services.AddScoped<IAdminGenreApplicationMapper, AdminGenreApplicationMapper>();
services.AddScoped<IAdminUserApplicationMapper, AdminUserApplicationMapper>();
services.AddScoped<IAchievementApplicationMapper, AchievementApplicationMapper>();

services.AddControllers();
services.ConfigureScalar();

services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    } );
} );

services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
services.AddProblemDetails();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok());
app.MapControllers();

app.Run();