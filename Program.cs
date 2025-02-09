using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace Final_Project
{
    public interface IBorrowable
    {
        void Borrow();
        void Return();
    }

    public class Book : IBorrowable
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
        public void Borrow()
        {
            IsAvailable = false;
        }
        public void Return()
        {
            IsAvailable = true;
        }
        public override string ToString()
        {
            return $"{Id}: {Title} by {Author} ({Year}) - {(IsAvailable ? "Available" : "Borrowed")}";
        }
    }
    public class EBook : Book
    {
        public string FileFormat { get; set; }
        public EBook(string title, string author, string genre, int year, string id, string fileFormat) : base(title, author, genre, year, id)
        {
            FileFormat = fileFormat;
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
        public void UpdateBook(string bookId, string newTitle, string newAuthor, string newGenre, int newYear)
        {
            var book = books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                book.Title = newTitle;
                book.Author = newAuthor;
                book.Genre = newGenre;
                book.Year = newYear;
                SaveData(BooksFile, books);
                LogAction($"Обновлена информация о книге \"{book.Title}\".");
            }
            else
            {
                Console.WriteLine("Ошибка: Книга не найдена.");
            }
        }

        public void SearchBooks(string keyword)
        {
            var results = books.Where(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                           b.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

            if (results.Any())
            {
                Console.WriteLine("Результаты поиска:");
                results.ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("Книг не найдено.");
            }
        }
        public IEnumerable<Book> FilterBooks(string genre, int? year = null)
        {
            return books.Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase) &&
                                    (year == null || b.Year == year));
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Library library = Library.Instance;

            while (true)
            {
                Console.WriteLine("\n=== Меню бібліотеки ===");
                Console.WriteLine("1. Додати книгу");
                Console.WriteLine("2. Оновити інформацію про книгу");
                Console.WriteLine("3. Видалити книгу");
                Console.WriteLine("4. Шукати книгу");
                Console.WriteLine("5. Показати всі книги");
                Console.WriteLine("6. Зареєструвати користувача");
                Console.WriteLine("7. Видалити користувача");
                Console.WriteLine("8. Показати всіх користувачів");
                Console.WriteLine("9. Видати книгу користувачу");
                Console.WriteLine("10. Повернути книгу");
                Console.WriteLine("11. Показати книги, які взяв користувач");
                Console.WriteLine("0. Вийти");
                Console.Write("Оберіть опцію: ");
                string choice = Console.ReadLine();
                Console.Clear();

                if (choice == "1")
                    AddBook(library);
                else if (choice == "2")
                    UpdateBook(library);
                else if (choice == "3")
                    RemoveBook(library);
                else if (choice == "4")
                    SearchBook(library);
                else if (choice == "5")
                    library.DisplayBooks();
                else if (choice == "6")
                    AddUser(library);
                else if (choice == "7")
                    RemoveUser(library);
                else if (choice == "8")
                    library.DisplayUsers();
                else if (choice == "9")
                    BorrowBook(library);
                else if (choice == "10")
                    ReturnBook(library);
                else if (choice == "11")
                    DisplayUserBooks(library);
                else if (choice == "0")
                    return;
                else
                    Console.WriteLine("Невідома опція, спробуйте ще раз.");
            }
        }
        static void AddBook(Library library)
        {
            Console.Write("Назва: ");
            string title = Console.ReadLine();
            Console.Write("Автор: ");
            string author = Console.ReadLine();
            Console.Write("Жанр: ");
            string genre = Console.ReadLine();
            Console.Write("Рік видання: ");
            int year = int.Parse(Console.ReadLine());
            Console.Write("ID книги: ");
            string id = Console.ReadLine();

            Book book = new Book(title, author, genre, year, id);
            library.AddBook(book);
        }
        static void UpdateBook(Library library)
        {
            Console.Write("Введіть ID книги для оновлення: ");
            string id = Console.ReadLine();
            Console.Write("Нова назва: ");
            string title = Console.ReadLine();
            Console.Write("Новий автор: ");
            string author = Console.ReadLine();
            Console.Write("Новий жанр: ");
            string genre = Console.ReadLine();
            Console.Write("Новий рік видання: ");
            int year = int.Parse(Console.ReadLine());

            library.UpdateBook(id, title, author, genre, year);
        }
        static void RemoveBook(Library library)
        {
            Console.Write("Введіть ID книги для видалення: ");
            string id = Console.ReadLine();
            library.RemoveBook(id);
        }
        static void SearchBook(Library library)
        {
            Console.Write("Введіть назву або автора книги: ");
            string keyword = Console.ReadLine();
            library.SearchBooks(keyword);
        }
        static void AddUser(Library library)
        {
            Console.Write("Ім'я: ");
            string firstName = Console.ReadLine();
            Console.Write("Прізвище: ");
            string lastName = Console.ReadLine();
            Console.Write("ID користувача: ");
            string userId = Console.ReadLine();

            User user = new User(firstName, lastName, userId);
            library.AddUser(user);
        }

        static void RemoveUser(Library library)
        {
            Console.Write("Введіть ID користувача для видалення: ");
            string id = Console.ReadLine();
            library.RemoveUser(id);
        }

        static void BorrowBook(Library library)
        {
            Console.Write("ID користувача: ");
            string userId = Console.ReadLine();
            Console.Write("ID книги: ");
            string bookId = Console.ReadLine();
            library.BorrowBook(userId, bookId);
        }

        static void ReturnBook(Library library)
        {
            Console.Write("ID користувача: ");
            string userId = Console.ReadLine();
            Console.Write("ID книги: ");
            string bookId = Console.ReadLine();
            library.ReturnBook(userId, bookId);
        }

        static void DisplayUserBooks(Library library)
        {
            Console.Write("ID користувача: ");
            string userId = Console.ReadLine();
            library.DisplayUserBooks(userId);
        }
    }
}
