using System.Collections.Generic;
using System.Linq;
using WeatherJournalBackend.Entities;

namespace WeatherJournalBackend.Data {
    public interface IUserRepository {
        void Add<T>(T entity) where T : class;

        User GetUserById(string userId);
        User GetUserByUsername(string username);
        List<Journal> GetJournals(string userId);
        Settings GetSettings(string userId);

        void DeleteUser(User user);
        void DeleteList<T>(List<T> entityList) where T : class;
    }

    public class UserRepository : IUserRepository {
        private UserContext _userContext;

        public UserRepository(UserContext userContext) {
            _userContext = userContext;
        }

        public void Add<T>(T entity) where T : class {
            _userContext.Add(entity);
        }

        public User GetUserById(string userid) {
            return _userContext.Users.FirstOrDefault(u => u.Id == userid);
        }

        public User GetUserByUsername(string username) {
            return _userContext.Users.FirstOrDefault(u => u.Username == username);
        }

        // Returns null if no result
        public List<Journal> GetJournals(string userId) {
            var result = _userContext.Journals
              .Where(j => j.UserId == userId).ToList();
            if (result.Any()) return result;
            return null;
        }

        public Settings GetSettings(string userId) {
            return _userContext.Settings
              .FirstOrDefault(s => s.UserId == userId);
        }

        public void DeleteUser(User user) {
            _userContext.Users.Remove(user);
        }

        public void DeleteList<T>(List<T> entityList) where T : class {
            _userContext.RemoveRange(entityList);
        }
    }
}

