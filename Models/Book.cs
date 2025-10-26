using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Models
{
    public class Book
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Назва книги є обов'язковою")]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        
        [Display(Name = "Посилання на обкладинку")]
        public string? CoverImageUrl { get; set; } 


     
        [Display(Name = "Жанр")]
        public int GenreId { get; set; }
       
        public Genre? Genre { get; set; }


    
        public ICollection<Author> Authors { get; set; } = new List<Author>();
    }
}