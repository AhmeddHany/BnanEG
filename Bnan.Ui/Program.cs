using Bnan.Inferastructure;
using Bnan.Inferastructure.Quartz;
using Microsoft.Extensions.FileProviders;
using NToastNotify;
using Quartz;

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
builder.Services.AddQuartz(q => QuartzConfiguration.ConfigureQuartz(q));
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
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
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