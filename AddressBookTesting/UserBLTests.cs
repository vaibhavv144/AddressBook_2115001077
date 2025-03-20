using NUnit.Framework;
using Moq;
using BusinessLayer.Service;
using BusinessLayer.Interface;
using RepositoryLayer.Interface;
using Middleware.JwtHelper;
using ModelLayer.DTO;
using ModelLayer.Entity;
using Microsoft.Extensions.Logging;

namespace AddressBookTesting
{
    [TestFixture]
    public class UserBLTests
    {
        private UserBL _userBL;
        private Mock<IUserRL> _mockUserRepo;
        private Mock<IJwtTokenHelper> _mockJwtHelper;
        private Mock<ILogger<UserBL>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockUserRepo = new Mock<IUserRL>();
            _mockJwtHelper = new Mock<IJwtTokenHelper>();
            _mockLogger = new Mock<ILogger<UserBL>>();

            _userBL = new UserBL(_mockUserRepo.Object, _mockLogger.Object, _mockJwtHelper.Object);
        }

        [Test]
        public void RegistrationBL_ValidUser_ReturnsUserEntity()
        {
            // Arrange
            var registerDTO = new RegisterDTO { Email = "test@example.com", password = "Test@123" };
            var expectedUser = new UserEntity { Email = "test@example.com", Password = "Test@123" };

            _mockUserRepo.Setup(repo => repo.Registration(It.IsAny<RegisterDTO>())).Returns(expectedUser);

            // Act
            var result = _userBL.RegistrationBL(registerDTO);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser.Email, result.Email);
        }

        [Test]
        public void RegistrationBL_NullUser_ReturnsNull()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.Registration(It.IsAny<RegisterDTO>())).Returns((UserEntity)null);

            // Act
            var result = _userBL.RegistrationBL(new RegisterDTO { Email = "invalid@example.com" });

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void LoginnUserBL_ValidCredentials_ReturnsUserWithToken()
        {
            // Arrange
            var loginDTO = new LoginDTO { Email = "test@example.com", Password = "Test@123" };
            var expectedUser = new UserEntity { Email = "test@example.com", Role = "User" };
            var expectedToken = "ValidJWTToken";

            _mockUserRepo.Setup(repo => repo.LoginnUserRL(It.IsAny<LoginDTO>())).Returns(expectedUser);
            _mockJwtHelper.Setup(helper => helper.GenerateToken(expectedUser.Email, expectedUser.Role)).Returns(expectedToken);

            // Act
            var (user, token) = _userBL.LoginnUserBL(loginDTO);

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(expectedUser.Email, user.Email);
            Assert.AreEqual(expectedToken, token);
        }

        [Test]
        public void LoginnUserBL_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.LoginnUserRL(It.IsAny<LoginDTO>())).Returns((UserEntity)null);

            // Act
            var (user, token) = _userBL.LoginnUserBL(new LoginDTO { Email = "invalid@example.com", Password = "wrongpass" });

            // Assert
            Assert.IsNull(user);
            Assert.IsNull(token);
        }

        [Test]
        public void UpdateUserPassword_ValidUser_ReturnsTrue()
        {
            // Arrange
            var email = "test@example.com";
            var newPassword = "NewPass@123";
            var user = new UserEntity { Email = email, Password = "OldPass@123" };

            _mockUserRepo.Setup(repo => repo.FindByEmail(email)).Returns(user);
            _mockUserRepo.Setup(repo => repo.Update(It.IsAny<UserEntity>())).Returns(true);

            // Act
            var result = _userBL.UpdateUserPassword(email, newPassword);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void UpdateUserPassword_UserNotFound_ReturnsFalse()
        {
            // Arrange
            var email = "invalid@example.com";

            _mockUserRepo.Setup(repo => repo.FindByEmail(email)).Returns((UserEntity)null);

            // Act
            var result = _userBL.UpdateUserPassword(email, "NewPass@123");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetByEmail_ValidEmail_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var expectedUser = new UserEntity { Email = email };

            _mockUserRepo.Setup(repo => repo.FindByEmail(email)).Returns(expectedUser);

            // Act
            var result = _userBL.GetByEmail(email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(email, result.Email);
        }

        [Test]
        public void GetByEmail_InvalidEmail_ReturnsNull()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.FindByEmail("invalid@example.com")).Returns((UserEntity)null);

            // Act
            var result = _userBL.GetByEmail("invalid@example.com");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ValidateEmail_ValidEmail_ReturnsTrue()
        {
            // Arrange
            var email = "test@example.com";

            _mockUserRepo.Setup(repo => repo.ValidateEmail(email)).Returns(true);

            // Act
            var result = _userBL.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ValidateEmail_InvalidEmail_ReturnsFalse()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.ValidateEmail("invalid@example.com")).Returns(false);

            // Act
            var result = _userBL.ValidateEmail("invalid@example.com");

            // Assert
            Assert.IsFalse(result);
        }

    }
}