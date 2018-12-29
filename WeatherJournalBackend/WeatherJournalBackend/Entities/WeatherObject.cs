using System;
using System.Collections.Generic;

namespace WeatherJournalBackend.Entities {
    public class Coord {
        public double Lon { get; set; }
        public double Lat { get; set; }
        public string WeatherObjectId { get; set; }
    }

    public class Weather {
        public int Id { get; set; }                             // Weather condition id
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int WeatherId { get; set; }
        public string WeatherObjectId { get; set; }
    }

    public class Main {
        public double Temp { get; set; }
        public double Pressure { get; set; }
        public int Humidity { get; set; }
        public double Temp_min { get; set; }
        public double Temp_max { get; set; }
        public double Sea_level { get; set; }
        public double Grnd_level { get; set; }
        public string WeatherObjectId { get; set; }
    }

    public class Wind {
        public double Speed { get; set; }
        public double Deg { get; set; }
        public string WeatherObjectId { get; set; }
    }

    public class Clouds {
        public int All { get; set; }
        public string WeatherObjectId { get; set; }
    }

    public class Sys {
        public int Type { get; set; }
        public int Id { get; set; }
        public double Message { get; set; }
        public string Country { get; set; }
        public int Sunrise { get; set; }
        public int Sunset { get; set; }
        public string WeatherObjectId { get; set; }
    }

    public class WeatherObject {
        public Coord Coord { get; set; }
        public List<Weather> Weather { get; set; }
        public string @Base { get; set; }
        public Main Main { get; set; }
        public int Visibility { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public int Dt { get; set; }
        public Sys Sys { get; set; }
        public int Id { get; set; }                               // City id
        public string Name { get; set; }
        public int Cod { get; set; }
        public string WeatherObjectId { get; set; }
    }
}
