using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using ModelLayer.Entity;
using NLog;
using ModelLayer.DTO;

namespace RepositoryLayer.Service
{
    public class AddressBookRL : IAddressBookRL
    {
        private readonly AddressBookContext _dbContext;
        private readonly IDistributedCache _cache;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Constructor to initialize DbContext and Distributed Cache
        public AddressBookRL(AddressBookContext dbContext, IDistributedCache cache)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // Method to save a new address entry
        public Addresses SaveAddressBookRL(AddressEntity addressEntity)
        {
            try
            {
                // Check if the user exists
                var user = _dbContext.Users.FirstOrDefault(u => u.Email == addressEntity.UserEmail);
                if (user == null) return null;

                addressEntity.User = user;
                _dbContext.AddressBooks.Add(addressEntity);
                _dbContext.SaveChanges();

                // Invalidate cache for all addresses and specific user addresses
                _cache.Remove("AllAddresses");
                _cache.Remove($"Addresses{addressEntity.UserEmail}");

                // Refresh cache asynchronously
                Task.Run(async () => await RefreshAllAddressesCache());

                return MapToDTO(addressEntity);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while saving address.");
                throw;
            }
        }

        // Method to get an address entry by ID with optional email filtering
        public async Task<AddressEntity?> GetAddressBookByIdRL(int id, string? email)
        {
            try
            {
                string cacheKey = $"Address_{id}";
                var cachedAddress = await _cache.GetStringAsync(cacheKey);

                // Return cached data if available
                if (!string.IsNullOrEmpty(cachedAddress))
                {
                    Logger.Info($"Cache hit for Address ID {id}");
                    return JsonSerializer.Deserialize<AddressEntity>(cachedAddress);
                }

                // Fetch address from database
                var address = await _dbContext.AddressBooks.FirstOrDefaultAsync(c => c.Id == id && (email == null || c.UserEmail == email));

                // Store result in cache
                if (address != null)
                {
                    Logger.Info($"Cache miss for Address ID {id}, adding to cache.");
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(address), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
                }
                return address;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error fetching address ID {id}");
                return null;
            }
        }

        // Method to retrieve all addresses, with optional filtering by user email
        public async Task<List<Addresses>> GetAllAddressBooksRL(string? email)
        {
            try
            {
                string cacheKey = string.IsNullOrEmpty(email) ? "AllAddresses" : $"Addresses_{email}";
                var cachedAddresses = await _cache.GetStringAsync(cacheKey);

                // Return cached data if available
                if (!string.IsNullOrEmpty(cachedAddresses))
                {
                    Logger.Info($"Cache hit for {cacheKey}");
                    return JsonSerializer.Deserialize<List<Addresses>>(cachedAddresses) ?? new List<Addresses>();
                }

                // Fetch addresses from database
                var addressEntities = string.IsNullOrEmpty(email) ? await _dbContext.AddressBooks.ToListAsync() : await _dbContext.AddressBooks.Where(a => a.UserEmail == email).ToListAsync();
                var addresses = addressEntities.Select(MapToDTO).ToList();

                // Store result in cache
                if (addresses.Any())
                {
                    Logger.Info($"Cache miss for {cacheKey}, adding to cache.");
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(addresses));
                }

                return addresses;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error fetching all addresses");
                return new List<Addresses>();
            }
        }

        // Method to edit an existing address entry
        public async Task<Addresses?> EditAddressBookRL(string? email, int id, AddressEntity addressEntity)
        {
            // Find the existing address by ID and ensure it belongs to the given email
            var existingAddress = await _dbContext.AddressBooks.FirstOrDefaultAsync(a => a.Id == id);
            if (existingAddress == null || (!string.IsNullOrEmpty(email) && existingAddress.UserEmail != email)) return null;

            // Update address details
            existingAddress.FullName = addressEntity.FullName;
            existingAddress.Email = addressEntity.Email;
            existingAddress.PhoneNumber = addressEntity.PhoneNumber;
            existingAddress.Address = addressEntity.Address;

            await _dbContext.SaveChangesAsync();

            // Update cache with modified address and remove outdated cache
            string cacheKey = $"Address_{id}";
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(existingAddress));
            _cache.Remove("AllAddresses");

            return MapToDTO(existingAddress);
        }

        // Method to delete an address entry
        public bool DeleteAddressBookRL(string? email, int id)
        {
            // Find the address to delete
            var addressEntity = _dbContext.AddressBooks.FirstOrDefault(a => a.Id == id && (email == null || a.UserEmail == email));
            if (addressEntity == null) return false;

            _dbContext.AddressBooks.Remove(addressEntity);
            _dbContext.SaveChanges();

            // Remove related cache entries
            string userCacheKey = $"Addresses_{addressEntity.UserEmail}";
            _cache.Remove($"Address{id}");  // Remove the individual address cache
            _cache.Remove("AllAddresses");   // Remove the all-addresses cache
            _cache.Remove(userCacheKey);     // Remove the user-specific cache

            return true;
        }

        // Helper method to map AddressEntity to DTO
        public  Addresses MapToDTO(AddressEntity entity)
        {
            return new Addresses
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                UserEmail = entity.UserEmail
            };
        }

        // Method to refresh the cache with all addresses asynchronously
        public  async Task RefreshAllAddressesCache()
        {
            var addressEntities = await _dbContext.AddressBooks.ToListAsync();
            var addresses = addressEntities.Select(MapToDTO).ToList();

            if (addresses.Any())
            {
                await _cache.SetStringAsync("AllAddresses", JsonSerializer.Serialize(addresses));
            }
        }
    }
}