using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record UserDto(
    string Id,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    string Role);


    public record AuthResponse(
    UserDto User,
    string Token,
    string RefreshToken,
    DateTime Expiration);
    public class AuthResult
    {
        public bool IsSuccess { get; private set; }
        public string? Message { get; private set; }
        public string? Token { get; private set; }
        public string? RefreshToken { get; private set; }
        public ApplicationUser User { get; private set; }

        public static AuthResult Success(string token, string refreshToken, ApplicationUser user)
        {
            return new AuthResult
            {
                IsSuccess = true,
                Token = token,
                RefreshToken = refreshToken,
                User = user
            };
        }

        public static AuthResult Fail(string message)
        {
            return new AuthResult
            {
                IsSuccess = false,
                Message = message
            };
        }
    }



    public class PhoneRegisterRequest
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        public UserRole Role { get; set; } = UserRole.Customer;
    }

    public class PhoneLoginRequest
    {
        [Required]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }
    }

    public class PhoneRegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string UserId { get; set; }

        public int? CartId { get; set; }

        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime ExpiresIn { get; set; }
    }

    public class PhoneLoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string UserId { get; set; }
        public string Emai { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsNewUser { get; set; }
        public DateTime ExpiresIn { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
public enum UserRole
{
    Admin =1,
    User =2,
    Customer =3

}