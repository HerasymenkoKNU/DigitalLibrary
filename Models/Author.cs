using System.ComponentModel.DataAnnotations; 

namespace DigitalLibrary.Models
{
    public class Author
    {
        public int Id { get; set; } 
        [Required(ErrorMessage = "Ім'я автора є обов'язковим")]
        [StringLength(100)]
        public string Name { get; set; }

       
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}