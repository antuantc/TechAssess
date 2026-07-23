using Microsoft.AspNetCore.HttpOverrides;
using InterviewProblems.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var configuredPathBase = builder.Configuration["PathBase"] ?? string.Empty;
var pathBase = string.Empty;
if (!builder.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(configuredPathBase))
{
    pathBase = "/" + configuredPathBase.Trim('/');
}

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddSingleton<RepoLocator>();
builder.Services.AddSingleton<ReferenceProvider>();
builder.Services.AddSingleton<ProblemCatalog>();
builder.Services.AddSingleton<SqlWorkbench>();
builder.Services.AddSingleton<VerbalQuestionProvider>();
builder.Services.AddSingleton<RoslynWorkspaceService>();
builder.Services.AddScoped<CandidateRunner>();

var app = builder.Build();

app.UseForwardedHeaders();
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
    app.Use((context, next) =>
    {
        // If the proxy ever forwards the prefix, strip it; then always set
        // PathBase so URL generation (~/, static files, Blazor hub) includes it.
        if (context.Request.Path.StartsWithSegments(pathBase, out var remainder))
        {
            context.Request.Path = remainder;
        }

        context.Request.PathBase = pathBase;
        return next();
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://js.monitor.azure.com; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "connect-src 'self' ws: wss: https://js.monitor.azure.com https://*.in.applicationinsights.azure.com; " +
        "font-src 'self' data:; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
