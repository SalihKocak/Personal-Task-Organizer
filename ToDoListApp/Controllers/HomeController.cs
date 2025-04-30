using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToDoListApp.Models;
using ToDoListApp.Services;

namespace ToDoListApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MongoDBService _mongoDBService;

    public HomeController(ILogger<HomeController> logger, MongoDBService mongoDBService)
    {
        _logger = logger;
        _mongoDBService = mongoDBService;
    }

    public IActionResult Index()
    {
        // Ana sayfa, uygulama tanıtımı ve kayıt/giriş seçenekleri
        return View();
    }

    public IActionResult About()
    {
        // Uygulama hakkında bilgi sayfası
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
