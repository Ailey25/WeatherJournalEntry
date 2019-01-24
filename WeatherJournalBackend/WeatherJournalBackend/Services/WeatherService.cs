using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherJournalBackend.Entities;
using WeatherJournalBackend.UoW;

namespace WeatherJournalBackend.Services {
    public interface IWeatherService {
        object GetObject(string objectType, string weatherObjectId);
        List<Weather> GetWeatherList(string weatherObjectId);
        void AddWeatherObject(WeatherObject weatherObject, string weatherObjectId);
        bool UpdateWeatherObject(WeatherObject newWeatherObject, string weatherObjectId);
        bool DeleteWeatherObject(string weatherObjectId);
    }

    public class WeatherService: IWeatherService {
        private const string COORD = "coord";
        private const string WEATHER = "weather";
        private const string MAIN = "main";
        private const string WIND = "wind";
        private const string CLOUDS = "clouds";
        private const string SYS = "sys";
        private const string WEATHER_OBJECT = "weatherobject";

        private IUnitOfWork _unitOfWork;

        public WeatherService(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        public object GetObject(string objectType, string weatherObjectId) {
            object result = null;
            switch (objectType) {
                case COORD:
                    result = _unitOfWork.WeatherRepository.GetCoord(weatherObjectId);
                    break;
                case MAIN:
                    result = _unitOfWork.WeatherRepository.GetMain(weatherObjectId);
                    break;
                case WIND:
                    result = _unitOfWork.WeatherRepository.GetWind(weatherObjectId);
                    break;
                case CLOUDS:
                    result = _unitOfWork.WeatherRepository.GetClouds(weatherObjectId);
                    break;
                case SYS:
                    result = _unitOfWork.WeatherRepository.GetSys(weatherObjectId);
                    break;
                case WEATHER_OBJECT:
                    result = _unitOfWork.WeatherRepository.GetWeatherObject(weatherObjectId);
                    break;
                default:
                    return null;
            }

            return result;
        }

        public List<Weather> GetWeatherList(string weatherObjectId) {
            return _unitOfWork.WeatherRepository.GetWeatherList(weatherObjectId);
        }

        public void AddWeatherObject(WeatherObject weatherObject, string weatherObjectId) {
            weatherObject.WeatherObjectId = weatherObjectId;
            _unitOfWork.WeatherRepository.Add(weatherObject);

            _unitOfWork.SaveWeatherChanges();
        }

        // UPDATE (currently only updates Main and Weather)
        public bool UpdateWeatherObject(WeatherObject newWeatherObject, string weatherObjectId) {
            var oldWeatherRows = _unitOfWork.WeatherRepository.GetWeatherList(weatherObjectId);
            if (oldWeatherRows.Any()) {
                _unitOfWork.WeatherRepository.DeleteList(oldWeatherRows);
            }
            _unitOfWork.SaveWeatherChanges();

            var newWeatherRows = newWeatherObject.Weather;
            foreach(Weather weather in newWeatherRows) {
                weather.WeatherObjectId = weatherObjectId;
                _unitOfWork.WeatherRepository.Add(weather);
            }
            _unitOfWork.SaveWeatherChanges();

            var existingMain = _unitOfWork.WeatherRepository.GetMain(weatherObjectId);
            if (existingMain == null) return false;
            existingMain.Temp = newWeatherObject.Main.Temp;
            existingMain.Pressure = newWeatherObject.Main.Pressure;
            existingMain.Humidity = newWeatherObject.Main.Humidity;
            existingMain.Temp_min = newWeatherObject.Main.Temp_min;
            existingMain.Temp_max = newWeatherObject.Main.Temp_max;
            existingMain.Sea_level = newWeatherObject.Main.Sea_level;
            existingMain.Grnd_level = newWeatherObject.Main.Grnd_level;

            _unitOfWork.SaveWeatherChanges();
            return true;
        }

        public bool DeleteWeatherObject(string weatherObjectId) {
            var weatherObject = _unitOfWork.WeatherRepository.GetWeatherObject(weatherObjectId);

            if (weatherObject == null) return false;

            _unitOfWork.WeatherRepository.Delete(weatherObject);

            _unitOfWork.SaveWeatherChanges();
            return true;
        }

    }
}
