using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherJournalEntry.Data;
using WeatherJournalEntry.Model;

namespace WeatherJournalEntry.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase {
        private const string ADD_WEATHER_OBJECT_TO_DB = "add";
        private readonly WeatherContext weatherContext;
        private readonly WeatherAPI weatherAPI;

        public ValuesController(WeatherContext weatherContext) {
            this.weatherContext = weatherContext;
            weatherAPI = new WeatherAPI();
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> GetAll() {
            return new string[] { "value1", "value2", "testvalue" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id) {
            return "values";
        }

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody] string value) {
        //}

        // POST api/values/x/y
        [HttpPost("{add}/{strID}")]
        public string Post(string add, string strID) {
            switch (add) {
                case ADD_WEATHER_OBJECT_TO_DB:
                    var jsonStr1 = "{\"coord\":{\"lon\":145.77,\"lat\":-16.92},\"weather\":[{\"id\":802,\"main\":\"Clouds\",\"description\":\"scattered clouds\",\"icon\":\"03n\"}],\"base\":\"stations\",\"main\":{\"temp\":300.15,\"pressure\":1007,\"humidity\":74,\"temp_min\":300.15,\"temp_max\":300.15},\"visibility\":10000,\"wind\":{\"speed\":3.6,\"deg\":160},\"clouds\":{\"all\":40},\"dt\":1485790200,\"sys\":{\"type\":1,\"id\":8166,\"message\":0.2064,\"country\":\"AU\",\"sunrise\":1485720272,\"sunset\":1485766550},\"id\":2172797,\"name\":\"Cairns\",\"cod\":200}";
                    var jsonStr2 = "{\"coord\":{\"lon\":139.01,\"lat\":35.02},\"weather\":[{\"id\":800,\"main\":\"Clear\",\"description\":\"clear sky\",\"icon\":\"01n\"}],\"base\":\"stations\",\"main\":{\"temp\":285.514,\"pressure\":1013.75,\"humidity\":100,\"temp_min\":285.514,\"temp_max\":285.514,\"sea_level\":1023.22,\"grnd_level\":1013.75},\"wind\":{\"speed\":5.52,\"deg\":311},\"clouds\":{\"all\":0},\"dt\":1485792967,\"sys\":{\"message\":0.0025,\"country\":\"JP\",\"sunrise\":1485726240,\"sunset\":1485763863},\"id\":1907296,\"name\":\"Tawarano\",\"cod\":200}";
                    var jsonStr3 = "{\"coord\":{\"lon\":-122.09,\"lat\":37.39},\"weather\":[{\"id\":500,\"main\":\"Rain\",\"description\":\"light rain\",\"icon\":\"10d\"}],\"base\":\"stations\",\"main\":{\"temp\":280.44,\"pressure\":1017,\"humidity\":61,\"temp_min\":279.15,\"temp_max\":281.15},\"visibility\":12874,\"wind\":{\"speed\":8.2,\"deg\":340,\"gust\":11.3},\"clouds\":{\"all\":1},\"dt\":1519061700,\"sys\":{\"type\":1,\"id\":392,\"message\":0.0027,\"country\":\"US\",\"sunrise\":1519051894,\"sunset\":1519091585},\"id\":0,\"name\":\"Mountain View\",\"cod\":200}";
                    var weatherDataObj = weatherAPI.ParseWeatherDataObject(jsonStr1);

                    weatherContext.Database.EnsureCreatedAsync();

                    var intID = 0;
                    if (Int32.TryParse(strID, out intID)) {
                        weatherDataObj.WeatherObjectId = intID;
                        weatherContext.WeatherObjects.Add(weatherDataObj);

                        weatherContext.SaveChanges();
                        return "Success: object added to database";
                    }

                    return "Failed: id cannot be converted to int";
                default:
                    return "Check object type parameter";
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id) {
        }
    }
}
