using Application.DTOs;
using Application.Services.Implemntation;
using Application.Services.interfaces;
using Domain.Entities;
using Infrastructure.Interfsces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace WembyResturant.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService , IUserService userService,
            ILogger<AuthController> logger , ITokenService tokenService , UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _logger = logger;
            _tokenService = tokenService;
            _userService = userService;
            _userManager = userManager;
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterByPhone([FromBody] PhoneRegisterRequest request)
        {
            var (result, user) = await _userService.AddUserByPhoneAsync(
                request.PhoneNumber,
                request.Role,
                request.FirstName,
                request.LastName);

            return result switch
            {
                "Success" or "PhoneNumberExists" => Ok(new PhoneRegisterResponse
                {
                    Success = true,
                    Message = result == "Success" ? "Registration successful" : "Auto logged in",
                    UserId = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }),

                "InvalidPhoneNumber" => BadRequest(new { message = "Invalid phone number" }),
                "RoleNotFound" => BadRequest(new { message = "Role does not exist" }),
                _ => StatusCode(500, new { message = "Registration failed" })
            };
        }
        //[HttpPost("register")]
        //public async Task<IActionResult> RegisterByPhone([FromBody] PhoneRegisterRequest request)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Phone registration attempt: {PhoneNumber}", request.PhoneNumber);

        //        // Call your service method
        //        var (result, user) = await _userService.AddUserByPhoneAsync(request.PhoneNumber, request.Role);

        //        switch (result)
        //        {
        //            case "Success":
        //                _logger.LogInformation("User registered successfully: {PhoneNumber}", request.PhoneNumber);

        //                // Generate tokens for immediate login
        //                var token = await _tokenService.GenerateTokenAsync(user);
        //                var refreshToken =  _tokenService.GenerateRefreshToken();
        //                await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        //                return Ok(new PhoneRegisterResponse
        //                {
        //                    Success = true,
        //                    Message = "Registration successful",
        //                    //Token = token,
        //                    RefreshToken = refreshToken,
        //                    UserId = user.Id,
        //                    PhoneNumber = user.PhoneNumber,
        //                    ExpiresIn = DateTime.UtcNow.AddHours(2) // Match your token expiration
        //                });

        //            case "PhoneNumberExists":
        //                _logger.LogInformation("Phone number already registered: {PhoneNumber}", request.PhoneNumber);

        //                // Generate tokens for existing user (auto-login)
        //                var existingUserToken = await _tokenService.GenerateTokenAsync(user);
        //                var existingUserRefreshToken =  _tokenService.GenerateRefreshToken();
        //                await _tokenService.SaveRefreshTokenAsync(user.Id, existingUserRefreshToken);

        //                return Ok(new PhoneRegisterResponse
        //                {
        //                    Success = true,
        //                    Message = "User already exists - auto logged in",
        //                    Token = existingUserToken,
        //                    RefreshToken = existingUserRefreshToken,
        //                    UserId = user.Id,
        //                    CartId = user.CartId,
        //                    PhoneNumber = user.PhoneNumber,
        //                    ExpiresIn = DateTime.UtcNow.AddHours(2)
        //                });

        //            case "InvalidPhoneNumber":
        //                return BadRequest(new { message = "Invalid phone number format" });

        //            case "RoleNotFound":
        //                return BadRequest(new { message = "Specified role does not exist" });

        //            case "ErrorInCreateUser":
        //            case "RoleAssignmentFailed":
        //                return StatusCode(500, new { message = "Registration failed. Please try again." });

        //            case "ErrorOccurred":
        //            default:
        //                return StatusCode(500, new { message = "An unexpected error occurred" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error during phone registration: {PhoneNumber}", request.PhoneNumber);
        //        return StatusCode(500, new { message = "Internal server error" });
        //    }
        //}

        [HttpPost("login")]
        public async Task<IActionResult> LoginByPhone([FromBody] PhoneLoginRequest request)
        {
            try
            {
                _logger.LogInformation("Phone login attempt: {PhoneNumber}", request.PhoneNumber);

                // This will either find existing user or create new one
                var (result, user) = await _userService.GetUserByPhoneAsync(request.PhoneNumber);

                if (user == null)
                {
                    return BadRequest(new { message = "Login failed - unable to create or find user" });
                }

                // Generate tokens
                var token = await _tokenService.GenerateTokenAsync(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

                return Ok(new PhoneLoginResponse
                {
                    Success = true,
                    Message = result == "PhoneNumberExists" ? "Login successful" : "Registration and login successful",
                    Token = token,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    Emai = $"{user.PhoneNumber}@temp.com",
                    PhoneNumber = user.PhoneNumber,
                    IsNewUser = result == "Success", // Flag to indicate if user was just created
                    ExpiresIn = DateTime.UtcNow.AddHours(2)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during phone login: {PhoneNumber}", request.PhoneNumber);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
