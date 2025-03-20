using System.Security.Claims;

namespace Middleware.JwtHelper
{
    public interface IJwtTokenHelper
    {
        /// <summary>
        /// Generates a JWT token for authentication.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="role">User's role (e.g., Admin, User)</param>
        /// <returns>JWT token as a string</returns>
        string GenerateToken(string email, string role);

        /// <summary>
        /// Generates a JWT token for password reset functionality.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>Password reset token as a string</returns>
        string GeneratePasswordResetToken(string email);

        /// <summary>
        /// Validates a JWT token and extracts claims.
        /// </summary>
        /// <param name="token">JWT token string</param>
        /// <returns>ClaimsPrincipal object containing user claims if the token is valid; otherwise, null</returns>
        ClaimsPrincipal ValidateToken(string token);
    }
}