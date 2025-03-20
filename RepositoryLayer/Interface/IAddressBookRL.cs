using ModelLayer.DTO;
using ModelLayer.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryLayer.Interface
{
    public interface IAddressBookRL
    {
        Addresses SaveAddressBookRL(AddressEntity addressEntity);
        Task<AddressEntity?> GetAddressBookByIdRL(int id, string? email); // Corrected nullable return type
        Task<List<Addresses>> GetAllAddressBooksRL(string? email); // Corrected return type to List<Addresses>
        Task<Addresses?> EditAddressBookRL(string? email, int id, AddressEntity addressEntity); // Corrected return type
        bool DeleteAddressBookRL(string? email, int id);

        // Corrected DTO mapping method signature
        Addresses MapToDTO(AddressEntity entity);

        // Added missing async Task return type for RefreshAllAddressesCache
        Task RefreshAllAddressesCache();
    }
}
