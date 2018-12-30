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
    }
}
