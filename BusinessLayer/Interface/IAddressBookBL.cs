using ModelLayer.Entity;
using ModelLayer.DTO;  // Added for DTO reference
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Interface
{
    public interface IAddressBookBL
    {
        Addresses SaveAddressBookBL(AddressEntity addressEntity);  // Changed return type to Addresses
        Task<AddressEntity?> GetAddressBookByIdBL(int id, string? email);
        Task<List<Addresses>> GetAllAddressBooksBL(string? email);  // Changed return type to List<Addresses>
        Task<Addresses> EditAddressBookBL(string? email, int id, AddressEntity addressEntity);  // Changed return type to Addresses
        bool DeleteAddressBookBL(string? email, int id);
    }
}
