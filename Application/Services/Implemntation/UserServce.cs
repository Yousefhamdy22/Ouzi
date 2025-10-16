using Application.DTOs;
using Application.Helper;
using Application.Services.interfaces;
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entities;
using Infrastructure.Interfsces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;


namespace Application.Services.Implemntation
{
    public class UserServce : IUserService
    {

        #region Ctors&DI
        private readonly ILogger<UserServce> _logger;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public UserServce(ILogger<UserServce> logger , IUserRepository userRepository
        , UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager ,
           IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _logger = logger;
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;

        }




        #endregion


        #region Register&Login

        public async Task<(string result, ApplicationUser user)> AddUserByPhoneAsync(
                 string phoneNumber,
                 UserRole role = UserRole.Customer,
                 string firstName = null,
                 string lastName = null)
        {
            try
            {
                // 1. Validate input
                if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10)
                    return ("InvalidPhoneNumber", null);

                var cleanPhone = NormalizePhoneNumber(phoneNumber);

                // 2. Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(cleanPhone);
                if (existingUser != null)
                    return ("PhoneNumberExists", existingUser);

                // 3. Create new user (Let Identity generate the ID automatically)
                var user = new ApplicationUser
                {
                    UserName = cleanPhone,           // Use phone as username
                    PhoneNumber = cleanPhone,
                    PhoneNumberConfirmed = true,
                    Email = $"{cleanPhone}@temp.com",
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedDate = DateTime.UtcNow
                };

                // 4. Create user account
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    _logger.LogError("User creation failed: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return ("ErrorInCreateUser", null);
                }

                // 5. Assign role (if role exists)
                if (await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _userManager.AddToRoleAsync(user, role.ToString());
                }

                _logger.LogInformation("User registered successfully: {PhoneNumber} (ID: {UserId})",
                    cleanPhone, user.Id);

                return ("Success", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for: {PhoneNumber}", phoneNumber);
                return ("ErrorOccurred", null);
            }
        }
        //public async Task<(string result, ApplicationUser user)> AddUserByPhoneAsync(string phoneNumber, UserRole role )
        //{
        //    try
        //    {
        //        _logger.LogInformation("Starting user creation for phone: {PhoneNumber}", phoneNumber);

        //        // Validate phone number
        //        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10)
        //        {
        //            _logger.LogWarning("Invalid phone number: {PhoneNumber}", phoneNumber);
        //            return ("InvalidPhoneNumber", null);
        //        }

        //        // Clean and normalize phone number
        //        var cleanPhone = NormalizePhoneNumber(phoneNumber);

        //        // Check if phone number exists
        //        var existingUser = await _userManager.Users
        //            .FirstOrDefaultAsync(u => u.PhoneNumber == cleanPhone);

        //        if (existingUser != null)
        //        {
        //            _logger.LogWarning("Phone number already exists: {PhoneNumber}", cleanPhone);
        //            return ("PhoneNumberExists", existingUser); // Return existing user
        //        }

        //        // Ensure role exists
        //        var roleExists = await _roleManager.RoleExistsAsync(role.ToString());
        //        if (!roleExists)
        //        {
        //            _logger.LogError("Role {Role} does not exist", role.ToString());
        //            return ("RoleNotFound", null);
        //        }

        //        // Create new user with phone number
        //        var user = new ApplicationUser
        //        {
        //            UserName = $"user_{cleanPhone}", 
        //            PhoneNumber = cleanPhone,
        //            PhoneNumberConfirmed = true, 
        //            Email = $"{cleanPhone}@temp.com", 
        //            EmailConfirmed = false,
        //            CreatedDate = DateTime.UtcNow
        //        };

        //        // Create user without password
        //        _logger.LogInformation("Creating user account for phone: {PhoneNumber}", cleanPhone);
        //        var result = await _userManager.CreateAsync(user);

        //        if (!result.Succeeded)
        //        {
        //            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        //            _logger.LogError("User creation failed for {PhoneNumber}: {Errors}", cleanPhone, errors);
        //            return ("ErrorInCreateUser", null);
        //        }

        //        _logger.LogInformation("User created successfully: {PhoneNumber}", cleanPhone);

        //        // Assign role
        //        _logger.LogInformation("Assigning role {Role} to user: {PhoneNumber}", role.ToString(), cleanPhone);
        //        var roleResult = await _userManager.AddToRoleAsync(user, role.ToString());

        //        if (!roleResult.Succeeded)
        //        {
        //            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
        //            _logger.LogError("Role assignment failed for {PhoneNumber}: {Errors}", cleanPhone, roleErrors);
        //            return ("RoleAssignmentFailed", user);
        //        }

        //        _logger.LogInformation("User registration completed successfully: {PhoneNumber}", cleanPhone);
        //        return ("Success", user);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Critical error during user registration for phone: {PhoneNumber}", phoneNumber);
        //        return ("ErrorOccurred", null);
        //    }
        //}

        // Add this method to your UserService


