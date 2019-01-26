using System.Collections.Generic;
using System.Linq;
using WeatherJournalBackend.Entities;

namespace WeatherJournalBackend.Data {
    public interface IWeatherRepository {
        void Add<T>(T entity) where T : class;

        Coord GetCoord(string weatherObjId);
        List<Weather> GetWeatherList(string weatherObjId);
        Main GetMain(string weatherObjId);
        Wind GetWind(string weatherObjId);
        Clouds GetClouds(string weatherObjId);
        Sys GetSys(string weatherObjId);
        WeatherObject GetWeatherObject(string weatherObjId);

        void Delete<T>(T entity) where T : class;
        void DeleteList<T>(List<T> entityList) where T : class;
    }

    public class WeatherRepository : IWeatherRepository {
        private WeatherContext _weatherContext;
          
        public WeatherRepository(WeatherContext weatherContext) {
            _weatherContext = weatherContext;
        }

        public void Add<T>(T entity) where T : class {
            _weatherContext.Add(entity);
        }

        public Coord GetCoord(string weatherObjId) {
            return _weatherContext.Coords
                .FirstOrDefault(c => c.WeatherObjectId == weatherObjId);
        }

        // Returns null if no result
        public List<Weather> GetWeatherList(string weatherObjId) {
            var result = _weatherContext.Weathers
                .Where(w => w.WeatherObjectId == weatherObjId).ToList();
            if (result.Any()) return result;
            return null;
        }

        public Main GetMain(string weatherObjId) {
            return _weatherContext.Mains
                .FirstOrDefault(m => m.WeatherObjectId == weatherObjId);
        }

        public Wind GetWind(string weatherObjId) {
            return _weatherContext.Winds
                .FirstOrDefault(w => w.WeatherObjectId == weatherObjId);
        }

        public Clouds GetClouds(string weatherObjId) {
            return _weatherContext.Clouds
                .FirstOrDefault(c => c.WeatherObjectId == weatherObjId);
        }

        public Sys GetSys(string weatherObjId) {
            return _weatherContext.Sys
                .FirstOrDefault(s => s.WeatherObjectId == weatherObjId);
        }

        public WeatherObject GetWeatherObject(string weatherObjId) {
            return _weatherContext.WeatherObjects
                .FirstOrDefault(wo => wo.WeatherObjectId == weatherObjId);
        }

        public void Delete<T>(T entity) where T : class {
            _weatherContext.Remove(entity);
        }

        public void DeleteList<T>(List<T> entityList) where T : class {
            _weatherContext.RemoveRange(entityList);
        }
    }
}
