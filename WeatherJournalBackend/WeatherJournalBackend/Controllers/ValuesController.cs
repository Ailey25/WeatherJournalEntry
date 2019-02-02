using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WeatherJournalBackend.Dtos;
using WeatherJournalBackend.Entities;
using WeatherJournalBackend.Services;
using WeatherJournalBackend.Helpers;
using Newtonsoft.Json;
using AutoMapper;

namespace WeatherJournalBackend.Controllers {
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecificOrigin")]
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
        private readonly IWeatherService _weatherService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IConfiguration Configuration;
        private readonly WeatherAPI weatherAPI;

        public ValuesController(
            IWeatherService weatherService, IMapper mapper,
            IUserService userService, IConfiguration configuration
        ) {
            _weatherService = weatherService;
            _userService = userService;
            _mapper = mapper;
            Configuration = configuration;
            weatherAPI = new WeatherAPI();
        }

        // GET api/values
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<string>> Instructionl() {
            return new string[] {
                "This is just a backend. Go to https://weatherjournalapp.azurewebsites.net"
            };
            //return new string[] {
            //    "GET: 'objectType/weatherObjectId' gets an object",
            //    "POST: 'callTypeAPI/weatherObjectIdToAssign/param1/param2' adds the response object to database",
            //    "DELETE: 'objectType/weatherObjectId' deletes an object and its dependent tables",
            //    "objectType: coord, weather, main, wind, clouds, sys, weatherobject",
            //    "callTypeAPI: 'cityname', 'cityid', 'coord'",
            //};
        }

        // GET api/values/weather/1
        [HttpGet("{objectType}/{weatherObjectId}")]
        public ActionResult<object> GetObject(string objectType, string weatherObjectId) {
            if (objectType == WEATHER) {
                List<Weather> weatherList = _weatherService.GetWeatherList(weatherObjectId);

                if (weatherList == null) return Ok(JsonConvert.SerializeObject(new List<WeatherDto>()));
                return Ok(JsonConvert.SerializeObject(weatherList));
            } else {
                object obj = _weatherService.GetObject(objectType, weatherObjectId);

                if (obj == null) return BadRequest(new { message = "Object wasn't found in the database" });
                return Ok(JsonConvert.SerializeObject(obj));
            }
        }

        // POST api/values/cityname/5/Toronto
        [HttpPost("{callTypeAPI}/{weatherObjIdToAssign}/{callParameter1}/{callParameter2?}")]
        public ActionResult<string> Post(
            string callTypeAPI, string weatherObjIdToAssign,
            string callParameter1, string callParameter2 = ""
        ) {
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
                    return BadRequest(new { message = "Check callTypeAPI parameter(s)" });
            }

            if (responseStr == null) return BadRequest(new { message = "API call failed" });

            var newWeatherObj = weatherAPI.ParseWeatherDataObject(responseStr);
            var wo = _weatherService.GetObject(WEATHER_OBJECT, weatherObjIdToAssign);
            if (wo != null) { // weather object already exists, update
                if (!_weatherService.UpdateWeatherObject(newWeatherObj, weatherObjIdToAssign)) {
                    return BadRequest(new { message = "Object to update not found in database" });
                }
            } else { // new weather object
                _weatherService.AddWeatherObject(newWeatherObj, weatherObjIdToAssign);
            }

