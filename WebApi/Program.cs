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
var env = builder.Environment;
builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("AppSettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"AppSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);
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
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok());
app.MapControllers();

app.Run();