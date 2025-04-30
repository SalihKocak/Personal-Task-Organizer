using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using ToDoListApp.Models;

namespace ToDoListApp.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<TodoItem> _todoCollection;
        private readonly IMongoCollection<Post> _postCollection;
        
        public string ConnectionString { get; private set; }
        public string DatabaseName { get; private set; }
        
        public MongoDBService(IConfiguration configuration)
        {
            ConnectionString = configuration["MongoDB:ConnectionString"];
            DatabaseName = configuration["MongoDB:DatabaseName"];
            
            MongoClient client = new MongoClient(ConnectionString);
            IMongoDatabase database = client.GetDatabase(DatabaseName);
            _userCollection = database.GetCollection<User>("Users");
            _todoCollection = database.GetCollection<TodoItem>("TodoItems");
            _postCollection = database.GetCollection<Post>("Posts");
        }

        // User methods
        public async Task<List<User>> GetAllUsersAsync() => 
            await _userCollection.Find(new BsonDocument()).ToListAsync();
        
        public async Task<User> GetUserByIdAsync(string id) => 
            await _userCollection.Find(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id))).FirstOrDefaultAsync();
        
        public async Task<User> GetUserByUsernameAsync(string username) => 
            await _userCollection.Find(Builders<User>.Filter.Eq("Username", username)).FirstOrDefaultAsync();
        
        public async Task<User> GetUserByEmailAsync(string email) => 
            await _userCollection.Find(Builders<User>.Filter.Eq("Email", email)).FirstOrDefaultAsync();
        
        public async Task CreateUserAsync(User user) => 
            await _userCollection.InsertOneAsync(user);
        
        public async Task UpdateUserAsync(string id, User user) => 
            await _userCollection.ReplaceOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)), user);
        
        public async Task DeleteUserAsync(string id) => 
            await _userCollection.DeleteOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)));

        // TodoItem methods
        public async Task<List<TodoItem>> GetAllTodoItemsAsync(string userId) => 
            await _todoCollection.Find(Builders<TodoItem>.Filter.Eq("UserId", userId)).ToListAsync();
        
        public async Task<TodoItem> GetTodoItemByIdAsync(string id)
        {
            try
            {
                return await _todoCollection.Find(Builders<TodoItem>.Filter.Eq("_id", ObjectId.Parse(id))).FirstOrDefaultAsync();
            }
            catch (FormatException)
            {
                // ID ObjectId formatına dönüştürülemedi
                return null;
            }
        }
        
        public async Task<List<TodoItem>> GetUserTodoItemsByStatusAsync(string userId, TodoStatus status) => 
            await _todoCollection.Find(Builders<TodoItem>.Filter.And(
                Builders<TodoItem>.Filter.Eq("UserId", userId),
                Builders<TodoItem>.Filter.Eq("Status", status)
            )).ToListAsync();
        
        public async Task CreateTodoItemAsync(TodoItem todoItem) => 
            await _todoCollection.InsertOneAsync(todoItem);
        
        public async Task UpdateTodoItemAsync(string id, TodoItem todoItem)
        {
            try
            {
                await _todoCollection.ReplaceOneAsync(Builders<TodoItem>.Filter.Eq("_id", ObjectId.Parse(id)), todoItem);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Geçersiz Todo ID formatı", nameof(id));
            }
        }
        
        public async Task DeleteTodoItemAsync(string id)
        {
            try
            {
                await _todoCollection.DeleteOneAsync(Builders<TodoItem>.Filter.Eq("_id", ObjectId.Parse(id)));
            }
            catch (FormatException)
            {
                throw new ArgumentException("Geçersiz Todo ID formatı", nameof(id));
            }
        }
        
        // Post methods
        public async Task<List<Post>> GetAllPostsAsync() => 
            await _postCollection.Find(new BsonDocument()).ToListAsync();
        
        public async Task<List<Post>> GetPostsByUserIdAsync(string userId) => 
            await _postCollection.Find(Builders<Post>.Filter.Eq("UserId", userId)).ToListAsync();
        
        public async Task<Post> GetPostByIdAsync(string id)
        {
            try
            {
                return await _postCollection.Find(Builders<Post>.Filter.Eq("_id", ObjectId.Parse(id))).FirstOrDefaultAsync();
            }
            catch (FormatException)
            {
                // ID ObjectId formatına dönüştürülemedi
                return null;
            }
        }
        
        public async Task CreatePostAsync(Post post) => 
            await _postCollection.InsertOneAsync(post);
        
        public async Task UpdatePostAsync(string id, Post post)
        {
            try
            {
                await _postCollection.ReplaceOneAsync(Builders<Post>.Filter.Eq("_id", ObjectId.Parse(id)), post);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Geçersiz Post ID formatı", nameof(id));
            }
        }
        
        public async Task DeletePostAsync(string id)
        {
            try
            {
                await _postCollection.DeleteOneAsync(Builders<Post>.Filter.Eq("_id", ObjectId.Parse(id)));
            }
            catch (FormatException)
            {
                throw new ArgumentException("Geçersiz Post ID formatı", nameof(id));
            }
        }
        
        // Comment methods (embedded in Posts)
        public async Task AddCommentToPostAsync(string postId, Comment comment)
        {
            Post post = await GetPostByIdAsync(postId);
            comment.Id = ObjectId.GenerateNewId().ToString();
            comment.CreatedDate = DateTime.Now;
            post.Comments.Add(comment);
            await UpdatePostAsync(postId, post);
        }
        
        public async Task UpdateCommentInPostAsync(string postId, Comment comment)
        {
            Post post = await GetPostByIdAsync(postId);
            int index = post.Comments.FindIndex(c => c.Id == comment.Id);
            if (index != -1)
            {
                post.Comments[index] = comment;
                await UpdatePostAsync(postId, post);
            }
        }
        
        public async Task DeleteCommentFromPostAsync(string postId, string commentId)
        {
            Post post = await GetPostByIdAsync(postId);
            post.Comments.RemoveAll(c => c.Id == commentId);
            await UpdatePostAsync(postId, post);
        }
    }
} 