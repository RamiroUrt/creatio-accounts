using CreatioAccounts.Api.Configuration;
using CreatioAccounts.Api.Infrastructure;
using CreatioAccounts.Api.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

var envValues = EnvFileLoader.LoadFrom();
if (envValues.Count > 0)
{
    builder.Configuration.AddInMemoryCollection(envValues);
}

builder.Services.AddCreatio(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}

app.UseErrorHandling();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();