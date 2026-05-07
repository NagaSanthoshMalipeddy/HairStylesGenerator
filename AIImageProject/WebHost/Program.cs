var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Serve the React build as static files; SPA fallback to index.html for client-side routes.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHealthChecks("/health");

app.MapFallbackToFile("index.html");

app.Run();
