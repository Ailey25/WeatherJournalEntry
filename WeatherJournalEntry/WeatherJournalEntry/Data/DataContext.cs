using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WeatherJournalEntry.Model;

namespace WeatherJournalEntry.Data {
    public class DataContext: DbContext {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        //public DbSet<Coord> Coords { get; set; }
        public DbSet<Weather> Weathers { get; set; }
        //public DbSet<Main> Mains { get; set; }
        //public DbSet<Wind> Winds { get; set; }
        //public DbSet<Clouds> Clouds { get; set; }
        //public DbSet<Sys> Sys { get; set; }
        //public DbSet<WeatherObject> WeatherObjects { get; set; }
    }
}
