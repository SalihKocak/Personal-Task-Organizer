using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ToDoListApp.Models
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Başlık 5-100 karakter arasında olmalıdır.")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "İçerik alanı zorunludur.")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "İçerik 10-5000 karakter arasında olmalıdır.")]
        public string Content { get; set; } = string.Empty;
        
        public string Tags { get; set; } = string.Empty;
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public List<Comment> Comments { get; set; } = new List<Comment>();
        
        // Navigation property - not stored in MongoDB
        [BsonIgnore]
        public User? User { get; set; }
    }
} 