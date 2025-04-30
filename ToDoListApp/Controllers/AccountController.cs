using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using ToDoListApp.Models;
using ToDoListApp.Services;
using ToDoListApp.ViewModels;

namespace ToDoListApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(MongoDBService mongoDBService, ILogger<AccountController> logger)
        {
            _mongoDBService = mongoDBService;
            _logger = logger;
        }

        // Kayıt Sayfasını Göster
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Kayıt İşlemini Gerçekleştir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // E-posta adresinin kullanılıp kullanılmadığını kontrol et
                var existingUser = await _mongoDBService.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                    return View(model);
                }

                // Kullanıcı adının kullanılıp kullanılmadığını kontrol et
                var existingUsername = await _mongoDBService.GetUserByUsernameAsync(model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanılıyor.");
                    return View(model);
                }

                // Rastgele profil resmi oluştur
                string profilePicture = GenerateRandomAvatar(model.FirstName, model.LastName);

                // Yeni kullanıcı oluştur
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    Password = HashPassword(model.Password),
                    ProfilePicture = profilePicture,
                    CreatedDate = DateTime.Now
                };

                await _mongoDBService.CreateUserAsync(user);

                // Kullanıcıyı oturum açma sayfasına yönlendir
                TempData["SuccessMessage"] = "Kaydınız başarıyla tamamlandı. Şimdi giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // Giriş Sayfasını Göster
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Giriş İşlemini Gerçekleştir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı adına göre kullanıcıyı bul
                var user = await _mongoDBService.GetUserByUsernameAsync(model.Username);

                // Kullanıcı bulunamadı veya şifre yanlış
                if (user == null || !VerifyPassword(model.Password, user.Password))
                {
                    ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                    return View(model);
                }

                // Oturum bilgilerini kaydet
                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);

                // Kullanıcıyı Todo sayfasına yönlendir
                return RedirectToAction("Index", "Todo");
            }

            return View(model);
        }

        // Çıkış Yap
        public IActionResult Logout()
        {
            // Oturum bilgilerini temizle
            HttpContext.Session.Clear();

            // Ana sayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }

        // Şifre hashleme fonksiyonu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Şifre doğrulama fonksiyonu
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        // Rastgele avatar oluştur
        private string GenerateRandomAvatar(string firstName, string lastName)
        {
            // Rastgele seçilen avatarın renk kodu ve stilleri
            string[] colors = {
                "ff6b6b", "48dbfb", "1dd1a1", "5f27cd", "54a0ff",
                "00d2d3", "feca57", "ff9ff3", "2e86de", "ee5253"
            };
            
            // Kullanıcının baş harflerini al
            string initials = "";
            if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
                initials += firstName[0];
            if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
                initials += lastName[0];
            
            initials = initials.ToUpper();
            
            // Rastgele bir renk seç
            Random random = new Random();
            string backgroundColor = colors[random.Next(colors.Length)];
            
            // UI-Avatars API kullanarak avatar URL'i oluştur
            return $"https://ui-avatars.com/api/?name={initials}&background={backgroundColor}&color=fff&bold=true&size=256";
        }
    }
} 