            return Ok(responseStr);
        }

        // DELETE api/values/weatherobject/5
        [HttpDelete("weatherobject/{weatherObjectId}")]
        public ActionResult<string> DeleteWeatherObject(string weatherObjectId) {
            var isDeleted = _weatherService.DeleteWeatherObject(weatherObjectId);
            if (!isDeleted) return BadRequest(new { message = "Id wasn't found in database" });
            return Ok(new { message = "Weather object deleted" });
        }

        //[HttpGet("test/{id}")]
        //public async Task<ActionResult<List<Weather>>> Test (string test, string id) {
        //    var jsonStr1 = "{\"coord\":{\"lon\":145.77,\"lat\":-16.92},\"weather\":[{\"id\":802,\"main\":\"Clouds\",\"description\":\"scattered clouds\",\"icon\":\"03n\"}],\"base\":\"stations\",\"main\":{\"temp\":300.15,\"pressure\":1007,\"humidity\":74,\"temp_min\":300.15,\"temp_max\":300.15},\"visibility\":10000,\"wind\":{\"speed\":3.6,\"deg\":160},\"clouds\":{\"all\":40},\"dt\":1485790200,\"sys\":{\"type\":1,\"id\":8166,\"message\":0.2064,\"country\":\"AU\",\"sunrise\":1485720272,\"sunset\":1485766550},\"id\":2172797,\"name\":\"Cairns\",\"cod\":200}";
        //    var jsonStr2 = "{\"coord\":{\"lon\":139.01,\"lat\":35.02},\"weather\":[{\"id\":800,\"main\":\"Clear\",\"description\":\"clear sky\",\"icon\":\"01n\"}, {\"id\":999,\"main\":\"Test weather\",\"description\":\"rainbow\",\"icon\":\"01n\"}],\"base\":\"stations\",\"main\":{\"temp\":285.514,\"pressure\":1013.75,\"humidity\":100,\"temp_min\":285.514,\"temp_max\":285.514,\"sea_level\":1023.22,\"grnd_level\":1013.75},\"wind\":{\"speed\":5.52,\"deg\":311},\"clouds\":{\"all\":0},\"dt\":1485792967,\"sys\":{\"message\":0.0025,\"country\":\"JP\",\"sunrise\":1485726240,\"sunset\":1485763863},\"id\":1907296,\"name\":\"Tawarano\",\"cod\":200}";

        //    var wo1 = weatherAPI.ParseWeatherDataObject(jsonStr1);
        //    var wo2 = weatherAPI.ParseWeatherDataObject(jsonStr2);

        //    //// test update
        //    //_weatherService.AddWeatherObject(wo1, id);
        //    //if (await _weatherService.UpdateWeatherObject(wo2, id)) {

        //    //    var result = await _weatherService.GetWeatherList(id);
        //    //    return Ok(result);
        //    //}
        //    //return BadRequest("Something went wrong");

        //    //// test get weather object
        //    //var result = await _weatherService.GetWeatherObject(id);
        //    //return Ok(result);
        //}

        [AllowAnonymous]
        [HttpPost("user/register")]
        public ActionResult<string> Register([FromBody]UserDto userDto) {
            var user = _mapper.Map<User>(userDto);
            var userResult = _userService.Create(user, userDto.Password);

            if (userResult != "") return BadRequest(new { message = userResult });

            _userService.SetSettings(user.Id);
            return Ok(new { message = "User registered" });
        }

        [AllowAnonymous]
        [HttpPost("user/authenticate")]
        public IActionResult Authenticate([FromBody]UserDto userDto) {
            var userObject = _userService.Authenticate(userDto.Username, userDto.Password);
            if (userObject == null) return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Secret").Value);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, userObject.Id)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            var user = new {
                Id = userObject.Id,
                Username = userObject.Username,
                FirstName = userObject.FirstName,
                LastName = userObject.LastName,
                Token = tokenString
            };

            var userStr = JsonConvert.SerializeObject(user);
            return Ok(userStr);
        }

        [HttpGet("user/{id}")]
        public ActionResult<UserDto> GetUser(string id) {
            var user = _userService.GetUser(id);

            if (user == null) return BadRequest(new { message = "User not found" });

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(JsonConvert.SerializeObject(userDto));
        }

        [HttpPost("user/profile/{userId}")]
        public ActionResult<string> UpdateProfile(string userId, [FromBody]UserDto userDto) {
            var user = _mapper.Map<User>(userDto);
            user.Id = userId;
            var result = _userService.UpdateFirstLastName(user);

            if (result != "") return BadRequest(new { message = result });

            return Ok(new { message = "User profile updated" });
        }

        [HttpPost("user/username/{userId}")]
        public ActionResult<string> UpdateUsername(string userId, [FromBody]UserDto userDto) {
            var user = _mapper.Map<User>(userDto);
            user.Id = userId;
            var result = _userService.UpdateUsername(user);

            if (result != "") return BadRequest(new { message = result });

            return Ok(new { message = "Username updated" });
        }

        [HttpPost("user/password/{userId}/{oldPassword}")]
        public ActionResult<string> UpdatePassword(
            string userId,
            string oldPassword,
            [FromBody]UserDto userDto
        ){
            var user = _mapper.Map<User>(userDto);
            user.Id = userId;
            var result = _userService.UpdatePassword(user, oldPassword, userDto.Password);

            if (result != "") return BadRequest(new { message = result });

            return Ok(new { message = "Password updated" });
        }

        [HttpDelete("user/{id}")]
        public ActionResult<string> DeleteUser(string id) {
            var isDeleted = _userService.DeleteUser(id);
            if (!isDeleted) return BadRequest(new { message = "User not found" });
            return Ok(new { message = "User deleted" });
        }

        [HttpPost("user/journal-list")]
        public ActionResult<string> SetJournals([FromBody]UserDto userDto) {
            var user = _mapper.Map<User>(userDto);

            var userResult = _userService.GetUser(user.Id);
            if (userResult == null) return BadRequest(new { message = "User not found" });

            var journals = _userService.GetJournals(user.Id);
            if (journals != null) {
                if (! _userService.UpdateJournals(user.Id, user.Journals)) {
                    return BadRequest(new { message = "Could not update journals in database" });
                }
                return Ok(new { message = "Journals updated" });
            }

            _userService.AddJournals(user.Id, user.Journals);
            return Ok(new { message = "Journals saved" });
        }

        [HttpGet("user/journal-list/{userId}")]
        public ActionResult<string> GetJournalList(string userId) {
            var result = _userService.GetJournals(userId);
            var journals = _mapper.Map<List<JournalDto>>(result);
            if (journals == null) return Ok(JsonConvert.SerializeObject(new List<JournalDto>()));
            return Ok(JsonConvert.SerializeObject(journals));
        }

        [HttpPost("user/settings")]
        public ActionResult<string> UpdateSettings([FromBody]SettingsDto settingsDto) {
            var settings = _mapper.Map<Settings>(settingsDto);
            var result = _userService.UpdateSettings(settings.UserId, settings);

            if (!result) return BadRequest(new { message = "Settings not found" });

            return Ok(new { message = "Settings updated" });
        }

        [HttpGet("user/settings/{id}")]
        public ActionResult<Settings> GetSettings(string id) {
            var result = _userService.GetSettings(id);

            if (result == null) return BadRequest(new { message = "Settings not found" });

            var settingsDto = _mapper.Map<Settings>(result);
            var settingsDtoStr = JsonConvert.SerializeObject(settingsDto);
            return Ok(settingsDtoStr);
        }
    }
}
