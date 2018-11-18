using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                        .HasKey(w => w.WeatherId);

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

        // GET
        public async Task<Coord> GetCoord(int weatherObjId) {
            return await Coords
                .FirstOrDefaultAsync(c => c.WeatherObjectId == weatherObjId);
        }

        public async Task<List<Weather>> GetWeatherList(int weatherObjId) {
            var result = await Weathers
                .Where(w => w.WeatherObjectId == weatherObjId).ToListAsync();
            if (result.Any()) return result;
            return null;
        }

        public async Task<Main> GetMain(int weatherObjId) {
            return await Mains
                .FirstOrDefaultAsync(m => m.WeatherObjectId == weatherObjId);
        }

        public async Task<Wind> GetWind(int weatherObjId) {
            return await Winds
                .FirstOrDefaultAsync(w => w.WeatherObjectId == weatherObjId);
        }

        public async Task<Clouds> GetClouds(int weatherObjId) {
            return await Clouds
                .FirstOrDefaultAsync(c => c.WeatherObjectId == weatherObjId);
        }

        public async Task<Sys> GetSys(int weatherObjId) {
            return await Sys
                .FirstOrDefaultAsync(s => s.WeatherObjectId == weatherObjId);
        }

        public async Task<WeatherObject> GetWeatherObject(int weatherObjId) {
            return await WeatherObjects
                .FirstOrDefaultAsync(wo => wo.WeatherObjectId == weatherObjId);
        }

        // ADD TO DATABASE
        public async void AddWeatherObjectToDatabase(WeatherObject weatherObject, int weatherObjId) {
            await Database.EnsureCreatedAsync();
            weatherObject.WeatherObjectId = weatherObjId;
            WeatherObjects.Add(weatherObject);
            SaveChanges();
        }

        // DELETE
        public async Task<string> DeleteWeatherList(int weatherObjId) {
            var list = await Weathers.Where(w => w.WeatherObjectId == weatherObjId).ToListAsync();
            if (list.Any()) {
                list.ForEach(elem => Remove(elem));
                SaveChanges();
                return "Success: Weather(s) deleted from database";
            } else {
                return "Failed: Weather not found in database";
            }
        }

        public void DeleteObject<T>(T item) where T : class {
            Remove(item);
            SaveChanges();
        }
    }
}
