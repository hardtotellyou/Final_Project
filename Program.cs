using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace Final_Project
{
    internal class Program
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
            static void Main(string[] args)
        {
           
        }
    }
}
