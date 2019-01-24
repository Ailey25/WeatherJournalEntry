using WeatherJournalBackend.Data;

namespace WeatherJournalBackend.UoW {
    public interface IUnitOfWork {
        UserRepository UserRepository { get; }
        WeatherRepository WeatherRepository { get; }

        void SaveUserChanges();
        void SaveWeatherChanges();
    }

    public class UnitOfWork : IUnitOfWork {
        private UserContext _userContext;
        private UserRepository _userRepository;
        private WeatherContext _weatherContext;
        private WeatherRepository _weatherRepository;

        public UnitOfWork(UserContext userContext, WeatherContext weatherContext) {
            _userContext = userContext;
            _weatherContext = weatherContext;
        }

        public UserRepository UserRepository {
            get {
                if (_userRepository == null) _userRepository = new UserRepository(_userContext);
                return _userRepository;
            }
        }

        public WeatherRepository WeatherRepository {
            get {
                if (_weatherRepository == null) _weatherRepository = new WeatherRepository(_weatherContext);
                return _weatherRepository;
            }
        }

        public void SaveUserChanges() {
            _userContext.SaveChanges();
        }

        public void SaveWeatherChanges() {
            _weatherContext.SaveChanges();
        }

    }
}
