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
                "POST: 'objectType/weatherObjectId' adds the object to database",
                "DELETE: 'objectType/weatherObjectId' deletes an object and its dependent tables",
                "objectType: coord, weather, main, wind, clouds, sys, weatherobject",
                "callTypeAPI: 'cityname', 'cityid', 'coord'",
            };
        }

        // GET api/values/weather/1
        [HttpGet("{objectType}/{weatherObjectId}")]
        public async Task<ActionResult<object>> GetRowInTable(
            string objectType, string weatherObjectId, string weatherId
        ) {
            int intWeatherObjId = 0;
            if (!(Int32.TryParse(weatherObjectId, out intWeatherObjId))) {
                return BadRequest("Failed: id cannot be converted to int");
            }

            ActionResult<object> result = null;
            switch (objectType) {
                case COORD:
                    result = await weatherContext.GetCoord(intWeatherObjId);
                    break;
                case WEATHER:
                    result = await weatherContext.GetWeatherList(intWeatherObjId);
                    break;
                case MAIN:
                    result = await weatherContext.GetMain(intWeatherObjId);
                    break;
                case WIND:
                    result = await weatherContext.GetWind(intWeatherObjId);
                    break;
                case CLOUDS:
                    result = await weatherContext.GetClouds(intWeatherObjId);
                    break;
                case SYS:
                    result = await weatherContext.GetSys(intWeatherObjId);
                    break;
                case WEATHER_OBJECT:
                    result = await weatherContext.GetWeatherObject(intWeatherObjId);
                    break;
                default:
                    return BadRequest("Failed: Check object type parameter");
            }

            if (result == null || result.Value == null) {
                return BadRequest("Failed: Id wasn't found in the database");
            }
            return Ok(result.Value);
        }

        // POST api/values/coord/5/123/4566?
        [HttpPost("{callTypeAPI}/{weatherObjIdToAssign}/{callParameter1}/{callParameter2?}")]
        public async Task<ActionResult<string>> Post(
            string callTypeAPI, string weatherObjIdToAssign,
            string callParameter1, string callParameter2 = ""
        ) {
            var intWeatherObjId = 0;
            if (!(Int32.TryParse(weatherObjIdToAssign, out intWeatherObjId))) {
                return BadRequest("Failed: given id cannot be converted to int");
            }

            // Check if weather object id to be assigned already exists
            var wo = await weatherContext.GetWeatherObject(intWeatherObjId);
            if (wo != null) {
                return BadRequest("Failed: weather object " + weatherObjIdToAssign + " already exists");
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

            // Parse string into object and add to database
            var weatherObj = weatherAPI.ParseWeatherDataObject(responseStr);
            weatherContext.AddWeatherObjectToDatabase(weatherObj, intWeatherObjId);
            return Ok("Success: object added to database " + responseStr);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {
        }

        // DELETE api/values/coord/5
        [HttpDelete("{objectType}/{weatherObjectId}")]
        public async Task<ActionResult<string>> Delete(
            string objectType, string weatherObjectId
        ) {
            var intWeatherObjId = 0;
            if (!(Int32.TryParse(weatherObjectId, out intWeatherObjId))) {
                return BadRequest("Failed: id cannot be converted to int");
            }

            ActionResult<object> obj = null;
            switch (objectType) {
                case COORD:
                    obj = await weatherContext.GetCoord(intWeatherObjId);
                    break;
                case WEATHER:
                    return await weatherContext.DeleteWeatherList(intWeatherObjId);
                case MAIN:
                    obj = await weatherContext.GetMain(intWeatherObjId);
                    break;
                case WIND:
                    obj = await weatherContext.GetWind(intWeatherObjId);
                    break;
                case CLOUDS:
                    obj = await weatherContext.GetClouds(intWeatherObjId);
                    break;
                case SYS:
                    obj = await weatherContext.GetSys(intWeatherObjId);
                    break;
                case WEATHER_OBJECT:
                    obj = await weatherContext.GetWeatherObject(intWeatherObjId);
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
