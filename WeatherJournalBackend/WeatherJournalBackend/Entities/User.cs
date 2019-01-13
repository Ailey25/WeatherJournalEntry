using System.Collections.Generic;

namespace WeatherJournalBackend.Entities {
    public class User {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public List<Journal> Journals { get; set; }
        public Settings Settings { get; set; }
    }

    public class Journal {
        public string UserId { get; set; }
        public string JournalId { get; set; }
        public string Title { get; set; }
        public string Entry { get; set; }
        public string CityId { get; set; }
    }

    public class Settings {
        public string UserId { get; set; }
        public string TempUnit { get; set; }
    }
}
