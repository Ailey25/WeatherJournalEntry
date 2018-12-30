using System.Collections.Generic;

namespace WeatherJournalBackend.Dtos {
    public class CoordDto {
        public double Lon { get; set; }
        public double Lat { get; set; }

        public virtual string WeatherObjectId { get; set; }
        public virtual WeatherObjectDto WeatherObject { get; set; }
    }

    public class WeatherDto {
        public int Id { get; set; }                             // Weather condition id
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int WeatherId { get; set; }

        public virtual string WeatherObjectId { get; set; }
        public virtual WeatherObjectDto WeatherObject { get; set; }
    }

    public class MainDto {
        public double Temp { get; set; }
        public double Pressure { get; set; }
        public int Humidity { get; set; }
        public double Temp_min { get; set; }
        public double Temp_max { get; set; }
        public double Sea_level { get; set; }
        public double Grnd_level { get; set; }

        public virtual string WeatherObjectId { get; set; }
        public virtual WeatherObjectDto WeatherObject { get; set; }
    }

    public class WindDto {
        public double Speed { get; set; }
        public double Deg { get; set; }

        public virtual string WeatherObjectId { get; set; }
        public virtual WeatherObjectDto WeatherObject { get; set; }
    }

    public class CloudsDto {
        public int All { get; set; }

        public virtual string WeatherObjectId { get; set; }
        public virtual WeatherObjectDto WeatherObject { get; set; }
    }

    public class SysDto {
        public int Type { get; set; }
        public int Id { get; set; }
        public double Message { get; set; }
        public string Country { get; set; }
        public int Sunrise { get; set; }
        public int Sunset { get; set; }

        public virtual string WeatherObjectId { get; set; }
        public virtual WeatherObjectDto WeatherObject { get; set; }
    }

    public class WeatherObjectDto {
        public CoordDto Coord { get; set; }
        public List<WeatherDto> Weather { get; set; }
        public string @Base { get; set; }
        public MainDto Main { get; set; }
        public int Visibility { get; set; }
        public WindDto Wind { get; set; }
        public CloudsDto Clouds { get; set; }
        public int Dt { get; set; }
        public SysDto Sys { get; set; }
        public int Id { get; set; }                               // City id
        public string Name { get; set; }
        public int Cod { get; set; }

        public string WeatherObjectId { get; set; }
    }
}
