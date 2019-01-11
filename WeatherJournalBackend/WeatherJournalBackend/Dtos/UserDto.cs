using System.Collections.Generic;

namespace WeatherJournalBackend.Dtos {
    public class UserDto {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public List<JournalDto> Journals { get; set; }
        public SettingsDto Settings { get; set; }
    }

    public class JournalDto {
        public string UserId { get; set; }
        public string JournalId { get; set; }
        public string Title { get; set; }
        public string Entry { get; set; }
        public string CallType { get; set; }
    }

    public class SettingsDto {
        public string UserId { get; set; }
        public string TempUnit { get; set; }

    }
}
