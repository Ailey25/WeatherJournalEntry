using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Microsoft.Data.Sqlite;
using WeatherJournalBackend.Entities;
using WeatherJournalBackend.UoW;

namespace WeatherJournalBackend.Services {
    public interface IUserService {
        User Authenticate(string username, string password);
        string Create(User user, string password);
        User GetUser(string userId);
        string UpdateFirstLastName(User userParam);
        string UpdateUsername(User userParam);
        string UpdatePassword(User userParam, string oldPassword, string newPassword);
        bool DeleteUser(string userId);

        void AddJournals(string userId, List<Journal> journals);
        Settings GetSettings(string userId);
        List<Journal> GetJournals(string userId);
        bool UpdateJournals(string userId, List<Journal> newJournalList);

        void SetSettings(string userId);
        bool UpdateSettings(string userId, Settings newSettings);
    }

    public class UserService : IUserService {
        private IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        public User Authenticate(string username, string password) {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
                return null;
            }

            var user = _unitOfWork.UserRepository.GetUserByUsername(username);
            if (user == null) return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) {
                return null;
            }

            return user;
        }

        public string Create(User user, string password) {
            try {
                if (string.IsNullOrWhiteSpace(password)) {
                    return "password cannot be null";
                }

                if (!(CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt))) {
                    return "Failed creating password";
                }

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Id = Guid.NewGuid().ToString();

                _unitOfWork.UserRepository.Add(user);
                _unitOfWork.SaveUserChanges();

                return "";
            } catch (Exception ex) when (ex.InnerException is SqliteException sqliteException) {
                switch(sqliteException.SqliteErrorCode) {
                    case 19:
                        return "Username " + user.Username + " is already taken";
                    default:
                        throw;
                }
            } catch (Exception ex) {
                return "User can't be created";
            }
        }

        public User GetUser(string userId) {
            return _unitOfWork.UserRepository.GetUserById(userId);
        }

        public string UpdateFirstLastName(User userParam) {
            var user = _unitOfWork.UserRepository.GetUserById(userParam.Id);

            if (user == null) return "User not found";

            user.FirstName = userParam.FirstName;
            user.LastName = userParam.LastName;
            _unitOfWork.SaveUserChanges();

            return "";
        }

        public string UpdateUsername(User userParam) {
            try {
                var user = _unitOfWork.UserRepository.GetUserById(userParam.Id);

                if (user == null) return "User not found";

                user.Username = userParam.Username;
                _unitOfWork.SaveUserChanges();

                return "";
            } catch (Exception ex) when (ex.InnerException is SqliteException sqliteException) {
                switch (sqliteException.SqliteErrorCode) {
                    case 19:
                        return "Username " + userParam.Username + " is already taken";
                    default:
                        throw;
                }
            } catch (Exception ex) {
                return "Username can't be updated";
            }
        }

        public string UpdatePassword(User userParam, string oldPassword, string newPassword) {
            var user = _unitOfWork.UserRepository.GetUserById(userParam.Id);

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
            _unitOfWork.SaveUserChanges();

            return "";
        }

        public bool DeleteUser(string userId) {
            var user = _unitOfWork.UserRepository.GetUserById(userId);

            if (user == null) return false;

            _unitOfWork.UserRepository.DeleteUser(user);
            _unitOfWork.SaveUserChanges();

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
                _unitOfWork.UserRepository.Add(journal);
            }

            _unitOfWork.SaveUserChanges();
        }

        public Settings GetSettings(string userId) {
            return _unitOfWork.UserRepository.GetSettings(userId);
        }

        public List<Journal> GetJournals(string userId) {
            return _unitOfWork.UserRepository.GetJournals(userId);
        }

        public bool UpdateJournals(string userId, List<Journal> newJournalList) {
            var oldJournalRows = _unitOfWork.UserRepository.GetJournals(userId);

            if (!oldJournalRows.Any()) return false;

            _unitOfWork.UserRepository.DeleteList(oldJournalRows);
            _unitOfWork.SaveUserChanges();

            foreach (Journal journal in newJournalList) {
                journal.UserId = userId;
                _unitOfWork.UserRepository.Add(journal);
            }
            _unitOfWork.SaveUserChanges();

            return true;
        }

        // Initial settings when user is registered
        public void SetSettings(string userId) {
            var initSettings = new Settings {
                UserId = userId,
                TempUnit = "C"
            };
            _unitOfWork.UserRepository.Add(initSettings);

            _unitOfWork.SaveUserChanges();
        }

        public bool UpdateSettings(string userId, Settings newSettings) {
            var existingSettings = _unitOfWork.UserRepository.GetSettings(userId);

            if (existingSettings == null) return false;
            if (newSettings.TempUnit != "C" && newSettings.TempUnit != "F") {
                return false;
            }

            existingSettings.TempUnit = newSettings.TempUnit;
            _unitOfWork.SaveUserChanges();

            return true;
        }
    }
}
