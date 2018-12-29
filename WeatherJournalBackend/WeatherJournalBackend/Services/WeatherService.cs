using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherJournalBackend.Entities;
using WeatherJournalBackend.Data;

namespace WeatherJournalBackend.Services {
    public interface IWeatherService {
        Task<Coord> GetCoord(string weatherObjId);
        Task<List<Weather>> GetWeatherList(string weatherObjId);
        Task<Main> GetMain(string weatherObjId);
        Task<Wind> GetWind(string weatherObjId);
        Task<Clouds> GetClouds(string weatherObjId);
        Task<Sys> GetSys(string weatherObjId);
        Task<WeatherObject> GetWeatherObject(string weatherObjId);
        void AddWeatherObject(WeatherObject weatherObject, string weatherObjId);
        Task<bool> UpdateWeatherObject(WeatherObject newWeatherObject, string weatherObjId);
        Task<string> DeleteWeatherList(string weatherObjId);
        void DeleteObject<T>(T item) where T : class;
    }

    public class WeatherService: IWeatherService {
        private WeatherContext _context;

        public WeatherService(WeatherContext context) {
            _context = context;
        }

        // GET
        public async Task<Coord> GetCoord(string weatherObjId) {
            return await _context.Coords
                .FirstOrDefaultAsync(c => c.WeatherObjectId == weatherObjId);
        }

        // Returns null if no result
        public async Task<List<Weather>> GetWeatherList(string weatherObjId) {
            var result = await _context.Weathers
                .Where(w => w.WeatherObjectId == weatherObjId).ToListAsync();
            if (result.Any()) return result;
            return null;
        }

        public async Task<Main> GetMain(string weatherObjId) {
            return await _context.Mains
                .FirstOrDefaultAsync(m => m.WeatherObjectId == weatherObjId);
        }

        public async Task<Wind> GetWind(string weatherObjId) {
            return await _context.Winds
                .FirstOrDefaultAsync(w => w.WeatherObjectId == weatherObjId);
        }

        public async Task<Clouds> GetClouds(string weatherObjId) {
            return await _context.Clouds
                .FirstOrDefaultAsync(c => c.WeatherObjectId == weatherObjId);
        }

        public async Task<Sys> GetSys(string weatherObjId) {
            return await _context.Sys
                .FirstOrDefaultAsync(s => s.WeatherObjectId == weatherObjId);
        }

        public async Task<WeatherObject> GetWeatherObject(string weatherObjId) {
            return await _context.WeatherObjects
                .FirstOrDefaultAsync(wo => wo.WeatherObjectId == weatherObjId);
        }

        // ADD TO DATABASE
        public void AddWeatherObject(WeatherObject weatherObject, string weatherObjId) {
            weatherObject.WeatherObjectId = weatherObjId;
            weatherObject.Coord.WeatherObjectId = weatherObjId;
            weatherObject.Main.WeatherObjectId = weatherObjId;
            weatherObject.Wind.WeatherObjectId = weatherObjId;
            weatherObject.Clouds.WeatherObjectId = weatherObjId;
            weatherObject.Sys.WeatherObjectId = weatherObjId;

            _context.WeatherObjects.Add(weatherObject);
            _context.SaveChanges();
        }

        // UPDATE (currently only updates Main and Weather)
        public async Task<bool> UpdateWeatherObject(WeatherObject newWeatherObject, string weatherObjId) {
            var oldWeatherRows = await _context.Weathers
                .Where(w => w.WeatherObjectId == weatherObjId).ToListAsync();
            if (oldWeatherRows.Any()) {
                _context.Weathers.RemoveRange(oldWeatherRows);
            }
            _context.SaveChanges();

            var newWeatherRows = newWeatherObject.Weather;
            foreach(Weather weather in newWeatherRows) {
                weather.WeatherObjectId = weatherObjId;
                _context.Weathers.Add(weather);
            }
            _context.SaveChanges();

            var existingMain = await GetMain(weatherObjId);
            if (existingMain == null) return false;
            existingMain.Temp = newWeatherObject.Main.Temp;
            existingMain.Pressure = newWeatherObject.Main.Pressure;
            existingMain.Humidity = newWeatherObject.Main.Humidity;
            existingMain.Temp_min = newWeatherObject.Main.Temp_min;
            existingMain.Temp_max = newWeatherObject.Main.Temp_max;
            existingMain.Sea_level = newWeatherObject.Main.Sea_level;
            existingMain.Grnd_level = newWeatherObject.Main.Grnd_level;
            _context.SaveChanges();

            return true;
        }

        // DELETE
        public async Task<string> DeleteWeatherList(string weatherObjId) {
            var list = await _context.Weathers
                .Where(w => w.WeatherObjectId == weatherObjId).ToListAsync();
            if (list.Any()) {
                list.ForEach(elem => _context.Remove(elem));
                _context.SaveChanges();
                return "Success: Weather(s) deleted from database";
            } else {
                return "Failed: Weather not found in database";
            }
        }

        public void DeleteObject<T>(T item) where T : class {
            _context.Remove(item);
            _context.SaveChanges();
        }
    }
}
