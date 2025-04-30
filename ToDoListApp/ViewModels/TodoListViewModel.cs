using ToDoListApp.Models;

namespace ToDoListApp.ViewModels
{
    public class TodoListViewModel
    {
        public List<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
        public List<TodoItem> PendingItems { get; set; } = new List<TodoItem>();
        public List<TodoItem> InProgressItems { get; set; } = new List<TodoItem>();
        public List<TodoItem> CompletedItems { get; set; } = new List<TodoItem>();
    }
} 