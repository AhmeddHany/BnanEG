using Bnan.Core;
using Microsoft.Extensions.FileProviders;
using Bnan.Inferastructure;
using NToastNotify;
using System.Globalization;
using Bnan.Inferastructure.MiddleWare;
using Bnan.Core.Interfaces;
using Bnan.Inferastructure.Repository;
using Quartz;
using Bnan.Inferastructure.Quartz;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews(opt =>
{
    opt.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());
}).AddMvcLocalization();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
builder.Services.AddRazorPages().AddNToastNotifyToastr(new ToastrOptions());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.AddPersistenceServices();
builder.Services.AddSignalR();

// Add Quartz services
builder.Services.AddQuartz(q =>QuartzConfiguration.ConfigureQuartz(q));
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Custom login path
    options.AccessDeniedPath = "/Identity/Account/Error/403"; // Custom access denied path
});
var app = builder.Build();
app.UseNToastNotify();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Identity/Account/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

var supportedCultures = new[] { "en-US", "ar-EG" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
app.UseStatusCodePagesWithReExecute("/Identity/Account/Error/{0}");
app.UseStatusCodePagesWithRedirects("/Identity/Account/Login");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<LogoutMiddleware>(); // Add the custom middleware here
app.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Users}/{action=Login}/{id?}"
    );
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Identity}/{controller=Account}/{action=Login}/{id?}"
 );


app.Run();