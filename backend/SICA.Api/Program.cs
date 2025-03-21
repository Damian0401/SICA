using FluentValidation;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Scalar.AspNetCore;
using SICA.Api;
using SICA.Api.Features.Files;
using SICA.Tools;

var builder = WebApplication.CreateBuilder(args);

var corsPolicy = new CorsPolicyBuilder()
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()
    .Build();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsPolicy);
});

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

builder.Services.AddOptions<ApiSettings>().BindConfiguration(ApiSettings.SectionName);

builder.AddTools();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapFilesModule();

app.UseCors();

app.Run();
