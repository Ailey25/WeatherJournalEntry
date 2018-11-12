using System;
using System.Collections.Generic;

namespace WeatherJournalEntry.Model {
    //public class Coord {
    //    public double lon { get; set; }
    //    public double lat { get; set; }
    //}

    public class Weather {
        public int Id { get; set; }
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    //public class Main {
    //    public double temp { get; set; }
    //    public int pressure { get; set; }
    //    public int humidity { get; set; }
    //    public double temp_min { get; set; }
    //    public double temp_max { get; set; }
    //}

    //public class Wind {
    //    public double speed { get; set; }
    //    public int deg { get; set; }
    //}

    //public class Clouds {
    //    public int all { get; set; }
    //}

    //public class Sys {
    //    public int type { get; set; }
    //    public int id { get; set; }
    //    public double message { get; set; }
    //    public string country { get; set; }
    //    public int sunrise { get; set; }
    //    public int sunset { get; set; }
    //}

    //public class WeatherObject {
    //    public Coord coord { get; set; }
    //    public List<Weather> weather { get; set; }
    //    public string @base { get; set; }
    //    public Main main { get; set; }
    //    public int visibility { get; set; }
    //    public Wind wind { get; set; }
    //    public Clouds clouds { get; set; }
    //    public int dt { get; set; }
    //    public Sys sys { get; set; }
    //    public int id { get; set; }
    //    public string name { get; set; }
    //    public int cod { get; set; }
    //}
}
