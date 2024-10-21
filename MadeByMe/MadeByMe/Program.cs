using System;
using Npgsql;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=8040;Database=MadeByMe";

        //InsertRandomUsers(connectionString, 30);
        //InsertRandomCategories(connectionString, 5);
        //InsertRandomPosts(connectionString, 30);
        //InsertRandomSelectedPosts(connectionString, 30);
        //InsertRandomUsersAndSelectedPosts(connectionString, 30);
        //InsertRandomReviews(connectionString, 30);

        DisplayAllTables(connectionString);
    }

    static void InsertRandomUsers(string connString, int recordCount)
    {
        var random = new Random();
        var names = new List<string> { "Олена", "Катерина", "Марія", "Софія", "Людмила", "Петро", "Владислав", "Тарас", "Злата", "Юлія","Михайло", "Галина" };

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            for (int i = 1; i <= recordCount; i++)
            {
                string name = names[random.Next(names.Count)] + " " + names[random.Next(names.Count)];
                string mobile = "+380" + random.Next(100000000, 999999999).ToString();
                string email = name.Replace(" ", "").ToLower() + "@example.com";

                var sql = $"INSERT INTO Users (users_id, users_name, mobile_number, email) VALUES ({i}, '{name}', '{mobile}', '{email}')";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        Console.WriteLine($"{recordCount} записів вставлено в таблицю Users.");
    }

    static void InsertRandomCategories(string connString, int recordCount)
    {
        var random = new Random();
        var categories = new List<string> { "Одяг", "Біжутерія", "Іграшки", "Брилки", "Інше" };

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            for (int i = 0; i < recordCount; i++)
            {
                string title = categories[random.Next(categories.Count)] + " ";

                var sql = $"INSERT INTO Categories (title) VALUES ('{title}')";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        Console.WriteLine($"{recordCount} записів вставлено в таблицю Categories.");
    }

    static void InsertRandomPosts(string connString, int recordCount)
    {
        var random = new Random();
        var postNames = new List<string> { "Підвіска", "Брилок на телефон", "Майка", "Джинси", "Букет", "Валіза" };
        var descriptions = new List<string> { "Зроблено з натуральних матеріалів", "Купуйте в подарунок своїм друзям!", "Допоможіть назбирати на мрію(", "За деталями звертайтесь в пп" };
        var photoBasePath = "Post_Photos/";

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            var categories = new List<string>();
            var getCategorySql = "SELECT title FROM Categories";
            using (var getCategoryCmd = new NpgsqlCommand(getCategorySql, conn))
            {
                using (var reader = getCategoryCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(reader.GetString(0));
                    }
                }
            }

            for (int i = 1; i <= recordCount; i++)
            {
                string postName = postNames[random.Next(postNames.Count)];
                string description = descriptions[random.Next(descriptions.Count)];
                string category = categories[random.Next(categories.Count)]; 
                string photoPath = $"{photoBasePath}{i}"; 
                int sellerId = random.Next(1, 31); 
                string payCard = "1234567812345678"; 
                string status = random.Next(0, 2) == 0 ? "in stock" : "sold"; 
                decimal rating = Math.Round((decimal)random.NextDouble() * 5, 2); 

                string insertQuery = @"
                INSERT INTO Posts (post_id, post_name, description, photo, category, seller, pay_card, status, rating)
                 VALUES (@post_id, @post_name, @description, @photo, @category, @seller, @pay_card, @status, @rating)";
                using (var cmd = new NpgsqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("post_id", i);
                    cmd.Parameters.AddWithValue("post_name", postName);
                    cmd.Parameters.AddWithValue("description", description);
                    cmd.Parameters.AddWithValue("photo", photoPath);
                    cmd.Parameters.AddWithValue("category", category);
                    cmd.Parameters.AddWithValue("seller", sellerId);
                    cmd.Parameters.AddWithValue("pay_card", payCard);
                    cmd.Parameters.AddWithValue("status", status);
                    cmd.Parameters.AddWithValue("rating", rating);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        Console.WriteLine($"{recordCount} записів вставлено в таблицю Posts.");
    }

    static void InsertRandomSelectedPosts(string connString, int recordCount)
    {
        var random = new Random();

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            for (int i = 1; i <= recordCount; i++)
            {
                int postId = random.Next(1, 31); 

                var sql = $"INSERT INTO Selected_posts (selected_post_id, post_id) VALUES ({i}, {postId})";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        Console.WriteLine($"{recordCount} записів вставлено в таблицю Selected_posts.");
    }

    static void InsertRandomUsersAndSelectedPosts(string connString, int recordCount)
    {
        var random = new Random();

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            for (int i = 1; i <= recordCount; i++)
            {
                int userId = random.Next(1, 31);
                int selectedPostId = random.Next(1, 31);

                var sql = $"INSERT INTO Users_and_Selected_posts (user_, selected_post) VALUES ({userId}, {selectedPostId})";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        Console.WriteLine($"{recordCount} записів вставлено в таблицю Users_and_Selected_posts.");
    }
    static void InsertRandomReviews(string connString, int recordCount)
    {
        var random = new Random();
        var reviewTexts = new List<string>
    {
        "Чудовий продукт!",
        "Дуже рекомендую!",
        "Дивовижна майстерність.",
        "Гарне співвідношення ціни та якості",
        "Куплю знову!",
        "Не так, як очікувалося",
        "Дуже задоволений",
        "Швидка доставка та відмінна якість"
    };

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            for (int i = 1; i <= recordCount; i++)
            {
                string reviewContent = reviewTexts[random.Next(reviewTexts.Count)];
                int userId = random.Next(1, 31); 
                int postId = random.Next(1, 31); 

                var sql = $@"
                INSERT INTO Reviews (review_id, review_content, author, post) 
                VALUES ({i}, '{reviewContent}', {userId}, {postId})";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        Console.WriteLine($"{recordCount} записів вставлено в таблицю Reviews.");
    }

    static void DisplayAllTables(string connString)
    {
        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            
            Console.WriteLine("Таблиця Users:");
            var usersSql = "SELECT users_id, users_name, mobile_number, email FROM Users";
            using (var usersCmd = new NpgsqlCommand(usersSql, conn))
            {
                using (var reader = usersCmd.ExecuteReader())
                {
                    Console.WriteLine($"{"ID",-5} {"Ім'я",-20} {"Телефон",-15} {"Email",-30}");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetInt16(0),-5} {reader.GetString(1),-20} {reader.GetString(2),-15} {reader.GetString(3),-30}");
                    }
                }
            }
            Console.WriteLine();

            
            Console.WriteLine("Таблиця Categories:");
            var categoriesSql = "SELECT title FROM Categories";
            using (var categoriesCmd = new NpgsqlCommand(categoriesSql, conn))
            {
                using (var reader = categoriesCmd.ExecuteReader())
                {
                    Console.WriteLine($"{"Категорія",-30}");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetString(0),-30}");
                    }
                }
            }
            Console.WriteLine();

            
            Console.WriteLine("Таблиця Posts:");
            var postsSql = "SELECT post_id, post_name, description, photo, category, seller, pay_card, status, rating FROM Posts";
            using (var postsCmd = new NpgsqlCommand(postsSql, conn))
            {
                using (var reader = postsCmd.ExecuteReader())
                {
                    Console.WriteLine($"{"ID",-5} {"Назва",-20} {"Опис",-40} {"Фото",-20} {"Категорія",-15} {"Продавець",-10} {"Картка",-20} {"Статус",-15} {"Рейтинг",-10}");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetInt16(0),-5} {reader.GetString(1),-20} {reader.GetString(2),-40} {reader.GetString(3),-20} {reader.GetString(4),-15} {reader.GetInt16(5),-10} {reader.GetString(6),-20} {reader.GetString(7),-15} {reader.GetDecimal(8),-10}");
                    }
                }
            }
            Console.WriteLine();

            
            Console.WriteLine("Таблиця Selected_posts:");
            var selectedPostsSql = "SELECT selected_post_id, post_id FROM Selected_posts";
            using (var selectedPostsCmd = new NpgsqlCommand(selectedPostsSql, conn))
            {
                using (var reader = selectedPostsCmd.ExecuteReader())
                {
                    Console.WriteLine($"{"Selected Post ID",-20} {"Post ID",-10}");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetInt16(0),-20} {reader.GetInt16(1),-10}");
                    }
                }
            }
            Console.WriteLine();

            
            Console.WriteLine("Таблиця Users_and_Selected_posts:");
            var usersSelectedPostsSql = "SELECT user_, selected_post FROM Users_and_Selected_posts";
            using (var usersSelectedPostsCmd = new NpgsqlCommand(usersSelectedPostsSql, conn))
            {
                using (var reader = usersSelectedPostsCmd.ExecuteReader())
                {
                    Console.WriteLine($"{"User ID",-10} {"Selected Post ID",-20}");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetInt16(0),-10} {reader.GetInt16(1),-20}");
                    }
                }
            }
            Console.WriteLine();

            
            Console.WriteLine("Таблиця Reviews:");
            var reviewsSql = "SELECT review_id, review_content, author, post FROM Reviews";
            using (var reviewsCmd = new NpgsqlCommand(reviewsSql, conn))
            {
                using (var reader = reviewsCmd.ExecuteReader())
                {
                    Console.WriteLine($"{"Review ID",-10} {"Вміст",-50} {"Автор",-10} {"Post ID",-10}");
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader.GetInt16(0),-10} {reader.GetString(1),-50} {reader.GetInt16(2),-10} {reader.GetInt16(3),-10}");
                    }
                }
            }
            Console.WriteLine();
        }
    }

}
