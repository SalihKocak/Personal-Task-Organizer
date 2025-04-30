using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ToDoListApp.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; } = string.Empty;
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Yorum içeriği zorunludur.")]
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "Yorum 2-1000 karakter arasında olmalıdır.")]
        public string Content { get; set; } = string.Empty;
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation properties - not stored in MongoDB
        [BsonIgnore]
        public Post? Post { get; set; }
        
        [BsonIgnore]
        public User? User { get; set; }
    }
} 