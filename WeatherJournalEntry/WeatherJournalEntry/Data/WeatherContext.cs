using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WeatherJournalEntry.Model;

namespace WeatherJournalEntry.Data {
    public class WeatherContext: DbContext {
        public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) {}

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
                        .HasOne(w => w.WeatherObject)
                        .WithMany(wo => wo.Weather)
                        .HasForeignKey(w => w.WeatherObjectId);

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
        }
    }
}
