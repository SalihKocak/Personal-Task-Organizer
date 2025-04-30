using ToDoListApp.Models;

namespace ToDoListApp.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CanceledTasks { get; set; }
        
        public Dictionary<Priority, int> TasksByPriority { get; set; } = new Dictionary<Priority, int>();
        
        public List<TodoItem> RecentTasks { get; set; } = new List<TodoItem>();
        
        public double CompletionRate { get; set; }
    }
} 