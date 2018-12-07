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
        private const string COORD = "coord";
        private const string WEATHER = "weather";
        private const string MAIN = "main";
        private const string WIND = "wind";
        private const string CLOUDS = "clouds";
        private const string SYS = "sys";
        private const string WEATHER_OBJECT = "weatherobject";
        private const string CITY_NAME = "cityname";
        private const string CITY_ID = "cityid";
        private readonly WeatherContext weatherContext;
        private readonly WeatherAPI weatherAPI;

        public ValuesController(WeatherContext weatherContext) {
            this.weatherContext = weatherContext;
            weatherAPI = new WeatherAPI();
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Instructionl() {
            return new string[] {
                "GET: 'objectType/weatherObjectId' gets an object",
                "POST: 'callTypeAPI/weatherObjectIdToAssign/param1/param2' adds the response object to database",
                "DELETE: 'objectType/weatherObjectId' deletes an object and its dependent tables",
                "objectType: coord, weather, main, wind, clouds, sys, weatherobject",
                "callTypeAPI: 'cityname', 'cityid', 'coord'",
            };
        }

        // GET api/values/weather/1
        [HttpGet("{objectType}/{weatherObjectId}")]
        public async Task<ActionResult<object>> GetObject(
            string objectType, string weatherObjectId, string weatherId
        ) {

            ActionResult<object> result = null;
            switch (objectType) {
                case COORD:
                    result = await weatherContext.GetCoord(weatherObjectId);
                    break;
                case WEATHER:
                    result = await weatherContext.GetWeatherList(weatherObjectId);
                    break;
                case MAIN:
                    result = await weatherContext.GetMain(weatherObjectId);
                    break;
                case WIND:
                    result = await weatherContext.GetWind(weatherObjectId);
                    break;
                case CLOUDS:
                    result = await weatherContext.GetClouds(weatherObjectId);
                    break;
                case SYS:
                    result = await weatherContext.GetSys(weatherObjectId);
                    break;
                case WEATHER_OBJECT:
                    result = await weatherContext.GetWeatherObject(weatherObjectId);
                    break;
                default:
                    return BadRequest("Failed: Check object type parameter");
            }

            if (result == null || result.Value == null) {
                return BadRequest("Failed: Id wasn't found in the database");
            }
            return Ok(result.Value);
        }

        // POST api/values/cityname/5/Toronto
        [HttpPost("{callTypeAPI}/{weatherObjIdToAssign}/{callParameter1}/{callParameter2?}")]
        public async Task<ActionResult<string>> Post(
            string callTypeAPI, string weatherObjIdToAssign,
            string callParameter1, string callParameter2 = ""
        ) {

            bool isNewObject = true;
            // Check if weather object id to be assigned already exists
            var wo = await weatherContext.GetWeatherObject(weatherObjIdToAssign);
            if (wo != null) {
                isNewObject = false;
            }

            string responseStr = null;
            switch (callTypeAPI) {
                case CITY_NAME:     // param1: city name; param2?: country code
                    if (callParameter2 != "") callParameter2 = "," + callParameter2;
                    string cityNameUrl = "weather?q=" + callParameter1 + callParameter2;
                    var responseName = weatherAPI.GetWeatherObjectFromAPI(cityNameUrl);
                    if (responseName != null) responseStr = responseName.Result;
                    break;
                case CITY_ID:       // param1: city id
                    if (callParameter2 == "") {
                        string cityIdUrl = "weather?id=" + callParameter1;
                        var responseId = weatherAPI.GetWeatherObjectFromAPI(cityIdUrl);
                        if (responseId != null) responseStr = responseId.Result;
                    }
                    break;
                case COORD:          // param1: lat; param2: lon
                    if (callParameter2 != "") {
                        string coordUrl = "weather?lat=" + callParameter1 + "&lon=" + callParameter2;
                        var responseCoord = weatherAPI.GetWeatherObjectFromAPI(coordUrl);
                        if (responseCoord != null) responseStr = responseCoord.Result;
                    }
                    break;
                default:
                    return BadRequest("Failed: check callTypeAPI parameter(s)");
            }

            if (responseStr == null) {
                return BadRequest("Failed: API call failed");
            }

            // Parse string into object and ADD to or UPDATE in database
            var newWeatherObj = weatherAPI.ParseWeatherDataObject(responseStr);
            if (isNewObject) {
                weatherContext.AddWeatherObjectToDatabase(newWeatherObj, weatherObjIdToAssign);
            } else {
                if (!(await weatherContext.UpdateWeatherObject(newWeatherObj, weatherObjIdToAssign))) {
                    return BadRequest("Object to update not found in database");
                }
            }
            return Ok("Success: object added to database " + responseStr);
        }

        //[HttpGet("test/{id}")]
        //public async Task<ActionResult<List<Weather>>> Test (string test, string id) {
        //    var jsonStr1 = "{\"coord\":{\"lon\":145.77,\"lat\":-16.92},\"weather\":[{\"id\":802,\"main\":\"Clouds\",\"description\":\"scattered clouds\",\"icon\":\"03n\"}],\"base\":\"stations\",\"main\":{\"temp\":300.15,\"pressure\":1007,\"humidity\":74,\"temp_min\":300.15,\"temp_max\":300.15},\"visibility\":10000,\"wind\":{\"speed\":3.6,\"deg\":160},\"clouds\":{\"all\":40},\"dt\":1485790200,\"sys\":{\"type\":1,\"id\":8166,\"message\":0.2064,\"country\":\"AU\",\"sunrise\":1485720272,\"sunset\":1485766550},\"id\":2172797,\"name\":\"Cairns\",\"cod\":200}";
        //    var jsonStr2 = "{\"coord\":{\"lon\":139.01,\"lat\":35.02},\"weather\":[{\"id\":800,\"main\":\"Clear\",\"description\":\"clear sky\",\"icon\":\"01n\"}, {\"id\":999,\"main\":\"Test weather\",\"description\":\"rainbow\",\"icon\":\"01n\"}],\"base\":\"stations\",\"main\":{\"temp\":285.514,\"pressure\":1013.75,\"humidity\":100,\"temp_min\":285.514,\"temp_max\":285.514,\"sea_level\":1023.22,\"grnd_level\":1013.75},\"wind\":{\"speed\":5.52,\"deg\":311},\"clouds\":{\"all\":0},\"dt\":1485792967,\"sys\":{\"message\":0.0025,\"country\":\"JP\",\"sunrise\":1485726240,\"sunset\":1485763863},\"id\":1907296,\"name\":\"Tawarano\",\"cod\":200}";

        //    var wo1 = weatherAPI.ParseWeatherDataObject(jsonStr1);
        //    var wo2 = weatherAPI.ParseWeatherDataObject(jsonStr2);

        //    weatherContext.AddWeatherObjectToDatabase(wo1, id);
        //    if (await weatherContext.UpdateWeatherObject(wo2, id)) {

        //        var result = await weatherContext.GetWeatherList(id);
        //        return Ok(result);
        //    }
        //    return BadRequest("Something went wrong");
        //}

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {
        }

        // DELETE api/values/coord/5
        [HttpDelete("{objectType}/{weatherObjectId}")]
        public async Task<ActionResult<string>> Delete(
            string objectType, string weatherObjectId
        ) {

            ActionResult<object> obj = null;
            switch (objectType) {
                case COORD:
                    obj = await weatherContext.GetCoord(weatherObjectId);
                    break;
                case WEATHER:
                    return await weatherContext.DeleteWeatherList(weatherObjectId);
                case MAIN:
                    obj = await weatherContext.GetMain(weatherObjectId);
                    break;
                case WIND:
                    obj = await weatherContext.GetWind(weatherObjectId);
                    break;
                case CLOUDS:
                    obj = await weatherContext.GetClouds(weatherObjectId);
                    break;
                case SYS:
                    obj = await weatherContext.GetSys(weatherObjectId);
                    break;
                case WEATHER_OBJECT:
                    obj = await weatherContext.GetWeatherObject(weatherObjectId);
                    break;
                default:
                    return BadRequest("Failed: Check object type parameter");
            }

            // when it's a single object
            if (obj == null || obj.Value == null) {
                return BadRequest("Failed: Id wasn't found in the database");
            } else {
                weatherContext.DeleteObject(obj.Value);
                return Ok("Success: Object deleted from database");
            }
        }
    }
}
