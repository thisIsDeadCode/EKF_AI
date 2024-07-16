using EKF_AI.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace EKF_AI.DataBase
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Image> Images { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EKF_AI;Trusted_Connection=True;");
        }
    }
}
