var builder = WebApplication.CreateBuilder(args);

// Application Insights — picks up APPLICATIONINSIGHTS_CONNECTION_STRING from env / config automatically.
// Combined with the App Service auto-instrumentation (ApplicationInsightsAgent_EXTENSION_VERSION) you
// already have set, this enables full request, dependency, and exception tracking.
builder.Services.AddApplicationInsightsTelemetry();

// Health checks — exposed at /health for App Service "Health check" probe.
builder.Services.AddHealthChecks();

// Standardized error responses (RFC 7807) for unhandled exceptions and non-success status codes.
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Named HttpClient for Azure OpenAI image edits — image generation can take 1-3 minutes,
// so override the default 100s HttpClient.Timeout. We stay under App Service's ~230s
// front-end idle timeout to avoid 502s from the load balancer.
builder.Services.AddHttpClient("AzureOpenAI", client =>
{
    client.Timeout = TimeSpan.FromMinutes(4);
});

// CORS — restrict to specific origins in production via the "Cors:AllowedOrigins" config / env var.
// Falls back to AllowAnyOrigin only when nothing is configured (dev convenience).
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}
else
{
    // Production: return ProblemDetails for unhandled exceptions and map error status codes
    // (404, 500, etc.) to a consistent JSON body instead of the default IIS / Kestrel HTML page.
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("AppCors");

// Health probe endpoint — point App Service > Monitoring > Health check at "/health".
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
