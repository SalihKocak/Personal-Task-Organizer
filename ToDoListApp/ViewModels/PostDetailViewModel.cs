using ToDoListApp.Models;
using System.Collections.Generic;

namespace ToDoListApp.ViewModels
{
    public class PostDetailViewModel
    {
        public Post Post { get; set; } = null!;
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public Comment NewComment { get; set; } = null!;
    }
} 