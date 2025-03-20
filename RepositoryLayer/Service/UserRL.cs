using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // ✅ Correct Logging
using ModelLayer.DTO;
using ModelLayer.Entity;
using RepositoryLayer.Context;
using Middleware.HashingAlgo;
using RepositoryLayer.Interface;

namespace RepositoryLayer.Services
{
    public class UserRL : IUserRL
    {
        private readonly ILogger<UserRL> _logger; // ✅ Use ILogger<UserRL>
        private readonly AddressBookContext _dbContext;

        public UserRL(ILogger<UserRL> logger, AddressBookContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public UserEntity Registration(RegisterDTO registerDTO)
        {
            try
            {
                _logger.LogInformation("Attempting to register user: {Email}", registerDTO.Email);

                var existingUser = _dbContext.Users.FirstOrDefault(e => e.Email == registerDTO.Email);
                if (existingUser == null)
                {
                    var hashedPassword = HashingMethods.HashPassword(registerDTO.password); // ✅ Hash password

                    var newUser = new UserEntity
                    {
                        FirstName = registerDTO.firstName,
                        LastName = registerDTO.lastName,
                        Password = hashedPassword, // ✅ Store hashed password
                        Email = registerDTO.Email
                    };

                    _dbContext.Users.Add(newUser);
                    _dbContext.SaveChanges();

                    _logger.LogInformation("User registered successfully: {Email}", registerDTO.Email);
                    return newUser;
                }

                _logger.LogWarning("User already exists: {Email}", registerDTO.Email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", registerDTO.Email);
                throw;
            }
        }

        public UserEntity LoginnUserRL(LoginDTO loginDTO)
        {
            try
            {
                _logger.LogInformation("User attempting to log in: {Email}", loginDTO.Email);

                var user = _dbContext.Users.FirstOrDefault(e => e.Email == loginDTO.Email);
                if (user != null && HashingMethods.VerifyPassword(loginDTO.Password, user.Password)) // ✅ Verify hashed password
                {
                    _logger.LogInformation("Login successful for user: {Email}", loginDTO.Email);
                    return user;
                }

                _logger.LogWarning("Invalid login attempt for user: {Email}", loginDTO.Email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for {Email}", loginDTO.Email);
                throw;
            }
        }

        public bool ValidateEmail(string email)
        {
            var data = _dbContext.Users.FirstOrDefault(e => e.Email == email);

            if (data == null)
            {
                return false;
            }
            return true;
        }

        public UserEntity FindByEmail(string email)
        {
            return _dbContext.Users.FirstOrDefault(e => e.Email == email);
        }

        public bool Update(UserEntity user)
        {
            _dbContext.Users.Update(user);
            _dbContext.SaveChanges();
            return true;
        }
    }
}