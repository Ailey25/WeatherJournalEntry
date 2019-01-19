using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherJournalBackend.Entities;
using WeatherJournalBackend.Data;

namespace WeatherJournalBackend.Services {
    public interface IUserService {
        User Authenticate(string username, string password);
        User GetUser(string id);
        string Create(User user, string password);
        string UpdateFirstLastName(User userParam);
        string UpdateUsername(User userParam);
        string UpdatePassword(User userParam, string oldPassword, string newPassword);
        bool DeleteUser(string userId);

        void AddJournals(string userId, List<Journal> journals);
        Task<List<Journal>> GetJournals(string userId);
        Task<bool> UpdateJournals(string userId, List<Journal> newJournalList);

        void SetSettings(string userId);
        Task<Settings> GetSettings(string userId);
        Task<bool> UpdateSettings(string userId, Settings newSettings);
    }

    public class UserService : IUserService {
        private UserContext _context;

        public UserService(UserContext context) {
            _context = context;
        }

        public User Authenticate(string username, string password) {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
                return null;
            }

            var user = _context.Users.FirstOrDefault(x => x.Username == username);
            if (user == null) return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) {
                return null;
            }

            return user;
        }

        public User GetUser(string id) {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public string Create(User user, string password) {
            if (string.IsNullOrWhiteSpace(password)) {
                return "password cannot be null";
            }

            if (_context.Users.Any(x => x.Username == user.Username)) {
                return "Username \"" + user.Username + "\" is already taken";
            }

            if (!(CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt))) {
                return "Failed creating password";
            }

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Id = Guid.NewGuid().ToString();

            _context.Users.Add(user);
            _context.SaveChanges();

            return "";
        }

        public string UpdateFirstLastName(User userParam) {
            var user = GetUser(userParam.Id);

            if (user == null) return "User not found";

            user.FirstName = userParam.FirstName;
            user.LastName = userParam.LastName;

            _context.Users.Update(user);
            _context.SaveChanges();

            return "";
        }

        public string UpdateUsername(User userParam) {
            var user = GetUser(userParam.Id);

            if (user == null) return "User not found";

            if (userParam.Username != user.Username) {
                // username has changed so check if the new username is already taken
                if (_context.Users.Any(x => x.Username == userParam.Username))
                    return "Username " + userParam.Username + " is already taken";
            }

            user.Username = userParam.Username;

            _context.Users.Update(user);
            _context.SaveChanges();

            return "";
        }

        public string UpdatePassword(User userParam, string oldPassword, string newPassword) {
            var user = GetUser(userParam.Id);
            if (user == null) return "User not found";

            if (string.IsNullOrWhiteSpace(newPassword)) {
                return "New password cannot be null";
            }

            var authenticateResult = Authenticate(user.Username, oldPassword);
            if (authenticateResult == null) return "Old password is incorrect";

            if (!(CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt))) {
                return "Failed creating password";
            }

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Update(user);
            _context.SaveChanges();

            return "";
        }

        public bool DeleteUser(string userId) {
            var user = GetUser(userId);

            if (user == null) return false;
           
            _context.Remove(user);
            _context.SaveChanges();
            return true;
        }

        private static bool CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) {
            if (password == null) {
                passwordHash = new byte[0];
                passwordSalt = new byte[0];
                return false;
            }
            if (string.IsNullOrWhiteSpace(password)) {
                passwordHash = new byte[0];
                passwordSalt = new byte[0];
                return false;
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }

            return true;
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt) {
            if (password == null) {
                return false;
                // throw new ArgumentNullException("password");
            }
            if (string.IsNullOrWhiteSpace(password)) {
                return false;
                //throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            }
            if (storedHash.Length != 64) {
                return false;
                // throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            }
            if (storedSalt.Length != 128) {
                return false;
                //throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt)) {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++) {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        public void AddJournals(string userId, List<Journal> journals) {
            foreach (Journal journal in journals) {
                journal.UserId = userId;
                _context.Journals.Add(journal);
            }

            _context.SaveChanges();
        }

        // Returns null if not found
        public async Task<List<Journal>> GetJournals(string userId) {
            var result = await _context.Journals
              .Where(j => j.UserId == userId).ToListAsync();
            if (result.Any()) return result;
            return null;
        }

        public async Task<bool> UpdateJournals(string userId, List<Journal> newJournalList) {
            var oldJournalRows = await _context.Journals
              .Where(j => j.UserId == userId).ToListAsync();
            if (oldJournalRows.Any()) {
                _context.Journals.RemoveRange(oldJournalRows);
            } else {
                return false;
            }
            _context.SaveChanges();

            foreach (Journal journal in newJournalList) {
                journal.UserId = userId;
                _context.Journals.Add(journal);
            }

            _context.SaveChanges();
            return true;
        }

        // Initial settings when user is registered
        public void SetSettings(string userId) {
            var initSettings = new Settings {
                UserId = userId,
                TempUnit = "C"
            };
            _context.Settings.Add(initSettings);

            _context.SaveChanges();
        }

        public async Task<bool> UpdateSettings(string userId, Settings newSettings) {
            var existingSettings = await GetSettings(userId);
            if (existingSettings == null) return false;
            existingSettings.TempUnit = newSettings.TempUnit;
            _context.SaveChanges();
            return true;
        }

        public async Task<Settings> GetSettings(string userId) {
            return await _context.Settings
              .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
