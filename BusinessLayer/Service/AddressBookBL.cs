using ModelLayer.Entity;
using RepositoryLayer.Interface;
using NLog;
using BusinessLayer.Interface;
using ModelLayer.DTO;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        private readonly IAddressBookRL _addressBookRL;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AddressBookBL(IAddressBookRL addressBookRL)
        {
            _addressBookRL = addressBookRL;
        }

        public Addresses SaveAddressBookBL(AddressEntity addressEntity)
        {
            Logger.Info("Saving address for: {0}", addressEntity.FullName);
            return _addressBookRL.SaveAddressBookRL(addressEntity);
        }

        public async Task<AddressEntity?> GetAddressBookByIdBL(int id, string? email)
        {
            Logger.Info("Fetching address by ID: {0}", id);
            return await _addressBookRL.GetAddressBookByIdRL(id, email);
        }

        public async Task<List<Addresses>> GetAllAddressBooksBL(string? email)
        {
            Logger.Info("Fetching all addresses.");
            var entityList = await _addressBookRL.GetAllAddressBooksRL(email);
            return entityList ?? new List<Addresses>();  // Ensuring a valid return type
        }


        public async Task<Addresses> EditAddressBookBL(string? email, int id, AddressEntity addressEntity)
        {
            Logger.Info("Editing address ID: {0}", id);
            var result = await _addressBookRL.EditAddressBookRL(email, id, addressEntity);
            return result;
        }

        public bool DeleteAddressBookBL(string? email, int id)
        {
            Logger.Info("Deleting address ID: {0}", id);
            return _addressBookRL.DeleteAddressBookRL(email, id);
        }
    }
}