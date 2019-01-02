using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDbApp;
using MongoDbApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            SaveDocs().GetAwaiter().GetResult();
            FindBookWithCountBiggerThenOne().GetAwaiter().GetResult();
            FindBookWithMinAndMaxCount().GetAwaiter().GetResult();
            FindAuthors().GetAwaiter().GetResult();
            FindBooksWithoutAuthors().GetAwaiter().GetResult();
            UpdateCountOfBooks().GetAwaiter().GetResult();
            AddGenre().GetAwaiter().GetResult();
            DeleteBooksWhereCountLessThenThree().GetAwaiter().GetResult();
            DeleteAllBooks();
            Console.ReadLine();
            
        }

        private static async Task GetDatabaseNames(MongoClient client)
        {
            using (var cursor = await client.ListDatabasesAsync())
            {
                var databaseDocuments = await cursor.ToListAsync();
                foreach (var databaseDocument in databaseDocuments)
                {
                    Console.WriteLine(databaseDocument["name"]);
                }
            }
        }

        private static async Task SaveDocs()
        {
            var db = new MongoDb("mongodb://localhost");
            var database = db.GetDataBase("BookStore2");
            var collection = database.GetCollection<Book>("Books");
            var books = new List<Book>(5) {
                new Book
                {
                    Name = "Hobbit",
                    Author = "Tolkien",
                    Count = 5,
                    Genre = new string[] { "fantasy" },
                    Year = 2014
                },

                new Book
                {
                    Name = "Lord of the rings",
                    Author = "Tolkien",
                    Count = 3,
                    Genre = new string[] { "fantasy" },
                    Year = 2015
                },
                new Book
                {
                    Name = "Kolobok",
                    Author = "",
                    Count = 10,
                    Genre = new string[] { "kids" },
                    Year = 2000
                },
                new Book
                {
                    Name = "Repka",
                    Author = "",
                    Count = 11,
                    Genre = new string[] { "kids" },
                    Year = 2000
                },
                new Book
                {
                    Name = "Dyadya Stiopa",
                    Author = "Mihalkov",
                    Count = 1,
                    Genre = new string[] { "kids" },
                    Year = 2001
                }};
     
            
            await collection.InsertManyAsync(books);
        }

        private static async Task FindBookWithCountBiggerThenOne()
        {
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BookStore2");
            var collection = database.GetCollection<Book>("Books");
            
            var filter = Builders<Book>.Filter.Gt("Count", 1);
            var books = await collection.Find(filter).ToListAsync();


            foreach (var b in books.OrderBy(book => book.Name)) {
                Console.WriteLine(b.Name);
            }

            Console.WriteLine();
            Console.WriteLine($"How many books:{books.Count}");
        }

        private static async Task FindBookWithMinAndMaxCount()
        {
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BookStore2");
            var collection = database.GetCollection<Book>("Books");

            var min = await collection.Find(x => true).SortBy(d => d.Count).Limit(1).FirstOrDefaultAsync();
            var max = await collection.Find(x => true).SortByDescending(d => d.Count).Limit(1).FirstOrDefaultAsync();

            Console.WriteLine($"min: name: {min.Name}, count: {min.Count}");
            Console.WriteLine($"max: name: {max.Name}, count: {max.Count}");
        }


        private static async Task FindAuthors()
        {
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BookStore2");
            var collection = database.GetCollection<Book>("Books");
            var filter = new BsonDocument();
            var books = await collection.Find(filter).ToListAsync();
            var enumerable = books.GroupBy(b => b.Author).Select(g => g.First());
            foreach (var book in enumerable)
            {
                Console.WriteLine(book.Author);
            }
        }

        private static async Task FindBooksWithoutAuthors()
        {
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BookStore2");
            var collection = database.GetCollection<Book>("Books");

            var filter = Builders<Book>.Filter.Eq("Author", "");
            var books = await collection.Find(filter).ToListAsync();
            foreach (var b in books.OrderBy(book => book.Name)) {
                Console.WriteLine(b.Name);
            }
        }

        private static async Task UpdateCountOfBooks()
        {
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BookStore2");
            var collection = database.GetCollection<Book>("Books");
            var filter = new BsonDocument();
            var update = Builders<Book>.Update.Inc("Count", 1);
            var result = await collection.UpdateManyAsync(filter, update);
            Console.WriteLine("найдено по соответствию: {0}; обновлено: {1}",
                result.MatchedCount, result.ModifiedCount);
            var books = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (var b in books)
            {
                Console.WriteLine($"name: {b.Name}, count:{b.Count}");
            }
        }

        private static async Task AddGenre()
        {
            string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BookStore2");
            var collection = database.GetCollection<Book>("Books");
            var filter = Builders<Book>.Filter.Eq("Genre", new string[] {"fantasy"});
            var update = Builders<Book>.Update.Set("Genre", new string[] {"fantasy", "favority" });
            var result = await collection.UpdateManyAsync(filter, update);
            Console.WriteLine("найдено по соответствию: {0}; обновлено: {1}",
                result.MatchedCount, result.ModifiedCount);
            var books = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (var b in books)
            {
                Console.WriteLine($"name: {b.Name}");
                Console.WriteLine($"Genre: ");
                foreach (var genre in b.Genre)
                {
                    Console.Write(genre);
                }
            }
        }

        private static async Task DeleteBooksWhereCountLessThenThree()
        {
            var client = new MongoClient("mongodb://localhost");
            var database = client.GetDatabase("BookStore2");
            var collection = database.GetCollection<Book>("Books");
            var result = await collection.DeleteManyAsync(p => p.Count < 3);
            Console.WriteLine("Deleted: {0}", result.DeletedCount);
            var books = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (var b in books)
                Console.WriteLine(b.Name);
        }


        private static void DeleteAllBooks()
        {
            var client = new MongoClient("mongodb://localhost");
            var database = client.GetDatabase("BookStore2");
            database.DropCollection("BookStore2");
        }
    }
}
