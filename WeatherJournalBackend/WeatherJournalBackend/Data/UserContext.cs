using Microsoft.EntityFrameworkCore;
using WeatherJournalBackend.Entities;

namespace WeatherJournalBackend.Data {
    public class UserContext : DbContext {
        private static bool _created = false;
        public UserContext(DbContextOptions<UserContext> options) : base(options) {
            if (!_created) {
                _created = true;
                Database.EnsureDeleted();
                Database.EnsureCreated();
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>()
                        .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Username)
                        .IsUnique();

            modelBuilder.Entity<User>()
                        .HasOne(u => u.Settings)
                        .WithOne(s => s.User)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                        .HasMany(u => u.Journals)
                        .WithOne(j => j.User)
                        .HasForeignKey(j => j.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Journal>()
                        .HasKey(j => j.JournalId);

            modelBuilder.Entity<Settings>()
                        .HasKey(s => s.UserId);
        }
    }
}
