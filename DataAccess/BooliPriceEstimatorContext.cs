using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class BooliPriceEstimatorContext : DbContext
    {
        public DbSet<SoldObject> SoldHousingObjects { get; set; }

        public BooliPriceEstimatorContext(DbContextOptions options) : base(options)
        {

        }

        public BooliPriceEstimatorContext()
        {

        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlite("Data Source=booli.db");
        //}ef migrations add InitialCreate
    }
}
