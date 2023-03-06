using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QueueRabbitMQ.Models;

namespace QueueRabbitMQ.Data
{
    public class DbContextClass : DbContext
    {

        public DbContextClass(DbContextOptions<DbContextClass> options) : base(options)
        {
        }

       

        public DbSet<Product> Products { get; set; }
    }
}
