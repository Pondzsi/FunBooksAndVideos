using System.Text.Json.Serialization;
using FunBooksAndVideos.Api.Errors;
using FunBooksAndVideos.Application;
using FunBooksAndVideos.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Enums travel as names ("Book"), in the API and in the OpenAPI document.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Every error, including validation failures and unhandled exceptions, is an RFC 9457 Problem Details response.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

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
