using Messenger.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Messenger.EF
{
    public class MessengerDbContext : IdentityDbContext<MessengerUser>
    {
        public DbSet<Dialog> Dialogs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Participant> Participants { get; set; }

        public MessengerDbContext(DbContextOptions<MessengerDbContext> options)
            : base(options)

        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Participant>().HasKey("DialogId", "MessengerUserId");
            
            base.OnModelCreating(builder);
        }
    }
}
