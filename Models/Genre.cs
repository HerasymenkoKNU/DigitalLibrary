using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Models
{
    public class Genre
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Назва жанру є обов'язковою")]
        [StringLength(50)]
        public string Name { get; set; }

      
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}