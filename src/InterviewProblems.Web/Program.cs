using InterviewProblems.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
		context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
			context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
			_ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
			{
				PermitLimit = 60,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0,
			}));
});

builder.Services.AddSingleton<RepoLocator>();
builder.Services.AddSingleton<ReferenceProvider>();
builder.Services.AddSingleton<ProblemCatalog>();
builder.Services.AddSingleton<SqlWorkbench>();
builder.Services.AddSingleton<VerbalQuestionProvider>();
builder.Services.AddSingleton<RoslynWorkspaceService>();
builder.Services.AddScoped<CandidateRunner>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
	context.Response.Headers["X-Content-Type-Options"] = "nosniff";
	context.Response.Headers["X-Frame-Options"] = "DENY";
	context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
	context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
	context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
	context.Response.Headers["Content-Security-Policy"] =
		"default-src 'self'; " +
		"base-uri 'self'; " +
		"form-action 'self'; " +
		"frame-ancestors 'none'; " +
		"object-src 'none'; " +
		"img-src 'self' data:; " +
		"style-src 'self' 'unsafe-inline'; " +
		"script-src 'self' https://cdn.jsdelivr.net 'unsafe-eval'; " +
		"connect-src 'self' ws: wss:; " +
		"worker-src 'self' blob:;";
	await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
