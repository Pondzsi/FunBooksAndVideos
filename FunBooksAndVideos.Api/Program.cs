using System.Text.Json.Serialization;
using FunBooksAndVideos.Api.Errors;
using FunBooksAndVideos.Application;
using FunBooksAndVideos.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Enums travel as names ("Book"), in the API and in the OpenAPI document. A number where a name belongs is rejected.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

// Every error, including validation failures and unhandled exceptions, is an RFC 9457 Problem Details response.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

try
{
    await app.Services.InitializeDatabaseAsync();
}
catch (DatabaseInitializationException exception)
{
    // One readable line instead of a stack trace: the usual cause is that the database container is not running.
    app.Logger.LogCritical("{Message}", exception.Message);
    Environment.ExitCode = 1;

    return;
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets the integration tests start the API in memory.
public partial class Program
{
}
