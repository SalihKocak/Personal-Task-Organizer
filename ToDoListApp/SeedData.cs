using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToDoListApp.Models;
using ToDoListApp.Services;

namespace ToDoListApp
{
    public class SeedData
    {
        private readonly MongoDBService _mongoDBService;
        
        public SeedData(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }
        
        public async Task SeedAsync()
        {
            await CleanDatabase();
            
            // Create demo users
            var users = await CreateUsers();
            
            // Create todos for each user
            foreach (var user in users)
            {
                await CreateTodosForUser(user);
            }
            
            // Create posts and comments
            await CreatePostsAndComments(users);
            
            // Extra todos and posts for Salih Furkan KOÇAK
            var salih = users.Find(u => u.Username == "sfkocak");
            if (salih != null)
            {
                await CreateExtraTodoItems(salih);
                await CreateExtraPosts(salih, users);
            }
        }
        
        private async Task CleanDatabase()
        {
            // Drop all collections
            var client = new MongoClient(_mongoDBService.ConnectionString);
            var database = client.GetDatabase(_mongoDBService.DatabaseName);
            
            await database.DropCollectionAsync("Users");
            await database.DropCollectionAsync("TodoItems");
            await database.DropCollectionAsync("Posts");
            
            // Recreate collections
            await database.CreateCollectionAsync("Users");
            await database.CreateCollectionAsync("TodoItems");
            await database.CreateCollectionAsync("Posts");
        }
        
        private async Task<List<User>> CreateUsers()
        {
            var users = new List<User>
            {
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "Salih Furkan",
                    LastName = "KOÇAK",
                    Username = "sfkocak",
                    Email = "sfkocak@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    ProfilePicture = "https://ui-avatars.com/api/?name=Salih+Furkan+KOÇAK&background=random&color=fff",
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "Ahmet",
                    LastName = "Yılmaz",
                    Username = "ayilmaz",
                    Email = "ahmet@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    ProfilePicture = "https://ui-avatars.com/api/?name=Ahmet+Yılmaz&background=random&color=fff",
                    CreatedDate = DateTime.Now.AddDays(-25)
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "Ayşe",
                    LastName = "Demir",
                    Username = "ademir",
                    Email = "ayse@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    ProfilePicture = "https://ui-avatars.com/api/?name=Ayşe+Demir&background=random&color=fff",
                    CreatedDate = DateTime.Now.AddDays(-20)
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "Mehmet",
                    LastName = "Kaya",
                    Username = "mkaya",
                    Email = "mehmet@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    ProfilePicture = "https://ui-avatars.com/api/?name=Mehmet+Kaya&background=random&color=fff",
                    CreatedDate = DateTime.Now.AddDays(-15)
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "Zeynep",
                    LastName = "Öztürk",
                    Username = "zozturk",
                    Email = "zeynep@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    ProfilePicture = "https://ui-avatars.com/api/?name=Zeynep+Öztürk&background=random&color=fff",
                    CreatedDate = DateTime.Now.AddDays(-10)
                }
            };
            
            foreach (var user in users)
            {
                await _mongoDBService.CreateUserAsync(user);
            }
            
            return users;
        }
        
