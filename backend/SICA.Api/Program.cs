using FluentValidation;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using SICA.Api;
using SICA.Api.Features.Files;
using SICA.Tools;
using SICA.Tools.BlobStore;

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
builder.Services.AddSingleton(TimeProvider.System);

builder.AddTools();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapFilesModule();

app.UseCors();

app.Run();
