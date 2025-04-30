using Microsoft.AspNetCore.Mvc;
using ToDoListApp.Models;
using ToDoListApp.Services;
using MongoDB.Bson;
using ToDoListApp.ViewModels;

namespace ToDoListApp.Controllers
{
    public class TodoController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly ILogger<TodoController> _logger;

        public TodoController(MongoDBService mongoDBService, ILogger<TodoController> logger)
        {
            _mongoDBService = mongoDBService;
            _logger = logger;
        }

        // Oturum kontrolü
        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("UserId") != null;
        }

        // Kullanıcı ID'sini al
        private string GetUserId()
        {
            return HttpContext.Session.GetString("UserId") ?? string.Empty;
        }

        // Todo Listesi Ana Sayfası
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetUserId();
            var todoItems = await _mongoDBService.GetAllTodoItemsAsync(userId);

            var viewModel = new TodoListViewModel
            {
                TodoItems = todoItems,
                PendingItems = todoItems.Where(t => t.Status == TodoStatus.Pending).ToList(),
                InProgressItems = todoItems.Where(t => t.Status == TodoStatus.InProgress).ToList(),
                CompletedItems = todoItems.Where(t => t.Status == TodoStatus.Completed).ToList()
            };

            return View(viewModel);
        }

        // Yeni Todo Ekleme Sayfası
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Yeni Todo Ekleme İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoItem todoItem)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                todoItem.UserId = GetUserId();
                todoItem.CreatedDate = DateTime.Now;
                
                await _mongoDBService.CreateTodoItemAsync(todoItem);
                return RedirectToAction(nameof(Index));
            }

            return View(todoItem);
        }

        // Todo Düzenleme Sayfası
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("Todo Edit: Kullanıcı oturum açmamış");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Todo Edit için ID: {Id}", id);
            
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                _logger.LogWarning("Todo Edit: Geçersiz ID formatı: {Id}", id);
                return BadRequest("Geçersiz ID formatı.");
            }

            try
            {
                var todoItem = await _mongoDBService.GetTodoItemByIdAsync(id);

                if (todoItem == null)
                {
                    _logger.LogWarning("Todo Edit: TodoItem bulunamadı, ID: {Id}", id);
                    return NotFound();
                }
                
                if (todoItem.UserId != GetUserId())
                {
                    _logger.LogWarning("Todo Edit: Yetkisiz erişim. TodoItemUserId: {TodoItemUserId}, CurrentUserId: {CurrentUserId}", todoItem.UserId, GetUserId());
                    return NotFound();
                }

                _logger.LogInformation("Todo Edit: TodoItem bulundu ve gösteriliyor, ID: {Id}", id);
                return View("Edit", todoItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Todo Edit: Beklenmeyen hata, ID: {Id}", id);
                return StatusCode(500, "Beklenmeyen bir hata oluştu.");
            }
        }

        // Todo Düzenleme İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Title,Description,Status,Priority,DueDate")] TodoItem todoItem)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("Todo Edit POST: Kullanıcı oturum açmamış");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Todo Edit POST için ID: {Id}", id);

            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                _logger.LogWarning("Todo Edit POST: Geçersiz ID formatı: {Id}", id);
                return BadRequest("Geçersiz ID formatı.");
            }

            if (id != todoItem.Id)
            {
                _logger.LogWarning("Todo Edit POST: ID uyuşmazlığı. PathId: {PathId}, FormId: {FormId}", id, todoItem.Id);
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var existingItem = await _mongoDBService.GetTodoItemByIdAsync(id);
                        
                        if (existingItem == null)
                        {
                            _logger.LogWarning("Todo Edit POST: TodoItem bulunamadı, ID: {Id}", id);
                            return NotFound();
                        }

                        if (existingItem.UserId != GetUserId())
                        {
                            _logger.LogWarning("Todo Edit POST: Yetkisiz erişim. TodoItemUserId: {TodoItemUserId}, CurrentUserId: {CurrentUserId}", existingItem.UserId, GetUserId());
                            return NotFound();
                        }

                        // Mevcut oluşturma tarihini koru
                        todoItem.CreatedDate = existingItem.CreatedDate;
                        todoItem.UserId = existingItem.UserId;

                        // Eğer durum "Tamamlandı" olarak değiştiyse, tamamlanma tarihini ayarla
                        if (todoItem.Status == TodoStatus.Completed && existingItem.Status != TodoStatus.Completed)
                        {
                            todoItem.CompletedDate = DateTime.Now;
                        }
                        else if (todoItem.Status != TodoStatus.Completed)
                        {
                            todoItem.CompletedDate = null;
                        }
                        else if (todoItem.Status == TodoStatus.Completed && existingItem.Status == TodoStatus.Completed)
                        {
                            // Zaten tamamlanmış durumda ise tamamlanma tarihini koru
                            todoItem.CompletedDate = existingItem.CompletedDate;
                        }

                        await _mongoDBService.UpdateTodoItemAsync(id, todoItem);
                        _logger.LogInformation("Todo Edit POST: TodoItem başarıyla güncellendi, ID: {Id}", id);
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Todo Edit POST: TodoItem düzenlenirken hata oluştu: {Message}, ID: {Id}", ex.Message, id);
                        ModelState.AddModelError("", "Güncelleme işlemi sırasında bir hata oluştu.");
                    }
                }
                else
                {
                    _logger.LogWarning("Todo Edit POST: Model doğrulama hatası, ID: {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Todo Edit POST: Beklenmeyen hata, ID: {Id}", id);
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu.");
            }

            return View("Edit", todoItem);
        }

        // Todo Silme Sayfası
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("Todo Delete: Kullanıcı oturum açmamış");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Todo Delete için ID: {Id}", id);
            
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                _logger.LogWarning("Todo Delete: Geçersiz ID formatı: {Id}", id);
                return BadRequest("Geçersiz ID formatı.");
            }

            try
            {
                var todoItem = await _mongoDBService.GetTodoItemByIdAsync(id);

                if (todoItem == null)
                {
                    _logger.LogWarning("Todo Delete: TodoItem bulunamadı, ID: {Id}", id);
                    return NotFound();
                }
                
                if (todoItem.UserId != GetUserId())
                {
                    _logger.LogWarning("Todo Delete: Yetkisiz erişim. TodoItemUserId: {TodoItemUserId}, CurrentUserId: {CurrentUserId}", todoItem.UserId, GetUserId());
                    return NotFound();
                }

                _logger.LogInformation("Todo Delete: TodoItem bulundu ve gösteriliyor, ID: {Id}", id);
                return View("Delete", todoItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Todo Delete: Beklenmeyen hata, ID: {Id}", id);
                return StatusCode(500, "Beklenmeyen bir hata oluştu.");
            }
        }

        // Todo Silme İşlemi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                return BadRequest("Geçersiz ID formatı.");
            }

            var todoItem = await _mongoDBService.GetTodoItemByIdAsync(id);

            if (todoItem == null)
            {
                return NotFound("Görev bulunamadı.");
            }

            if (todoItem.UserId != GetUserId())
            {
                return Forbid("Bu görevi silme yetkiniz yok.");
            }

            try
            {
                await _mongoDBService.DeleteTodoItemAsync(id);
                TempData["SuccessMessage"] = "Görev başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Todo silinirken hata oluştu: {Message}", ex.Message);
                ModelState.AddModelError("", "Silme işlemi sırasında bir hata oluştu.");
                return View("Delete", todoItem);
            }
        }

        // Durum değiştirme (AJAX işlemleri için)
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(string id, TodoStatus status)
        {
            if (!IsAuthenticated())
            {
                return Json(new { success = false, message = "Oturum açmanız gerekiyor." });
            }

            var todoItem = await _mongoDBService.GetTodoItemByIdAsync(id);

            if (todoItem == null || todoItem.UserId != GetUserId())
            {
                return Json(new { success = false, message = "Görev bulunamadı." });
            }

            todoItem.Status = status;

            // Eğer durum "Tamamlandı" olarak değiştiyse, tamamlanma tarihini ayarla
            if (status == TodoStatus.Completed)
            {
                todoItem.CompletedDate = DateTime.Now;
            }
            else
            {
                todoItem.CompletedDate = null;
            }

            await _mongoDBService.UpdateTodoItemAsync(id, todoItem);
            return Json(new { success = true });
        }

        // Dashboard sayfası
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetUserId();
            var todoItems = await _mongoDBService.GetAllTodoItemsAsync(userId);

            var dashboardViewModel = new DashboardViewModel
            {
                TotalTasks = todoItems.Count,
                CompletedTasks = todoItems.Count(t => t.Status == TodoStatus.Completed),
                PendingTasks = todoItems.Count(t => t.Status == TodoStatus.Pending),
                InProgressTasks = todoItems.Count(t => t.Status == TodoStatus.InProgress),
                CanceledTasks = todoItems.Count(t => t.Status == TodoStatus.Canceled),
                
                TasksByPriority = new Dictionary<Priority, int>
                {
                    { Priority.Low, todoItems.Count(t => t.Priority == Priority.Low) },
                    { Priority.Medium, todoItems.Count(t => t.Priority == Priority.Medium) },
                    { Priority.High, todoItems.Count(t => t.Priority == Priority.High) },
                    { Priority.Critical, todoItems.Count(t => t.Priority == Priority.Critical) }
                },
                
                RecentTasks = todoItems.OrderByDescending(t => t.CreatedDate).Take(5).ToList(),
                CompletionRate = todoItems.Any() ? (double)todoItems.Count(t => t.Status == TodoStatus.Completed) / todoItems.Count * 100 : 0
            };

            return View(dashboardViewModel);
        }
    }
} 