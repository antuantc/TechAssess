using InterviewProblems.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<RepoLocator>();
builder.Services.AddSingleton<ReferenceProvider>();
builder.Services.AddSingleton<ProblemCatalog>();
builder.Services.AddSingleton<SqlWorkbench>();
builder.Services.AddSingleton<VerbalQuestionProvider>();
builder.Services.AddSingleton<RoslynWorkspaceService>();
builder.Services.AddScoped<CandidateRunner>();

var app = builder.Build();

app.UsePathBase("/TechAssess");
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
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "connect-src 'self' ws: wss: https://cdn.jsdelivr.net; " +
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
