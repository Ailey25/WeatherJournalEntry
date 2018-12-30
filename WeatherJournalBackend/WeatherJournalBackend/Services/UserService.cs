using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherJournalBackend.Entities;
using WeatherJournalBackend.Data;

namespace WeatherJournalBackend.Services {
    public interface IUserService {
        User Authenticate(string username, string password);
        User Get(string username);
        string Create(User user, string password);
        string Update(User user, string password);
        void Delete(string username);
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

        public User Get(string id) {
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

            _context.Users.Add(user);
            _context.SaveChanges();

            return "";
        }

        public string Update(User userParam, string password) {
            var user = Get(userParam.Id);

            if (string.IsNullOrWhiteSpace(password)) {
                return "password cannot be null";
            }

            if (user == null) {
                return "User not found";
            }

            if (userParam.Username != user.Username) {
                // username has changed so check if the new username is already taken
                if (_context.Users.Any(x => x.Username == userParam.Username))
                    return "Username " + userParam.Username + " is already taken";
            }

            user.FirstName = userParam.FirstName;
            user.LastName = userParam.LastName;
            user.Username = userParam.Username;

            if (!(CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt))) {
                return "Failed creating password";
            }

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Update(user);
            _context.SaveChanges();

            return "";
        }

        public void Delete(string id) {
            var user = Get(id);
            if (user != null) {
                _context.Remove(user);
                _context.SaveChanges();
            }
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
    }
}
