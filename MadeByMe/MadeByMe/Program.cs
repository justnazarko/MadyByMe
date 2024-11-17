using System;
using System.Collections.Generic;
using Bogus;
using Npgsql;

class Program
{
    static void Main(string[] args)
    {
        string connString = "Host=localhost;Port=5432;Username=postgres;Password=8040;Database=MadeByMe";
        InsertRandomData(connString, 0, 0, 0, 0, 0);
        PrintAllTables(connString);
    }

    static void InsertRandomData(string connString, int userCount, int categoryCount, int postCount, int reviewCount, int selectedPostCount)
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.MobileNumber, f => f.Phone.PhoneNumber("+380#########"))
            .RuleFor(u => u.Email, f => f.Internet.Email());

        var categoryFaker = new Faker<Category>()
            .RuleFor(c => c.Title, f => f.Commerce.Categories(1)[0]);

        var postFaker = new Faker<Post>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence(10))
            .RuleFor(p => p.PhotoPath, f => f.Image.PicsumUrl())
            .RuleFor(p => p.CategoryRef, f => f.Random.Int(1, categoryCount))
            .RuleFor(p => p.SellerRef, f => f.Random.Int(1, userCount))
            .RuleFor(p => p.PayCard, f => f.Finance.CreditCardNumber())
            .RuleFor(p => p.Status, f => f.PickRandom(new[] { "in stock", "sold" }))
            .RuleFor(p => p.Rating, f => f.Random.Decimal(1, 5));

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            var users = userFaker.Generate(userCount);
            foreach (var user in users)
            {
                var sql = "INSERT INTO Users (name, mobile_number, email) VALUES (@name, @mobile_number, @email)";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("name", user.Name);
                    cmd.Parameters.AddWithValue("mobile_number", user.MobileNumber);
                    cmd.Parameters.AddWithValue("email", user.Email);
                    cmd.ExecuteNonQuery();
                }
            }

            var categories = categoryFaker.Generate(categoryCount);
            foreach (var category in categories)
            {
                var sql = "INSERT INTO Categories (title) VALUES (@title)";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("title", category.Title);
                    cmd.ExecuteNonQuery();
                }
            }

            var posts = postFaker.Generate(postCount);
            foreach (var post in posts)
            {
                var sql = @"
                    INSERT INTO Posts (name, description, photo_path, category_ref, seller_ref, pay_card, status, rating)
                    VALUES (@name, @description, @photo_path, @category_ref, @seller_ref, @pay_card, @status, @rating)";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("name", post.Name);
                    cmd.Parameters.AddWithValue("description", post.Description);
                    cmd.Parameters.AddWithValue("photo_path", post.PhotoPath);
                    cmd.Parameters.AddWithValue("category_ref", post.CategoryRef);
                    cmd.Parameters.AddWithValue("seller_ref", post.SellerRef);
                    cmd.Parameters.AddWithValue("pay_card", post.PayCard.Length > 16 ? post.PayCard.Substring(0, 16) : post.PayCard);
                    cmd.Parameters.AddWithValue("status", post.Status);
                    cmd.Parameters.AddWithValue("rating", post.Rating);
                    cmd.ExecuteNonQuery();
                }
            }

            var reviewFaker = new Faker<Review>()
                .RuleFor(r => r.ReviewContent, f => f.Lorem.Sentence(10))
                .RuleFor(r => r.AuthorRef, f => f.Random.Short(1, 50))
                .RuleFor(r => r.PostRef, f => f.Random.Short(1, 50));

            var reviews = reviewFaker.Generate(reviewCount);
            foreach (var review in reviews)
            {
                var sql = @"
                INSERT INTO Reviews (review_content, author_ref, post_ref)
                VALUES (@review_content, @author_ref, @post_ref)";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("review_content", review.ReviewContent);
                    cmd.Parameters.AddWithValue("author_ref", review.AuthorRef);
                    cmd.Parameters.AddWithValue("post_ref", review.PostRef);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
                    {
                        Console.WriteLine($"Пропущено відгук через некоректні посилання (author_ref={review.AuthorRef}, post_ref={review.PostRef}).");
                    }
                }
            }

            var selectedPostFaker = new Faker<SelectedPost>()
                .RuleFor(sp => sp.UserRef, f => f.Random.Short(1, 50))
                .RuleFor(sp => sp.PostRef, f => f.Random.Short(1, 50));

            var selectedPosts = selectedPostFaker.Generate(selectedPostCount);
            var insertedPairs = new HashSet<(short, short)>();

            foreach (var selectedPost in selectedPosts)
            {
                if (!insertedPairs.Add((selectedPost.UserRef, selectedPost.PostRef)))
                    continue;

                var sql = @"
                INSERT INTO Selected_posts (user_ref, post_ref)
                VALUES (@user_ref, @post_ref)";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("user_ref", selectedPost.UserRef);
                    cmd.Parameters.AddWithValue("post_ref", selectedPost.PostRef);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
                    {
                        Console.WriteLine($"Пропущено зв'язок через некоректні посилання (user_ref={selectedPost.UserRef}, post_ref={selectedPost.PostRef}).");
                    }
                    catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505")
                    {
                        Console.WriteLine($"Пропущено дублюючий зв'язок (user_ref={selectedPost.UserRef}, post_ref={selectedPost.PostRef}).");
                    }
                }
            }
        }
    }

    static void PrintAllTables(string connString)
    {
        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();
            Console.WriteLine("Users:");
            PrintTable(conn, "Users", new[] { "user_id", "name", "mobile_number", "email" });

            Console.WriteLine("\nCategories:");
            PrintTable(conn, "Categories", new[] { "category_id", "title" });

            Console.WriteLine("\nPosts:");
            PrintTable(conn, "Posts", new[] { "post_id", "name", "description", "photo_path", "category_ref", "seller_ref", "pay_card", "status", "rating" });

            Console.WriteLine("\nReviews:");
            PrintTable(conn, "Reviews", new[] { "review_id", "review_content", "author_ref", "post_ref" });

            Console.WriteLine("\nSelected_posts:");
            PrintTable(conn, "Selected_posts", new[] { "user_ref", "post_ref" });
        }
    }

    static void PrintTable(NpgsqlConnection conn, string tableName, string[] columns)
    {
        var sql = $"SELECT * FROM {tableName}";
        using (var cmd = new NpgsqlCommand(sql, conn))
        using (var reader = cmd.ExecuteReader())
        {
            int[] columnWidths = new int[columns.Length];

            // Обчислюємо максимальну довжину значень у кожному стовпці для вирівнювання
            foreach (var column in columns)
            {
                columnWidths[Array.IndexOf(columns, column)] = column.Length;
            }

            // Знаходимо максимальну ширину для кожного стовпця
            while (reader.Read())
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    int currentLength = reader[columns[i]].ToString().Length;
                    if (currentLength > columnWidths[i])
                    {
                        columnWidths[i] = currentLength;
                    }
                }
            }

            // Друкуємо заголовки колонок
            for (int i = 0; i < columns.Length; i++)
            {
                Console.Write(columns[i].PadRight(columnWidths[i] + 2)); // Додаємо додатковий відступ
            }
            Console.WriteLine();

            // Друкуємо рядки таблиці
            reader.Close(); // Потрібно закрити попередній reader, щоб знову виконати запит
            using (var cmd2 = new NpgsqlCommand(sql, conn))
            using (var reader2 = cmd2.ExecuteReader())
            {
                while (reader2.Read())
                {
                    for (int i = 0; i < columns.Length; i++)
                    {
                        Console.Write(reader2[columns[i]].ToString().PadRight(columnWidths[i] + 2)); // Виведення значень з вирівнюванням
                    }
                    Console.WriteLine();
                }
            }
        }
    }

}

class User
{
    public string Name { get; set; }
    public string MobileNumber { get; set; }
    public string Email { get; set; }
}

class Category
{
    public string Title { get; set; }
}

class Post
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string PhotoPath { get; set; }
    public int CategoryRef { get; set; }
    public int SellerRef { get; set; }
    public string PayCard { get; set; }
    public string Status { get; set; }
    public decimal Rating { get; set; }
}

class Review
{
    public string ReviewContent { get; set; }
    public short AuthorRef { get; set; }
    public short PostRef { get; set; }
}

class SelectedPost
{
    public short UserRef { get; set; }
    public short PostRef { get; set; }
}
