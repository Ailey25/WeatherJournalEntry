using Microsoft.EntityFrameworkCore;
using WeatherJournalBackend.Entities;

namespace WeatherJournalBackend.Data {
    public class WeatherContext : DbContext {
        private static bool _created = false;
        public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) {
            if (!_created) {
                _created = true;
                Database.EnsureDeleted();
                Database.EnsureCreated();
            }
        }

        public DbSet<Coord> Coords { get; set; }
        public DbSet<Weather> Weathers { get; set; }
        public DbSet<Main> Mains { get; set; }
        public DbSet<Wind> Winds { get; set; }
        public DbSet<Clouds> Clouds { get; set; }
        public DbSet<Sys> Sys { get; set; }
        public DbSet<WeatherObject> WeatherObjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Coord>()
                        .HasKey(c => c.WeatherObjectId);

            modelBuilder.Entity<Weather>()
                        .HasKey(w => w.WeatherId);

            modelBuilder.Entity<Main>()
                        .HasKey(m => m.WeatherObjectId);

            modelBuilder.Entity<Wind>()
                        .HasKey(w => w.WeatherObjectId);

            modelBuilder.Entity<Clouds>()
                        .HasKey(w => w.WeatherObjectId);

            modelBuilder.Entity<Sys>()
                        .HasKey(s => s.WeatherObjectId);

            modelBuilder.Entity<WeatherObject>()
                        .HasKey(wo => wo.WeatherObjectId);
            modelBuilder.Entity<WeatherObject>()
                        .HasOne(wo => wo.Coord);
            modelBuilder.Entity<WeatherObject>()
                        .HasMany(wo => wo.Weather);
            modelBuilder.Entity<WeatherObject>()
                        .HasOne(wo => wo.Main);
            modelBuilder.Entity<WeatherObject>()
                        .HasOne(wo => wo.Wind);
            modelBuilder.Entity<WeatherObject>()
                        .HasOne(wo => wo.Clouds);
            modelBuilder.Entity<WeatherObject>()
                        .HasOne(wo => wo.Sys);
        }
    }
}
