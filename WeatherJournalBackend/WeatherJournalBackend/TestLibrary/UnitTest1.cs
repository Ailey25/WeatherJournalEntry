using System;
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
    }
}
