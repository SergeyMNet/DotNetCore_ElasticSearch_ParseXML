using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebAppCore.Models.BookViewModel;

namespace WebAppCore.Data
{
    //public class StorageContext : DbContext
    //{
    //    public DbSet<BookBase> Books { get; set; }


    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    {
    //        optionsBuilder.UseSqlite("Filename=./Books.db");

    //    }
    //}

    public class StorageContext : DbContext
    {
        public StorageContext(DbContextOptions<StorageContext> options) : base(options)
        {
        }

        public DbSet<BookBase> Books { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookBase>().ToTable("Books");
        }
    }

    public static class DbInitializer
    {
        public static void Initialize(StorageContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Books.Any())
            {
                return;   // DB has been seeded
            }

            //var books = new BookBase[]
            //{
            //new BookBase{Title= "Some book",Url= "link to book"},
            
            //};
            //foreach (BookBase s in books)
            //{
            //    context.Books.Add(s);
            //}
            //context.SaveChanges();

            
        }
    }
}