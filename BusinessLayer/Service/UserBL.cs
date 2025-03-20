using System;
using ModelLayer.DTO;
using Microsoft.Extensions.Logging; // ✅ Use Microsoft ILogger
using ModelLayer.Entity;
using RepositoryLayer.Interface;
using BusinessLayer.Interface;
using Middleware.JwtHelper; // ✅ Added JWT Helper

namespace BusinessLayer.Service;
public class UserBL : IUserBL
{
    private readonly ILogger<UserBL> _logger; // ✅ Correct logging
    private readonly IUserRL _userRL;
    private readonly IJwtTokenHelper _jwtTokenHelper; // ✅ JWT Helper added

    public UserBL(IUserRL userRL, ILogger<UserBL> logger, IJwtTokenHelper jwtTokenHelper)
    {
        _logger = logger;
        _userRL = userRL;
        _jwtTokenHelper = jwtTokenHelper; // ✅ Assign JWT Helper
    }

    public UserEntity RegistrationBL(RegisterDTO registerDTO)
    {
        try
        {
            _logger.LogInformation("Attempting to register user: {Email}", registerDTO.Email);

            var result = _userRL.Registration(registerDTO);
            if (result != null)
            {
                _logger.LogInformation("User registration successful for: {Email}", registerDTO.Email);
            }
            else
            {
                _logger.LogWarning("User registration failed for: {Email}", registerDTO.Email);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", registerDTO.Email);
            throw;
        }
    }

    public (UserEntity user, string token) LoginnUserBL(LoginDTO loginDTO)
    {
        try
        {
            _logger.LogInformation("Attempting to log in user: {Email}", loginDTO.Email);

            var user = _userRL.LoginnUserRL(loginDTO);
            if (user != null)
            {
                _logger.LogInformation("Login successful for user: {Email}", loginDTO.Email);

                // ✅ Generate JWT token
                var token = _jwtTokenHelper.GenerateToken(user.Email, user.Role);
                return (user, token);
            }

            _logger.LogWarning("Login failed for user: {Email}", loginDTO.Email);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", loginDTO.Email);
            throw;
        }
    }


    public bool UpdateUserPassword(string email, string newPassword)
    {
        // Lookup user by email
        var user = _userRL.FindByEmail(email);
        if (user == null) return false;

        // Hash and update the password
        user.Password = newPassword;
        return _userRL.Update(user);
    }

    public UserEntity GetByEmail(string email)
    {
        return _userRL.FindByEmail(email);
    }

    public bool ValidateEmail(string email)
    {
        return _userRL.ValidateEmail(email);
    }
}