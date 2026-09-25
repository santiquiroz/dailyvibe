using DailyVibe.Api.Authentication;
using DailyVibe.Api.Cors;
using DailyVibe.Api.ErrorHandling;
using DailyVibe.Api.OpenApi;
using DailyVibe.Api.Persistence;
using DailyVibe.Application;
using DailyVibe.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddSwaggerWithBearer();
builder.Services.AddDevClientCors();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtBearerAuthentication();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(DevClientCors.PolicyName);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MigrateDatabaseIfEnabled();

app.Run();
