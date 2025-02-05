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
        public static Library Instance => _instance ??= new Library();

        public void AddBook(Book book)
        {
            if (books.Any(b => b.Id == book.Id))
            {
                Console.WriteLine("Книга с таким ID уже существует!");
                return;
            }
            books.Add(book);
            SaveData(books, BooksFile);
            LogOperation($"Добавлена книга: {book}");
        }
    }
        internal class Program
    {
            static void Main(string[] args)
            {
           
            }
    }
}
