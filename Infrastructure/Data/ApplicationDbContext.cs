using ApplicationCore.Chat.Domain;
using ApplicationCore.Interfaces;
using ApplicationCore.Users.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>, IAppDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> optionsBuilder)
            : base(optionsBuilder)
        {

        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Message>()
                .Property(m => m.MessageId)
                .UseSqlServerIdentityColumn();
        }
    }
}
