using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTO;
using NLog;
using BusinessLayer.Interface;
using Middleware.JwtHelper;
using Middleware.Email;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ModelLayer.Model;
using Middleware.HashingAlgo;
using Middleware.RabbitMQClient;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly IUserBL _userBL;
    private readonly IJwtTokenHelper _jwtTokenHelper;
    private readonly SMTP _smtp;
    private readonly IRabbitMQService _rabbitMQService;

    public AuthController(IUserBL userBL, IJwtTokenHelper jwtTokenHelper, SMTP smtp, IRabbitMQService rabbitMQService)
    {
        _userBL = userBL ?? throw new ArgumentNullException(nameof(userBL), "UserBL cannot be null.");
        _jwtTokenHelper = jwtTokenHelper;
        _smtp = smtp;
        _rabbitMQService = rabbitMQService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDTO registerDTO)
    {
        if (registerDTO == null) return BadRequest(new { Success = false, Message = "Invalid request data." });

        try
        {
            _logger.Info($"Register attempt for email: {registerDTO.Email}");

            var newUser = _userBL.RegistrationBL(registerDTO);
            if (newUser == null)
            {
                _logger.Warn($"Registration failed. Email already exists: {registerDTO.Email}");
                return Conflict(new { Success = false, Message = "User with this email already exists." });
            }

            _logger.Info($"User registered successfully: {registerDTO.Email}");
            _rabbitMQService.SendMessage($"{registerDTO.Email}, You have successfully Registered!");
            return Created("user registered", new { Success = true, Message = "User registered successfully." });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Registration failed for {registerDTO.Email}");
            return StatusCode(500, new { Success = false, Message = "Internal Server Error." });
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO loginDTO)
    {
        if (loginDTO == null) return BadRequest(new { Success = false, Message = "Invalid request data." });

        try
        {
            _logger.Info($"Login attempt for user: {loginDTO.Email}");

            var (user, token) = _userBL.LoginnUserBL(loginDTO);
            if (user == null || string.IsNullOrEmpty(token))
            {
                _logger.Warn($"Invalid login attempt for user: {loginDTO.Email}");
                return Unauthorized(new { Success = false, Message = "Invalid username or password." });
            }

            _logger.Info($"User {loginDTO.Email} logged in successfully.");
            _rabbitMQService.ReceiveMessage();

            return Ok(new { Success = true, Message = "Login Successful.", Token = token });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Login failed for {loginDTO.Email}");
            return StatusCode(500, new { Success = false, Message = "Internal Server Error." });
        }
    }

    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordDTO request)
    {
        if (string.IsNullOrEmpty(request.Email)) return BadRequest(new { Message = "Email is required." });

        try
        {
            if (!_userBL.ValidateEmail(request.Email)) return Ok(new { Message = "Not a valid email" });

            var resetToken = _jwtTokenHelper.GeneratePasswordResetToken(request.Email);
            string subject = "Reset Your Password";
            string body = $"Click the link to reset your password: \n https://AdressBook.com/reset-password?token={resetToken}";

            _smtp.SendEmailAsync(request.Email, subject, body);
            return Ok(new { Message = "Password reset email has been sent." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "Error occurred while processing the password reset", Error = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordDTO model)
    {
        if (string.IsNullOrEmpty(model.Token)) return BadRequest(new { Success = false, Message = "Invalid token." });

        try
        {
            var principal = _jwtTokenHelper.ValidateToken(model.Token);
            if (principal == null) return BadRequest(new { Success = false, Message = "Invalid or expired token." });

            var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value
                          ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            if (string.IsNullOrEmpty(emailClaim) || emailClaim != model.Email)
                return BadRequest(new { Success = false, Message = "Invalid Email." });

            var user = _userBL.GetByEmail(emailClaim);
            if (user == null) return NotFound(new { Success = false, Message = "User not found" });

            _userBL.UpdateUserPassword(model.Email, HashingMethods.HashPassword(model.NewPassword));
            return Ok(new { Success = true, Message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}