        private async Task CreateTodosForUser(User user)
        {
            var random = new Random();
            var todoCount = random.Next(3, 7); // 3-6 todos per user
            
            var todoTitles = new List<string>
            {
                "Rapor hazırlama",
                "Toplantı notlarını düzenleme",
                "Müşteri araması",
                "Proje sunumu",
                "E-postaları yanıtlama",
                "Haftalık planlama",
                "Alışveriş listesi hazırlama",
                "Fatura ödemeleri",
                "Araştırma yapma",
                "Web sitesi güncellemesi",
                "Sosyal medya içeriği oluşturma",
                "Spor yapmak",
                "Kitap okumak",
                "Öğle yemeği planlaması",
                "Bütçe planlaması"
            };
            
            var todoDescriptions = new List<string>
            {
                "Aylık satış raporu hazırlanacak",
                "Proje toplantısı notlarını düzenle ve takıma gönder",
                "Potansiyel müşteri ile görüşme",
                "Yeni proje sunumunu tamamla",
                "Tüm okunmamış e-postaları yanıtla",
                "Gelecek hafta için planlama yap",
                "Haftalık alışveriş listesini hazırla",
                "Ay sonu faturalarını öde",
                "Yeni ürün için pazar araştırması yap",
                "Web sitesindeki içerikleri güncelle",
                "Haftanın sosyal medya içeriklerini planla",
                "Günlük egzersiz rutinini tamamla",
                "Başladığın kitabı bitir",
                "Öğle yemeği menüsünü planla",
                "Aylık bütçeyi gözden geçir"
            };
            
            for (int i = 0; i < todoCount; i++)
            {
                var status = (TodoStatus)random.Next(0, 4);
                var priority = (Priority)random.Next(0, 4);
                var daysOffset = random.Next(-5, 10);
                
                var todoTitle = todoTitles[random.Next(todoTitles.Count)];
                var todoDescription = todoDescriptions[random.Next(todoDescriptions.Count)];
                
                var todoItem = new TodoItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    Title = todoTitle,
                    Description = todoDescription,
                    Status = status,
                    Priority = priority,
                    DueDate = DateTime.Now.AddDays(daysOffset),
                    CreatedDate = DateTime.Now.AddDays(-random.Next(1, 10)),
                    CompletedDate = status == TodoStatus.Completed ? DateTime.Now.AddDays(-random.Next(0, 5)) : null
                };
                
                await _mongoDBService.CreateTodoItemAsync(todoItem);
                
                // Remove used items to avoid duplicates for the same user
                todoTitles.Remove(todoTitle);
                todoDescriptions.Remove(todoDescription);
                
                if (todoTitles.Count == 0 || todoDescriptions.Count == 0)
                    break;
            }
        }
        
        private async Task CreateExtraTodoItems(User user)
        {
            var todoItems = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    Title = "Web uygulaması geliştirme",
                    Description = ".NET Core kullanarak yapılacak to-do list uygulaması için geliştirme",
                    Status = TodoStatus.InProgress,
                    Priority = Priority.High,
                    DueDate = DateTime.Now.AddDays(7),
                    CreatedDate = DateTime.Now.AddDays(-10)
                },
                new TodoItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    Title = "Veritabanı tasarımı",
                    Description = "MongoDB ilişkisel yapıyı tasarlama",
                    Status = TodoStatus.Completed,
                    Priority = Priority.High,
                    DueDate = DateTime.Now.AddDays(-5),
                    CreatedDate = DateTime.Now.AddDays(-15),
                    CompletedDate = DateTime.Now.AddDays(-6)
                },
                new TodoItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    Title = "UI/UX tasarımı",
                    Description = "Kullanıcı arayüzü ve deneyimi için Bootstrap ve CSS ile tasarım",
                    Status = TodoStatus.Completed,
                    Priority = Priority.Medium,
                    DueDate = DateTime.Now.AddDays(-3),
                    CreatedDate = DateTime.Now.AddDays(-8),
                    CompletedDate = DateTime.Now.AddDays(-4)
                },
                new TodoItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    Title = "Topluluk özelliği ekleme",
                    Description = "Kullanıcı paylaşımları ve yorumlar için gerekli altyapıyı oluşturma",
                    Status = TodoStatus.InProgress,
                    Priority = Priority.Medium,
                    DueDate = DateTime.Now.AddDays(2),
                    CreatedDate = DateTime.Now.AddDays(-5)
                },
                new TodoItem
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    Title = "Demo verileri ekleme",
                    Description = "Uygulama tanıtımı için örnek veriler oluşturma",
                    Status = TodoStatus.Pending,
                    Priority = Priority.Low,
                    DueDate = DateTime.Now.AddDays(1),
                    CreatedDate = DateTime.Now.AddDays(-1)
                }
            };
            
            foreach (var todoItem in todoItems)
            {
                await _mongoDBService.CreateTodoItemAsync(todoItem);
            }
        }
        
        private async Task CreatePostsAndComments(List<User> users)
        {
            var random = new Random();
            
            var postTitles = new List<string>
            {
                "Verimlilik İpuçları",
                "Görev Yönetimi Stratejileri",
                "Zamanı Etkili Kullanma",
                "Pomodoro Tekniği Deneyimim",
                "Günlük Rutinler Oluşturmak",
                "Proje Yönetiminde Dikkat Edilmesi Gerekenler",
                "Motivasyonu Yüksek Tutma Yolları",
                "Erteleme Alışkanlığıyla Başa Çıkma",
                "Uzaktan Çalışma İpuçları",
                "İş-Yaşam Dengesi Sağlama",
                "Hedef Belirleme ve Takip Etme",
                "Üretkenliği Artıran Uygulamalar"
            };
            
            var postContents = new List<string>
            {
                "Verimli çalışmak için en önemli ipuçlarından biri, gün içinde belirli saatlerde çalışmak ve düzenli molalar vermektir. Ben genellikle sabah erken saatlerde daha verimli olduğumu fark ettim. Bu nedenle önemli görevlerimi sabah saatlerine planlıyorum. Ayrıca her 25 dakikalık çalışma sonrası 5 dakikalık molalar veriyorum (Pomodoro tekniği).\n\nBir diğer ipucum ise, görevleri önem ve aciliyet durumuna göre sınıflandırmak. Bu uygulama tam olarak bunu yapmanıza olanak sağlıyor. Her gün en az 3 önemli görev belirleyip onlara odaklanmak, gün sonunda başarı hissini artırıyor.",
                
                "Görev yönetimi konusunda uzun zamandır farklı yöntemler deniyorum. En etkili bulduğum strateji, görevleri parçalara ayırmak. Büyük bir proje yerine, onu küçük, yönetilebilir parçalara böldüğümde ilerleme kaydetmek çok daha kolay oluyor.\n\nAyrıca her gün için yapılacak görevlerin sayısını sınırlandırmak da oldukça faydalı. Ben genellikle bir günde en fazla 5-7 görevi listeliyorum. Böylece kendimi bunalmış hissetmiyorum ve odaklanabiliyorum.",
                
                "Zamanı etkili kullanmak için en önemli adım, zaman hırsızlarını belirlemek. Sosyal medya, sürekli gelen e-posta bildirimleri veya plansız toplantılar gibi faktörler üretkenliğimizi ciddi şekilde düşürebiliyor.\n\nBen son zamanlarda bildirimleri kapatıp, belirli zaman aralıklarında e-postalarımı kontrol etmeye başladım. Ayrıca toplantılarımı mümkün olduğunca aynı güne topladım. Bu sayede kesintisiz çalışabileceğim zaman dilimleri oluşturdum ve verimliliğim gözle görülür şekilde arttı.",
                
                "Pomodoro tekniğini yaklaşık 2 aydır uyguluyorum ve sonuçlarından çok memnunum. Bu teknik, 25 dakika yoğun çalışma ve ardından 5 dakika mola verme esasına dayanıyor. 4 pomodoro tamamlandıktan sonra ise 15-30 dakikalık uzun bir mola veriyorsunuz.\n\nBu yöntem sayesinde dikkatimi daha uzun süre koruyabiliyorum. Ayrıca molalar, zihnimin dinlenmesini ve yenilenmesini sağlıyor. Erteleme alışkanlığıyla mücadelede de oldukça etkili bir yöntem. \"Sadece bir pomodoro boyunca çalışacağım\" diyerek başlamak çok daha kolay oluyor.",
                
                "Günlük rutinler oluşturmak, üretkenliği artırmada en etkili yöntemlerden biri. Ben sabah rutini olarak; erken kalkmak, 15 dakika meditasyon yapmak, 30 dakika egzersiz yapmak ve günün planını oluşturmak gibi adımları takip ediyorum.\n\nAkşam rutini ise; günü değerlendirmek, ertesi gün için hazırlık yapmak ve kitap okumak şeklinde. Bu rutinler sayesinde hem zihinsel hem de fiziksel olarak daha hazır hissediyorum. Ayrıca belirsizliği azaltıp, kontrol hissini artırıyor.",
                
                "Proje yönetiminde en çok dikkat ettiğim nokta, net hedefler belirlemek ve bu hedeflere ulaşmak için gerekli adımları detaylı olarak planlamak. Her projeye başlamadan önce SMART (Specific, Measurable, Achievable, Relevant, Time-bound) hedefler belirliyorum.\n\nBir diğer önemli nokta ise iletişim. Proje ekibiyle düzenli toplantılar yapmak, ilerleyişi takip etmek ve olası sorunları erken tespit etmek çok önemli. Bu uygulama gibi dijital araçlar da proje takibini kolaylaştırıyor.",
                
                "Motivasyonu yüksek tutmak için kendime küçük ödüller belirliyorum. Örneğin, zorlu bir görevi tamamladığımda kendime bir kahve molası veya sevdiğim bir aktivite için zaman ayırıyorum.\n\nAyrıca ilerlemeyi görselleştirmek de motivasyon açısından çok etkili. Bu uygulamadaki gibi tamamlanan görevleri görmek ve başarı hissini yaşamak, yeni görevlere başlamak için itici güç oluyor. Bir diğer ipucum ise, kendinize neden bu görevi yaptığınızı hatırlatmak. Görevin size veya projenize nasıl katkı sağlayacağını düşünmek motivasyonu artırıyor.",
                
                "Erteleme alışkanlığıyla başa çıkmak için kullandığım en etkili yöntem, \"2 Dakika Kuralı\". Eğer bir görev 2 dakika veya daha az sürede tamamlanabilecekse, hemen yapıyorum, ertelemiyorum.\n\nDaha büyük görevler için ise, görevi daha küçük ve yönetilebilir parçalara bölmek işe yarıyor. Ayrıca en zor veya en az sevdiğim görevi günün başında yapmaya çalışıyorum. Bu şekilde, o görev zihnimde büyüyüp tüm günümü etkilemiyor.",
                
                "Uzaktan çalışma hayatımızın bir parçası haline geldi. Verimli bir uzaktan çalışma düzeni için özel bir çalışma alanı oluşturmak çok önemli. Bu alan sadece çalışma için ayrılmış olmalı ve mümkünse sessiz olmalı.\n\nAyrıca mesai saatlerini belirlemek ve bu saatlere uymak, iş-yaşam dengesini korumak için kritik. Ben genellikle çalışma saatlerimin başında ve sonunda 10 dakikalık \"geçiş rituelleri\" uyguluyorum. Bu, zihnimin çalışma moduna geçmesine veya çalışma modundan çıkmasına yardımcı oluyor.",
                
                "İş-yaşam dengesi sağlamak, uzun vadeli üretkenlik için çok önemli. Kendime her gün en az 1 saat tamamen kendime ayırdığım bir zaman dilimi belirliyorum. Bu sürede telefonu kapatıp, sevdiğim bir aktiviteye odaklanıyorum.\n\nHafta sonları ise mümkün olduğunca iş düşünmemeye çalışıyorum. Bu şekilde zihnimin dinlenmesini ve yenilenmesini sağlıyorum. Ayrıca düzenli tatiller planlamak da uzun vadeli motivasyon için çok önemli.",
                
                "Hedef belirleme ve takip etme konusunda SMART kriterleri dışında, hedefleri görselleştirmenin de çok faydasını görüyorum. Hedeflerimi bir not defterine yazıp, görünür bir yere asıyorum. Bu sürekli olarak hedeflerimi hatırlamamı sağlıyor.\n\nAyrıca hedeflere giden yolda ara kontrol noktaları belirlemek de çok önemli. Bu şekilde ilerleyişimi daha net görebiliyor ve gerekirse stratejimi değiştirebiliyorum. Bu uygulama gibi dijital araçlar da hedef takibini kolaylaştırıyor.",
                
                "Üretkenliği artıran birçok uygulama denedim ve bazıları gerçekten fark yaratıyor. Örneğin, bu TaskHub uygulaması görev yönetimi konusunda oldukça başarılı.\n\nBunun dışında Forest (dikkat dağınıklığıyla mücadele), Notion (not alma ve organizasyon), Trello (proje yönetimi) ve Focus@Will (odaklanmaya yardımcı müzik) gibi uygulamaları da kullanıyorum. Ancak asıl önemli olan, bu araçları nasıl kullandığınız. En iyi araçlar bile doğru kullanılmazsa fayda sağlamayabilir."
            };
            
            foreach (var user in users)
            {
                // 1-3 posts per user
                var postCount = random.Next(1, 4);
                
                for (int i = 0; i < postCount && postTitles.Count > 0; i++)
                {
                    int titleIndex = random.Next(postTitles.Count);
                    int contentIndex = random.Next(postContents.Count);
                    
                    string title = postTitles[titleIndex];
                    string content = postContents[contentIndex];
                    
                    // Tags
                    var tags = GetRandomTags();
                    
                    // Create post
                    var post = new Post
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        UserId = user.Id,
                        Title = title,
                        Content = content,
                        Tags = string.Join(", ", tags),
                        CreatedDate = DateTime.Now.AddDays(-random.Next(1, 20)),
                        Comments = new List<Comment>()
                    };
                    
                    // Add comments
                    int commentCount = random.Next(0, 4); // 0-3 comments per post
                    for (int j = 0; j < commentCount; j++)
                    {
                        var commenter = users[random.Next(users.Count)];
                        
                        var comment = new Comment
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            PostId = post.Id,
                            UserId = commenter.Id,
                            Content = GetRandomComment(),
                            CreatedDate = post.CreatedDate.AddDays(random.Next(1, 5))
                        };
                        
                        post.Comments.Add(comment);
                    }
                    
                    await _mongoDBService.CreatePostAsync(post);
                    
                    // Remove used title and content to avoid duplicates
                    postTitles.RemoveAt(titleIndex);
                    postContents.RemoveAt(contentIndex);
                }
            }
        }
        
        private async Task CreateExtraPosts(User user, List<User> allUsers)
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    Title = "TaskHub Uygulaması Geliştirme Süreci",
                    Content = "Merhaba arkadaşlar,\n\nSon birkaç aydır üzerinde çalıştığım TaskHub uygulamasını sizlerle paylaşmak istiyorum. Bu uygulama, günlük görevlerinizi yönetmenize ve diğer kullanıcılarla deneyimlerinizi paylaşmanıza olanak tanıyor.\n\nUygulama geliştirme sürecinde .NET Core ve MongoDB kullandım. Özellikle MongoDB'nin esnek yapısı, uygulama içindeki farklı veri modellerini yönetmekte büyük kolaylık sağladı.\n\nÖn yüz için Bootstrap ve modern CSS teknikleri kullanarak, kullanıcı dostu bir arayüz tasarlamaya çalıştım. Responsive tasarım sayesinde mobil cihazlarda da sorunsuz çalışıyor.\n\nGörev yönetimi kısmında, görevleri farklı kategorilere ayırabilir, öncelik belirleyebilir ve son tarih ekleyebilirsiniz. Tamamlanan görevleri işaretleyerek ilerlemenizi takip edebilirsiniz.\n\nUygulamanın en sevdiğim özelliklerinden biri de topluluk kısmı. Burada verimlilik ipuçlarınızı paylaşabilir, diğer kullanıcıların deneyimlerinden faydalanabilirsiniz.\n\nGeliştirme sürecinde karşılaştığım zorlukları ve çözümleri başka bir yazıda detaylı olarak paylaşacağım. Şimdilik uygulama hakkındaki görüş ve önerilerinizi bekliyorum.",
                    Tags = "web geliştirme, .NET Core, MongoDB, görev yönetimi, verimlilik",
                    CreatedDate = DateTime.Now.AddDays(-7),
                    Comments = new List<Comment>()
                },
                new Post
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    Title = "Görev Yönetiminde İpuçları",
                    Content = "Görev yönetimi konusunda uzun yıllardır farklı teknikler deniyorum ve en etkili bulduğum yöntemleri sizlerle paylaşmak istiyorum:\n\n1. Öncelik Matrisi: Görevleri önem ve aciliyet durumuna göre 4 kategoriye ayırın: Önemli ve Acil, Önemli ama Acil Değil, Önemsiz ama Acil, Ne Önemli Ne Acil. İlk kategoriden başlayarak sırayla ilerleyin.\n\n2. 2 Dakika Kuralı: Eğer bir görev 2 dakika veya daha az sürede tamamlanabiliyorsa, hemen yapın. Ertelemeyin.\n\n3. SMART Hedefler: Görevlerinizi Specific (Belirli), Measurable (Ölçülebilir), Achievable (Ulaşılabilir), Relevant (İlgili) ve Time-bound (Zamana Bağlı) olarak belirleyin.\n\n4. Pomodoro Tekniği: 25 dakika çalış, 5 dakika dinlen. 4 pomodoro sonrası 15-30 dakika uzun mola ver.\n\n5. Görevleri Gruplama: Benzer görevleri gruplandırarak, bağlam değiştirme maliyetini azaltın.\n\n6. Görevleri Parçalama: Büyük görevleri daha küçük, yönetilebilir parçalara bölün.\n\n7. En Zordan Başlama: Günün en zor göreviyle başlayarak, yüksek enerji ve motivasyonunuzu en çok ihtiyaç duyulan görevde kullanın.\n\nBu teknikleri kendi iş akışınıza göre uyarlayabilir ve hangisinin sizin için en etkili olduğunu keşfedebilirsiniz. Hepimizin çalışma şekli farklı, bu yüzden deneyerek size en uygun yöntemi bulmanız önemli.",
                    Tags = "verimlilik, görev yönetimi, zaman yönetimi, pomodoro",
                    CreatedDate = DateTime.Now.AddDays(-3),
                    Comments = new List<Comment>()
                }
            };
            
            var random = new Random();
            
            foreach (var post in posts)
            {
                // Add comments
                int commentCount = random.Next(2, 5); // 2-4 comments per post
                for (int j = 0; j < commentCount; j++)
                {
                    var commenter = allUsers[random.Next(allUsers.Count)];
                    
                    var comment = new Comment
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        PostId = post.Id,
                        UserId = commenter.Id,
                        Content = GetRandomComment(),
                        CreatedDate = post.CreatedDate.AddDays(random.Next(1, 3))
                    };
                    
                    post.Comments.Add(comment);
                }
                
                await _mongoDBService.CreatePostAsync(post);
            }
        }
        
        private List<string> GetRandomTags()
        {
            var allTags = new List<string>
            {
                "verimlilik", "görev yönetimi", "zaman yönetimi", "pomodoro", 
                "motivasyon", "planlama", "hedefler", "üretkenlik", 
                "çalışma teknikleri", "iş-yaşam dengesi", "erteleme", "odaklanma"
            };
            
            var random = new Random();
            var tagCount = random.Next(1, 5); // 1-4 tags per post
            var selectedTags = new List<string>();
            
            for (int i = 0; i < tagCount; i++)
            {
                if (allTags.Count == 0) break;
                
                int index = random.Next(allTags.Count);
                selectedTags.Add(allTags[index]);
                allTags.RemoveAt(index);
            }
            
            return selectedTags;
        }
        
        private string GetRandomComment()
        {
            var comments = new List<string>
            {
                "Çok faydalı bir paylaşım, teşekkürler!",
                "Bu ipuçlarını deneyeceğim, teşekkürler.",
                "Ben de benzer teknikleri kullanıyorum ve gerçekten işe yarıyor.",
                "Harika bir yazı olmuş, başka ipuçları da paylaşır mısın?",
                "Bu konuda tamamen katılıyorum. Günlük rutinler oluşturmak gerçekten çok önemli.",
                "Pomodoro tekniğini ben de kullanıyorum ve verimliliğimi gerçekten artırdı.",
                "İlginç bir bakış açısı, daha önce hiç bu şekilde düşünmemiştim.",
                "Bu uygulamayı bir süredir kullanıyorum ve hayatımı değiştirdi diyebilirim.",
                "Zamanı yönetmek konusunda her zaman zorlanıyordum, bu ipuçları çok işime yarayacak.",
                "Erteleme alışkanlığıyla mücadele etmek için başka önerilerin var mı?",
                "Hedeflerimi belirlemek ve takip etmek için hangi uygulamaları önerirsin?",
                "Bu yöntemleri işyerimde de uygulamaya başlayacağım, çok teşekkürler."
            };
            
            var random = new Random();
            return comments[random.Next(comments.Count)];
        }
    }
} 