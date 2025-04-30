using ToDoListApp;
using ToDoListApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // ViewBag'e MongoDBService ekleyerek hızlı erişim sağlanıyor
    options.Filters.Add<ViewBagSetupFilter>();
});

// Add MongoDB Service
builder.Services.AddSingleton<MongoDBService>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Enable session
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add a demo data seeding endpoint
app.MapGet("/seed-demo-data", async (IServiceProvider serviceProvider) =>
{
    try 
    {
        var mongoDBService = serviceProvider.GetRequiredService<MongoDBService>();
        var seedData = new SeedData(mongoDBService);
        await seedData.SeedAsync();
        return Results.Ok("Demo data has been seeded successfully!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error seeding demo data: {ex.Message}");
    }
});

app.Run();

// ViewBag'e MongoDBService ekleyen filter
public class ViewBagSetupFilter : IActionFilter
{
    private readonly MongoDBService _mongoDBService;

    public ViewBagSetupFilter(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is Controller controller)
        {
            controller.ViewBag.MongoDBService = _mongoDBService;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
