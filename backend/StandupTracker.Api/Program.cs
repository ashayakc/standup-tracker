using StandupTracker.Api.Endpoints;
using StandupTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<StandupStore>();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapStandupEndpoints();

app.Run();

public partial class Program { }
