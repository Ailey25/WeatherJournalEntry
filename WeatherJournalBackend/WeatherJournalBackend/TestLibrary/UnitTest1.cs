using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using WeatherJournalBackend.Entities;
using WeatherJournalBackend.Services;
using WeatherJournalBackend.UoW;

namespace TestLibrary {
    [TestFixture]
    public class UnitTest1 {
        //[Test]
        //public void TestFail() {
        //    Assert.That(true, Is.False);
        //}

        [Test]
        public void GetUserById_ValidId() {
            User validUser = new User {
                Id = "2",
                FirstName = "old first name",
                LastName = "old new name",
                Username = "oldUser123"
            };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(x => x.UserRepository.GetUserById(validUser.Id))
                          .Returns(validUser);

            UserService userService = new UserService(unitOfWorkMock.Object);
            var actual = userService.GetUser(validUser.Id);

            Assert.IsTrue(actual != null);
            Assert.IsTrue(actual.FirstName == "old first name");
            Assert.IsTrue(actual.LastName == "old new name");
            Assert.IsTrue(actual.Username == "oldUser123");
        }

        [Test]
        public void GetJournals_ValidJournals() {
            string validUserId = "2";
            List<Journal> validJournals = new List<Journal>();
            validJournals.Add(new Journal {
                JournalId = "3",
                Title = "title 1",
                Entry = "entry 1",
                CityId = "345"
            });
            validJournals.Add(new Journal {
                JournalId = "4",
                Title = "title 2",
                Entry = "entry 2",
                CityId = "234"
            });

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(x => x.UserRepository.GetJournals(validUserId))
                          .Returns(validJournals);

            UserService userService = new UserService(unitOfWorkMock.Object);
            var actual = userService.GetJournals(validUserId);

            CollectionAssert.AreEqual(validJournals, actual);
        }

        [Test]
        public void GetJournals_NotFound() {
            List<Journal> emptyJournal = null;
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(x => x.UserRepository.GetJournals(It.IsAny<string>()))
                          .Returns(emptyJournal);

            UserService userService = new UserService(unitOfWorkMock.Object);
            var actual = userService.GetJournals(It.IsAny<string>());

            Assert.IsTrue(actual == null);
        }
    }
}
