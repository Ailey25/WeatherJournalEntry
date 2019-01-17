using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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
                    result = await _weatherService.GetCoord(weatherObjectId);
                    break;
                case WEATHER:
                    result = await _weatherService.GetWeatherList(weatherObjectId);
                    break;
                case MAIN:
                    result = await _weatherService.GetMain(weatherObjectId);
                    break;
                case WIND:
                    result = await _weatherService.GetWind(weatherObjectId);
                    break;
                case CLOUDS:
                    result = await _weatherService.GetClouds(weatherObjectId);
                    break;
                case SYS:
                    result = await _weatherService.GetSys(weatherObjectId);
                    break;
                case WEATHER_OBJECT:
                    result = await _weatherService.GetWeatherObject(weatherObjectId);
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
            var wo = await _weatherService.GetWeatherObject(weatherObjIdToAssign);
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
                    var errorStringCallType = JsonConvert.SerializeObject(new {
                        ok = false,
                        message = "Failed: check callTypeAPI parameter(s)"
                    });
                    return BadRequest(errorStringCallType);
            }

            if (responseStr == null) {
                return BadRequest("Failed: API call failed");
            }

            // Parse string into object and ADD to or UPDATE in database
            var newWeatherObj = weatherAPI.ParseWeatherDataObject(responseStr);
            if (isNewObject) {
                _weatherService.AddWeatherObject(newWeatherObj, weatherObjIdToAssign);
            } else {
                if (!(await _weatherService.UpdateWeatherObject(newWeatherObj, weatherObjIdToAssign))) {
                    var errorStringNotFound = JsonConvert.SerializeObject(new {
                        ok = false,
                        message = "Object to update not found in database"
                    });
                    return BadRequest(errorStringNotFound);
                }
            }
            var successString = JsonConvert.SerializeObject(new {
                ok = true,
                weatherObjectStr = responseStr
            });
            return Ok(successString);
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

        // DELETE api/values/coord/5
        [HttpDelete("{objectType}/{weatherObjectId}")]
        public async Task<ActionResult<string>> Delete(
            string objectType, string weatherObjectId
        ) {

            ActionResult<object> obj = null;
            switch (objectType) {
                case COORD:
                    obj = await _weatherService.GetCoord(weatherObjectId);
                    break;
                case WEATHER:
                    return await _weatherService.DeleteWeatherList(weatherObjectId);
                case MAIN:
                    obj = await _weatherService.GetMain(weatherObjectId);
                    break;
                case WIND:
                    obj = await _weatherService.GetWind(weatherObjectId);
                    break;
                case CLOUDS:
                    obj = await _weatherService.GetClouds(weatherObjectId);
                    break;
                case SYS:
                    obj = await _weatherService.GetSys(weatherObjectId);
                    break;
                case WEATHER_OBJECT:
                    obj = await _weatherService.GetWeatherObject(weatherObjectId);
                    break;
                default:
                    return BadRequest("Failed: Check object type parameter");
            }

            // when it's a single object
            if (obj == null || obj.Value == null) {
                return BadRequest("Failed: Id wasn't found in the database");
            } else {
                _weatherService.DeleteObject(obj.Value);
                return Ok("Success: Object deleted from database");
            }
        }

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
        public ActionResult<string> Delete(string id) {
            _userService.Delete(id);
            return Ok();
        }

        [HttpPost("user/journal-list")]
        public async Task<ActionResult<string>> SetJournals([FromBody]UserDto userDto) {
            bool journalsIsExist = false;
            var user = _mapper.Map<User>(userDto);

            if (_userService.GetUser(user.Id) == null) {
                var errorString = JsonConvert.SerializeObject(new {
                    ok = false,
                    message = "User not found"
                });
                return BadRequest(errorString);
            }

            var journals = _userService.GetJournals(user.Id);
            if (journals.Result != null) journalsIsExist = true;

            if (journalsIsExist) {
                if (!(await _userService.UpdateJournals(user.Id, user.Journals))) {
                    var errorString = JsonConvert.SerializeObject(new {
                        ok = false,
                        message = "Could not update journals in database"
                    });
                    return BadRequest(errorString);
                }
            } else {
                _userService.AddJournals(user.Id, user.Journals);
            }

            var successString = JsonConvert.SerializeObject(new {
                ok = true,
                message = "Journals updated/added"
            });
            return Ok(successString);
        }

        [HttpGet("user/journal-list/{userId}")]
        public async Task<ActionResult<string>> GetJournalList(string userId) {
            var result = await _userService.GetJournals(userId);
            var journals = _mapper.Map<List<JournalDto>>(result);
            if (journals == null) journals = new List<JournalDto>();
            var successString1 = JsonConvert.SerializeObject(new {
                ok = true,
                journalList = journals
            });
            return Ok(successString1);
        }

        [HttpPost("user/settings")]
        public ActionResult<string> UpdateSettings([FromBody]SettingsDto settingsDto) {
            var settings = _mapper.Map<Settings>(settingsDto);
            var result = _userService.UpdateSettings(settings.UserId, settings);

            if (!result.Result) return BadRequest(new { message = "Settings not found" });

            return Ok(new { message = "Settings updated" });
        }

        [HttpGet("user/settings/{id}")]
        public ActionResult<Settings> GetSettings(string id) {
            var result = _userService.GetSettings(id);

            if (result == null) return BadRequest(new { message = "Settings not found" });

            var settingsDto = _mapper.Map<Settings>(result.Result);
            var settingsDtoStr = JsonConvert.SerializeObject(settingsDto);
            return Ok(settingsDtoStr);
        }
    }
}
