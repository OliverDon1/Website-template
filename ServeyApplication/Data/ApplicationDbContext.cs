using Microsoft.EntityFrameworkCore;
using ServeyApplication.Models;

namespace ServeyApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<EmailConfirmationToken> EmailConfirmationTokens { get; set; }

    }
}