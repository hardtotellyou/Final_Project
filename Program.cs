using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace Final_Project
{
    // Інтерфейс IBorrowable, який вимагає методи Borrow() і Return()
    public interface IBorrowable
    {
        void Borrow(); // Метод для позичання книги
        void Return(); // Метод для повернення книги
    }

    // Клас Book, що реалізує інтерфейс IBorrowable
    public class Book : IBorrowable
    {
        public string Title { get; set; } // Назва книги
        public string Author { get; set; } // Автор книги
        public string Genre { get; set; } // Жанр книги
        public int Year { get; set; } // Рік видання
        public string Id { get; set; } // Ідентифікатор книги
        public bool IsAvailable { get; set; } = true; // Статус наявності книги в бібліотеці (за замовчуванням доступна)

        // Конструктори класу
        public Book() { }
        public Book(string title, string author, string genre, int year, string id)
        {
            Title = title;
            Author = author;
            Genre = genre;
            Year = year;
            Id = id;
        }

        // Метод Borrow() реалізує позичання книги (змінюється на недоступну)
        public void Borrow()
        {
            IsAvailable = false;
        }

        // Метод Return() реалізує повернення книги (змінюється на доступну)
        public void Return()
        {
            IsAvailable = true;
        }

        // Перевизначення методу ToString для зручного виведення інформації про книгу
        public override string ToString()
        {
            return $"{Id}: {Title} by {Author} ({Year}) - {(IsAvailable ? "Available" : "Borrowed")}";
        }
    }

    // Клас EBook, що наслідує клас Book та додає властивість FileFormat
    public class EBook : Book
    {
        public string FileFormat { get; set; } // Формат електронної книги

        // Конструктор класу EBook
        public EBook(string title, string author, string genre, int year, string id, string fileFormat) : base(title, author, genre, year, id)
        {
            FileFormat = fileFormat;
        }
    }

    // Клас User для представлення користувача бібліотеки
    public class User
    {
        public string FirstName { get; set; } // Ім'я користувача
        public string LastName { get; set; } // Прізвище користувача
        public string UserId { get; set; } // Ідентифікатор користувача

        // Конструктори класу
        public User() { }
        public User(string firstName, string lastName, string userId)
        {
            FirstName = firstName;
            LastName = lastName;
            UserId = userId;
        }

        // Перевизначення методу ToString для зручного виведення інформації про користувача
        public override string ToString()
        {
            return $"{UserId}: {FirstName} {LastName}";
        }
    }

    // Клас Library для керування бібліотекою
    public class Library
    {
        private static Library _instance; // Одиничний екземпляр бібліотеки (патерн Singleton)
        private List<Book> books; // Список книг в бібліотеці
        private List<User> users; // Список користувачів бібліотеки
        private Dictionary<string, List<Book>> borrowedBooks; // Словник з позиченими книгами (за ID користувача)

        // Імена файлів для збереження даних
        private const string BooksFile = "books.xml";
        private const string UsersFile = "users.xml";
        private const string LogFile = "log.txt";

        // Приватний конструктор для ініціалізації бібліотеки
        private Library()
        {
            // Завантаження даних з файлів або створення нових списків
            books = LoadData<List<Book>>(BooksFile) ?? new List<Book>();
            users = LoadData<List<User>>(UsersFile) ?? new List<User>();
            borrowedBooks = new Dictionary<string, List<Book>>(); // Ініціалізація словника для позичених книг
        }

        // Публічний доступ до єдиного екземпляра бібліотеки
        public static Library Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Library();
                return _instance;
            }
        }

        // Метод для збереження даних у файл
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

        // Метод для завантаження даних з файлу
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

        // Метод для запису дій в лог-файл
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

        // Метод для додавання книги в бібліотеку
        public void AddBook(Book book)
        {
            // Перевірка на наявність книги з таким ID
            if (books.Any(b => b.Id == book.Id))
            {
                Console.WriteLine("Помилка: Книга з таким ID вже існує.");
                return;
            }
            books.Add(book); // Додаємо книгу до списку
            SaveData(BooksFile, books); // Зберігаємо зміни у файл
            LogAction($"Книга \"{book.Title}\" додана до бібліотеки."); // Записуємо в лог
            Console.WriteLine($"Книга \"{book.Title}\" додана до бібліотеки.");
        }

        // Метод для видалення книги за ID
        public void RemoveBook(string bookId)
        {
            // Знаходимо книгу по ID
            Book book = books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                books.Remove(book); // Видаляємо книгу зі списку
                SaveData(BooksFile, books); // Зберігаємо зміни у файл
                LogAction($"Книга \"{book.Title}\" була видалена."); // Записуємо в лог
                Console.WriteLine($"Книга \"{book.Title}\" видалена з бібліотеки.");
            }
            else
            {
                Console.WriteLine("Помилка: Книга не знайдена.");
            }
        }

        // Метод для відображення всіх книг в бібліотеці
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
                Console.WriteLine(book); // Виводимо інформацію про книгу
            }
        }

        // Метод для додавання нового користувача
        public void AddUser(User user)
        {
            if (users.Any(u => u.UserId == user.UserId))
            {
                Console.WriteLine("Помилка: Користувач з таким ID вже існує.");
                return;
            }
            users.Add(user); // Додаємо користувача до списку
            SaveData(UsersFile, users); // Зберігаємо зміни у файл
            LogAction($"Користувач {user.FirstName} {user.LastName} був зареєстрований.");
            Console.WriteLine($"Користувач {user.FirstName} {user.LastName} доданий.");
        }

        // Метод для видалення користувача за ID
        public void RemoveUser(string userId)
        {
            // Знаходимо користувача по ID
            User user = users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                users.Remove(user); // Видаляємо користувача зі списку
                LogAction($"Користувач {user.FirstName} {user.LastName} був видалений.");
                Console.WriteLine($"Користувач {user.FirstName} {user.LastName} видалений.");
            }
            else
            {
                Console.WriteLine("Помилка: Користувач не знайдений.");
            }
        }

        // Метод для відображення всіх користувачів
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
                Console.WriteLine(user); // Виводимо інформацію про користувача
            }
        }

        // Метод для позичання книги користувачу
        public void BorrowBook(string userId, string bookId)
        {
            User user = users.FirstOrDefault(u => u.UserId == userId); // Знаходимо користувача
            Book book = books.FirstOrDefault(b => b.Id == bookId); // Знаходимо книгу

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
                Console.WriteLine($"Книга \"{book.Title}\" вже взята.");
                return;
            }

            book.IsAvailable = false; // Змінюємо статус книги на недоступну

            if (!borrowedBooks.ContainsKey(userId))
            {
                borrowedBooks[userId] = new List<Book>(); // Ініціалізуємо список для користувача
            }
            borrowedBooks[userId].Add(book); // Додаємо книгу до списку позичених книг користувача
            LogAction($"Користувач {user.FirstName} {user.LastName} взяв книгу \"{book.Title}\".");
            Console.WriteLine($"Користувач {user.FirstName} {user.LastName} взяв книгу \"{book.Title}\".");
        }

        // Метод для повернення книги користувачем
        public void ReturnBook(string userId, string bookId)
        {
            if (!borrowedBooks.ContainsKey(userId) || borrowedBooks[userId].Count == 0)
            {
                Console.WriteLine("Помилка: Користувач не має взятих книг.");
                return;
            }

            // Знаходимо книгу в списку позичених книг користувача
            Book book = borrowedBooks[userId].FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                Console.WriteLine("Помилка: Книга не знайдена серед взятих.");
                return;
            }

            borrowedBooks[userId].Remove(book); // Видаляємо книгу з позичених книг
            book.IsAvailable = true; // Змінюємо статус книги на доступну
            LogAction($"Користувач повернув книгу \"{book.Title}\".");
            Console.WriteLine($"Користувач повернув книгу \"{book.Title}\".");
        }

        // Метод для відображення всіх книг, які позичив користувач
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
                Console.WriteLine(book); // Виводимо інформацію про позичені книги
            }
        }

        // Метод для оновлення інформації про книгу
        public void UpdateBook(string bookId, string newTitle, string newAuthor, string newGenre, int newYear)
        {
            var book = books.FirstOrDefault(b => b.Id == bookId); // Знаходимо книгу по ID
            if (book != null)
            {
                book.Title = newTitle;
                book.Author = newAuthor;
                book.Genre = newGenre;
                book.Year = newYear;
                SaveData(BooksFile, books); // Зберігаємо зміни у файл
                LogAction($"Оновлена інформація о книжці \"{book.Title}\".");
            }
            else
            {
                Console.WriteLine("Ошибка: Книга не знайдена.");
            }
        }

        // Метод для пошуку книг по ключовому слову (назва або автор)
        public void SearchBooks(string keyword)
        {
            bool found = false;
            Console.WriteLine("Результати поиска:");

            foreach (var book in books)
            {
                if (book.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    book.Author.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Console.WriteLine(book); // Виводимо знайдені книги
                    found = true;
                }
            }
            if (!found)
            {
                Console.WriteLine("Книг не найдено.");
            }
        }

        // Метод для фільтрації книг за жанром і роком видання
        public IEnumerable<Book> FilterBooks(string genre, int? year = null)
        {
            return books.Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase) &&
                                    (year == null || b.Year == year)); // Повертаємо відфільтровані книги
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