        public async Task<(string Result, ApplicationUser User)> GetUserByPhoneAsync(string phoneNumber)
        {
            try
            {
                // Normalize the phone number (remove spaces, dashes, etc.)
                var normalizedPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());

                _logger.LogInformation("Original phone: {Phone}, Normalized: {Normalized}",
                    phoneNumber, normalizedPhone);

                // Check with exact match first
                var exactMatchUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

                _logger.LogInformation("Exact match result: {Result}", exactMatchUser != null ? "FOUND" : "NOT FOUND");

                // If not found, try with normalized version
                if (exactMatchUser == null)
                {
                    var normalizedMatchUser = await _userManager.Users
                        .FirstOrDefaultAsync(u =>
                            u.PhoneNumber != null &&
                            new string(u.PhoneNumber.Where(char.IsDigit).ToArray()) == normalizedPhone);

                    _logger.LogInformation("Normalized match result: {Result}", normalizedMatchUser != null ? "FOUND" : "NOT FOUND");

                    if (normalizedMatchUser != null)
                    {
                        return ("Success", normalizedMatchUser);
                    }
                }
                else
                {
                    return ("Success", exactMatchUser);
                }

                return ("UserNotFound", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserByPhoneAsync for phone: {PhoneNumber}", phoneNumber);
                return ("ErrorOccurred", null);
            }

        }
        //public async Task<(string Result, ApplicationUser User)> GetOrCreateUserByPhoneAsync(string phoneNumber)
        //{
        //    try
        //    {
        //        // First try to find existing user
        //        var existingUser = await _userManager.FindByNameAsync(phoneNumber);
        //        if (existingUser != null)
        //        {
        //            return ("PhoneNumberExists", existingUser);
        //        }

        //        // If user doesn't exist, create with default role
        //        //var user = new ApplicationUser { UserName = phoneNumber, PhoneNumber = phoneNumber ,
        //        //    Email = $"{phoneNumber}@temp.com"

        //        //};
        //        //var createResult = await _userManager.CreateAsync(user);

        //        //if (!createResult.Succeeded)
        //        //{
        //        //    _logger.LogError("Error creating user: {Errors}", string.Join(", ", createResult.Errors));
        //        //    return ("ErrorInCreateUser", null);
        //        //}

        //        // Assign default role (e.g., "User")
        //        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        //        if (!roleResult.Succeeded)
        //        {
        //            _logger.LogWarning("Failed to assign default role to user: {Errors}",
        //                string.Join(", ", roleResult.Errors));
        //            // Continue anyway since user was created successfully
        //        }

        //        return ("Success", user);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in GetOrCreateUserByPhoneAsync for phone: {PhoneNumber}", phoneNumber);
        //        return ("ErrorOccurred", null);
        //    }
        //}

        #region HelperMethod

        private string NormalizePhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters
            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }

        //public string GetCurrentUserId()
        //{
           
        //    return _httpContextAccessor.HttpContext?.User?
        //        .FindFirst(ClaimTypes.NameIdentifier)?.Value
        //        ?? throw new UnauthorizedAccessException("User must be logged in");
        //}
        #endregion


        #endregion

        public async Task<ApplicationUser> GetUserByIdAsync(Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            return await _userRepository.ExistsAsync(userId);
        }

        public async Task<IEnumerable<Address>> GetUserAddressesAsync(string userId, 
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserWithAddressesAsync(userId);
            return user?.Addresses ?? Enumerable.Empty<Address>();
        }
        
        public Task<Response<string>> GetUserAddressAsync(string userId, AddressDto addressDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (addressDto == null)
                    throw new ArgumentNullException(nameof(addressDto));

                // Basic validation on the DTO properties
                if (string.IsNullOrWhiteSpace(addressDto.Street))
                    throw new ArgumentException("Street address is required.", nameof(addressDto));

                if (string.IsNullOrWhiteSpace(addressDto.City))
                    throw new ArgumentException("City is required.", nameof(addressDto));

                // Build the address string
                var addressParts = new List<string> { addressDto.Street };

                //if (!string.IsNullOrWhiteSpace(addressDto.Building))
                //    addressParts.Add($"Building: {addressDto.Building}");

                addressParts.AddRange(new[]
                {
                addressDto.City,

            }.Where(part => !string.IsNullOrWhiteSpace(part)));

                var formattedAddress = string.Join(", ", addressParts);
                _logger.LogDebug("Formatted address: {FormattedAddress}", formattedAddress);

                //return Task.FromResult(formattedAddress);

                var errorResponse = new Response<string>
                {
                    Data = null,
                    Success = false,
                    //Message = ex.Message
                };

                return Task.FromResult(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to format address for DTO: {@AddressDto}", addressDto);
                throw;
            }
        }

        public Task<string> AddUserAsync(ApplicationUser user, string password, UserRole role)
        {
            throw new NotImplementedException();
        }

        

        //public Guid GetCurrentUserId()
        //{
        //    var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        //    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        //    {
        //        throw new UnauthorizedAccessException("User is not authenticated");
        //    }

        //    return userId;
        //}
        public Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            // Try to parse as Guid
            if (Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }

            // If it's already a string representation, you might need to handle differently
            throw new InvalidOperationException("User ID is not in valid Guid format");
        }
        public Task<Response<string>> GetUserAddressAsync(Guid userId, AddressDto addressDto, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserExistsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
