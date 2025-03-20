using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Service;
using BusinessLayer.Interface;
using RepositoryLayer.Interface;
using ModelLayer.Entity;
using ModelLayer.DTO;
using NLog;

namespace AddressBookTesting
{
    [TestFixture]
    public class AddressBookBLTests
    {
        private AddressBookBL _addressBookBL;
        private Mock<IAddressBookRL> _mockAddressBookRepo;

        [SetUp]
        public void Setup()
        {
            _mockAddressBookRepo = new Mock<IAddressBookRL>();
            _addressBookBL = new AddressBookBL(_mockAddressBookRepo.Object);
        }

        [Test]
        public void SaveAddressBookBL_ValidAddress_ReturnsAddress()
        {
            // Arrange
            var addressEntity = new AddressEntity { FullName = "John Doe", Address = "New York" };
            var expectedAddress = new Addresses { FullName = "John Doe", Address = "New York" };

            _mockAddressBookRepo.Setup(repo => repo.SaveAddressBookRL(It.IsAny<AddressEntity>())).Returns(expectedAddress);

            // Act
            var result = _addressBookBL.SaveAddressBookBL(addressEntity);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedAddress.FullName, result.FullName);
        }

        [Test]
        public async Task GetAddressBookByIdBL_ValidId_ReturnsAddress()
        {
            // Arrange
            var id = 1;
            var email = "test@example.com";
            var expectedAddress = new AddressEntity { Id = 1, FullName = "John Doe" };

            _mockAddressBookRepo.Setup(repo => repo.GetAddressBookByIdRL(id, email)).ReturnsAsync(expectedAddress);

            // Act
            var result = await _addressBookBL.GetAddressBookByIdBL(id, email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);
        }

        [Test]
        public async Task GetAddressBookByIdBL_InvalidId_ReturnsNull()
        {
            // Arrange
            var id = 99;
            var email = "test@example.com";

            _mockAddressBookRepo.Setup(repo => repo.GetAddressBookByIdRL(id, email)).ReturnsAsync((AddressEntity)null);

            // Act
            var result = await _addressBookBL.GetAddressBookByIdBL(id, email);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllAddressBooksBL_ValidData_ReturnsList()
        {
            // Arrange
            var email = "test@example.com";
            var expectedList = new List<Addresses>
            {
                new Addresses { FullName = "John Doe", Address = "New York" },
                new Addresses { FullName = "Jane Doe", Address = "Los Angeles" }
            };

            _mockAddressBookRepo.Setup(repo => repo.GetAllAddressBooksRL(email)).ReturnsAsync(expectedList);

            // Act
            var result = await _addressBookBL.GetAllAddressBooksBL(email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetAllAddressBooksBL_NoData_ReturnsEmptyList()
        {
            // Arrange
            _mockAddressBookRepo.Setup(repo => repo.GetAllAddressBooksRL(It.IsAny<string>())).ReturnsAsync((List<Addresses>)null);

            // Act
            var result = await _addressBookBL.GetAllAddressBooksBL("test@example.com");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task EditAddressBookBL_ValidAddress_ReturnsUpdatedAddress()
        {
            // Arrange
            var id = 1;
            var email = "test@example.com";
            var updatedAddress = new AddressEntity { Id = 1, FullName = "Updated Name" };
            var expectedAddress = new Addresses { FullName = "Updated Name" };

            _mockAddressBookRepo.Setup(repo => repo.EditAddressBookRL(email, id, updatedAddress)).ReturnsAsync(expectedAddress);

            // Act
            var result = await _addressBookBL.EditAddressBookBL(email, id, updatedAddress);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedAddress.FullName, result.FullName);
        }

        [Test]
        public void DeleteAddressBookBL_ValidId_ReturnsTrue()
        {
            // Arrange
            var id = 1;
            var email = "test@example.com";

            _mockAddressBookRepo.Setup(repo => repo.DeleteAddressBookRL(email, id)).Returns(true);

            // Act
            var result = _addressBookBL.DeleteAddressBookBL(email, id);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteAddressBookBL_InvalidId_ReturnsFalse()
        {
            // Arrange
            var id = 99;
            var email = "test@example.com";

            _mockAddressBookRepo.Setup(repo => repo.DeleteAddressBookRL(email, id)).Returns(false);

            // Act
            var result = _addressBookBL.DeleteAddressBookBL(email, id);

            // Assert
            Assert.IsFalse(result);
        }
    }
}