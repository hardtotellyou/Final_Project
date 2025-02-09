using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace Final_Project
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public string Id { get; set; }
        public bool IsAvailable { get; set; } = true;
        public Book() { }
        public Book(string title, string author, string genre, int year, string id)
        {
            Title = title;
            Author = author;
            Genre = genre;
            Year = year;
            Id = id;
        }
        public override string ToString()
        {
            return $"{Id}: {Title} by {Author} ({Year}) - {(IsAvailable ? "Available" : "Borrowed")}";
        }
    }
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public User() { }
        public User(string firstName, string lastName, string userId)
        {
            FirstName = firstName;
            LastName = lastName;
            UserId = userId;
        }
        public override string ToString()
        {
            return $"{UserId}: {FirstName} {LastName}";
        }
    }
    public class Library
    {
        private static Library _instance;
        private List<Book> books;
        private List<User> users;
        private Dictionary<string, List<Book>> borrowedBooks;
        private const string BooksFile = "books.xml";
        private const string UsersFile = "users.xml";
        private const string LogFile = "log.txt";
        private Library()
        {
            books = LoadData<List<Book>>(BooksFile) ?? new List<Book>();
            users = LoadData<List<User>>(UsersFile) ?? new List<User>();
            borrowedBooks = new Dictionary<string, List<Book>>();
        }

        public static Library Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Library();
                return _instance;
            }
        }
        private void SaveData<T>(string fileName, T data)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(fs, data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при збереженні файлу {fileName}: {ex.Message}");
            }
        }
        private T LoadData<T>(string fileName) where T : class
        {
            try
            {
                if (File.Exists(fileName))
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        return serializer.Deserialize(fs) as T;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при завантаженні файлу {fileName}: {ex.Message}");
            }
            return null;
        }
        private void LogAction(string action)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFile, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {action}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при записі в лог-файл: {ex.Message}");
            }
        }
        public void AddBook(Book book)
        {
                if (books.Any(b => b.Id == book.Id))
                {
                    Console.WriteLine("Помилка: Книга з таким ID вже існує.");
                    return;
                }
            books.Add(book);
            SaveData(BooksFile, books);
            LogAction($"Книга \"{book.Title}\" додана до бібліотеки.");
            Console.WriteLine($"Книга \"{book.Title}\" додана до бібліотеки.");
        }
        public void RemoveBook(string bookId)
        {
            Book book = books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                books.Remove(book);
                SaveData(BooksFile, books);
                LogAction($"Книга \"{book.Title}\" була видалена.");
                Console.WriteLine($"Книга \"{book.Title}\" видалена з бібліотеки.");
            }
            else
            {
                Console.WriteLine("Помилка: Книга не знайдена.");
            }
        }
        public void DisplayBooks()
        {
            if (books.Count == 0)
            {
                Console.WriteLine("Бібліотека порожня.");
                return;
            }

            Console.WriteLine("Список книг у бібліотеці:");
            foreach (var book in books)
            {
                Console.WriteLine(book);
            }
        }
        public void AddUser(User user)
        {
            if (users.Any(u => u.UserId == user.UserId))
            {
                Console.WriteLine("Помилка: Користувач з таким ID вже існує.");
                return;
            }
            users.Add(user);
            SaveData(UsersFile, users);
            LogAction($"Користувач {user.FirstName} {user.LastName} був зареєстрований.");
            Console.WriteLine($"Користувач {user.FirstName} {user.LastName} доданий.");
        }
        public void RemoveUser(string userId)
        {
            User user = users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                users.Remove(user);
                LogAction($"Користувач {user.FirstName} {user.LastName} був видалений.");
                Console.WriteLine($"Користувач {user.FirstName} {user.LastName} видалений.");
            }
            else
            {
                Console.WriteLine("Помилка: Користувач не знайдений.");
            }
        }
        public void DisplayUsers()
        {
            if (users.Count == 0)
            {
                SaveData(UsersFile, users);
                Console.WriteLine("Немає зареєстрованих користувачів.");
                return;
            }

            Console.WriteLine("Список користувачів:");
            foreach (var user in users)
            {
                Console.WriteLine(user);
            }
        }
        public void BorrowBook(string userId, string bookId)
        {
            User user = users.FirstOrDefault(u => u.UserId == userId);
            Book book = books.FirstOrDefault(b => b.Id == bookId);

            if (user == null)
            {
                Console.WriteLine("Помилка: Користувач не знайдений.");
                return;
            }

            if (book == null)
            {
                Console.WriteLine("Помилка: Книга не знайдена.");
                return;
            }

            if (!book.IsAvailable)
            {
                Console.WriteLine($"Книга \"{book.Title}\" уже взята.");
                return;
            }

            book.IsAvailable = false;

            if (!borrowedBooks.ContainsKey(userId))
            {
                borrowedBooks[userId] = new List<Book>();
            }
            borrowedBooks[userId].Add(book);
            LogAction($"Користувач {user.FirstName} {user.LastName} взяв книгу \"{book.Title}\".");
            Console.WriteLine($"Користувач {user.FirstName} {user.LastName} взяв книгу \"{book.Title}\".");
        }
        public void ReturnBook(string userId, string bookId)
        {
            if (!borrowedBooks.ContainsKey(userId) || borrowedBooks[userId].Count == 0)
            {
                Console.WriteLine("Помилка: Користувач не має взятих книг.");
                return;
            }

            Book book = borrowedBooks[userId].FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                Console.WriteLine("Помилка: Книга не знайдена серед взятих.");
                return;
            }

            borrowedBooks[userId].Remove(book);
            book.IsAvailable = true;
            LogAction($"Користувач повернув книгу \"{book.Title}\".");
            Console.WriteLine($"Користувач повернув книгу \"{book.Title}\".");
        }
        public void DisplayUserBooks(string userId)
        {
            if (!borrowedBooks.ContainsKey(userId) || borrowedBooks[userId].Count == 0)
            {
                Console.WriteLine("Користувач не має взятих книг.");
                return;
            }

            Console.WriteLine($"Список книг, які взяв користувач {userId}:");
            foreach (var book in borrowedBooks[userId])
            {
                Console.WriteLine(book);
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
                
        }
    }
}
