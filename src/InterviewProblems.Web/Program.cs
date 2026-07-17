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

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
