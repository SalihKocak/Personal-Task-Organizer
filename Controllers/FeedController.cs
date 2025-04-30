using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToDoListApp.Services;
using ToDoListApp.Models;
using ToDoListApp.ViewModels;
using MongoDB.Bson;

namespace ToDoListApp.Controllers
{
    public class FeedController : Controller
    {
        private readonly MongoDBService _mongoDBService;

        public FeedController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
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

        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var posts = await _mongoDBService.GetAllPostsAsync();
            return View(posts);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var post = await _mongoDBService.GetPostByIdAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            var viewModel = new PostDetailViewModel
            {
                Post = post,
                Comments = post.Comments.OrderByDescending(c => c.CreatedDate).ToList(),
                NewComment = new Comment { PostId = id }
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                post.Id = ObjectId.GenerateNewId().ToString();
                post.UserId = GetUserId();
                post.CreatedDate = DateTime.Now;
                post.Comments = new List<Comment>();

                await _mongoDBService.CreatePostAsync(post);
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var post = await _mongoDBService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId != GetUserId())
            {
                return Forbid();
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Post post)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != post.Id)
            {
                return NotFound();
            }

            var existingPost = await _mongoDBService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound();
            }

            if (existingPost.UserId != GetUserId())
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                post.UserId = existingPost.UserId;
                post.CreatedDate = existingPost.CreatedDate;
                post.Comments = existingPost.Comments;
                
                await _mongoDBService.UpdatePostAsync(id, post);
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var post = await _mongoDBService.GetPostByIdAsync(id);
                
            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId != GetUserId())
            {
                return Forbid();
            }

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var post = await _mongoDBService.GetPostByIdAsync(id);
            
            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId != GetUserId())
            {
                return Forbid();
            }

            await _mongoDBService.DeletePostAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                comment.Id = ObjectId.GenerateNewId().ToString();
                comment.UserId = GetUserId();
                comment.CreatedDate = DateTime.Now;

                await _mongoDBService.AddCommentToPostAsync(comment.PostId, comment);
                return RedirectToAction(nameof(Details), new { id = comment.PostId });
            }
            
            return RedirectToAction(nameof(Details), new { id = comment.PostId });
        }

        public async Task<IActionResult> MyPosts()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetUserId();
            var posts = await _mongoDBService.GetPostsByUserIdAsync(userId);

            return View(posts);
        }
    }
} 