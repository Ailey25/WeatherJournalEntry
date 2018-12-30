using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using WeatherJournalBackend.Entities;

namespace WeatherJournalBackend.Helpers {
    public class WeatherAPI {
        private const string API_URL = "http://api.openweathermap.org/data/2.5/";
        private const string API_PARAM_KEY = "&APPID=b944ebba123deeb1af548d804245dac4";

        public WeatherAPI() {}

        // Performs API call using url
        // Return JSON string
        public async Task<string> GetWeatherObjectFromAPI(string url) {
            // URL
            string urlParam = url + API_PARAM_KEY;

            // Set up
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(API_URL);

            // Response
            HttpResponseMessage response = await client.GetAsync(urlParam);
            if (response.IsSuccessStatusCode) {
                // Parse response
                var dataObjectString = response.Content.ReadAsStringAsync().Result;
                return dataObjectString;
            }

            return null;
        }

        // Parse data string into object
        // Return object
        public WeatherObject ParseWeatherDataObject(string jsonStr) {
            var weatherObject = JsonConvert.DeserializeObject<WeatherObject>(jsonStr);
            return weatherObject;
        }
    }
}
