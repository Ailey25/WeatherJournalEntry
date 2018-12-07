using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherJournalEntry.Model;

namespace WeatherJournalEntry.Data {
    public class WeatherContext: DbContext {
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
        public async Task<Coord> GetCoord(string weatherObjId) {
            return await Coords
                .FirstOrDefaultAsync(c => c.WeatherObjectId == weatherObjId);
        }

        // Returns null if no result
        public async Task<List<Weather>> GetWeatherList(string weatherObjId) {
            var result = await Weathers
                .Where(w => w.WeatherObjectId == weatherObjId).ToListAsync();
            if (result.Any()) return result;
            return null;
        }

        public async Task<Main> GetMain(string weatherObjId) {
            return await Mains
                .FirstOrDefaultAsync(m => m.WeatherObjectId == weatherObjId);
        }

        public async Task<Wind> GetWind(string weatherObjId) {
            return await Winds
                .FirstOrDefaultAsync(w => w.WeatherObjectId == weatherObjId);
        }

        public async Task<Clouds> GetClouds(string weatherObjId) {
            return await Clouds
                .FirstOrDefaultAsync(c => c.WeatherObjectId == weatherObjId);
        }

        public async Task<Sys> GetSys(string weatherObjId) {
            return await Sys
                .FirstOrDefaultAsync(s => s.WeatherObjectId == weatherObjId);
        }

        public async Task<WeatherObject> GetWeatherObject(string weatherObjId) {
            return await WeatherObjects
                .FirstOrDefaultAsync(wo => wo.WeatherObjectId == weatherObjId);
        }

        // ADD TO DATABASE
        public void AddWeatherObjectToDatabase(WeatherObject weatherObject, string weatherObjId) {
            Database.EnsureCreated();
            weatherObject.WeatherObjectId = weatherObjId;
            WeatherObjects.Add(weatherObject);
            SaveChanges();
        }

        // UPDATE (currently only updates Main and Weather)
        public async Task<bool> UpdateWeatherObject(WeatherObject newWeatherObject, string weatherObjId) {
            //var existingCoord = await GetCoord(weatherObjId);

            var oldWeatherRows = await Weathers
                .Where(w => w.WeatherObjectId == weatherObjId).ToListAsync();
            if (oldWeatherRows.Any()) {
                Weathers.RemoveRange(oldWeatherRows);
            }
            SaveChanges();
            var newWeatherRows = newWeatherObject.Weather;
            foreach(Weather weather in newWeatherRows) {
                weather.WeatherObjectId = weatherObjId;
                Weathers.Add(weather);
            }
            SaveChanges();

            var existingMain = await GetMain(weatherObjId);
            if (existingMain == null) return false;
            existingMain.Temp = newWeatherObject.Main.Temp;
            existingMain.Pressure = newWeatherObject.Main.Pressure;
            existingMain.Humidity = newWeatherObject.Main.Humidity;
            existingMain.Temp_min = newWeatherObject.Main.Temp_min;
            existingMain.Temp_max = newWeatherObject.Main.Temp_max;
            existingMain.Sea_level = newWeatherObject.Main.Sea_level;
            existingMain.Grnd_level = newWeatherObject.Main.Grnd_level;
            SaveChanges();

            //var existingWind = await GetWind(weatherObjId);
            //var existingClouds = await GetClouds(weatherObjId);
            //var existingSys = await GetSys(weatherObjId);
            //var existingWeatherObject = await GetWeatherList(weatherObjId);

            return true;
        }

        // DELETE
        public async Task<string> DeleteWeatherList(string weatherObjId) {
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
