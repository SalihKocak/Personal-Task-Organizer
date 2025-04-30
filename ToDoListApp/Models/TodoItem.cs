using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ToDoListApp.Models
{
    public enum TodoStatus
    {
        [Display(Name = "Beklemede")]
        Pending = 0,
        
        [Display(Name = "Devam Ediyor")]
        InProgress = 1,
        
        [Display(Name = "Tamamlandı")]
        Completed = 2,
        
        [Display(Name = "İptal Edildi")]
        Canceled = 3
    }

    public enum Priority
    {
        [Display(Name = "Düşük")]
        Low = 0,
        
        [Display(Name = "Orta")]
        Medium = 1,
        
        [Display(Name = "Yüksek")]
        High = 2,
        
        [Display(Name = "Acil")]
        Critical = 3
    }

    public class TodoItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Başlık 3-100 karakter arasında olmalıdır.")]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Durum")]
        public TodoStatus Status { get; set; } = TodoStatus.Pending;
        
        [Display(Name = "Öncelik")]
        public Priority Priority { get; set; } = Priority.Medium;
        
        [Display(Name = "Son Tarih")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DueDate { get; set; }
        
        [Display(Name = "Oluşturulma Tarihi")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Tamamlanma Tarihi")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CompletedDate { get; set; }
        
        // Navigation property - not stored in MongoDB
        [BsonIgnore]
        public User? User { get; set; }
    }
} 