
using DigitalLibrary.Models; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 


using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Data
{
    
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
  
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

     
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get;  set; }
        public DbSet<Genre> Genres { get; set; }
    }
}