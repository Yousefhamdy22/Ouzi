using Application.DTOs;
using Application.Helper;
using Azure.Core;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.interfaces
{
    public  interface IUserService
    {

       // Task<(string result, ApplicationUser user)> AddUserByPhoneAsync(string phoneNumber , string firstName ,v, UserRole role );
        Task<(string result, ApplicationUser user)> AddUserByPhoneAsync(
                string phoneNumber,
                UserRole role ,
                string firstName = null,
                string lastName = null);
        Task<string> AddUserAsync(ApplicationUser user, string password, UserRole role);

        Task<(string Result, ApplicationUser User)> GetUserByPhoneAsync(string phoneNumber);
        //Task<UserDto?> GetUserByIdAsync(string userId);
        Guid GetCurrentUserId();
        Task<Response<string>> GetUserAddressAsync(Guid userId , AddressDto addressDto ,
            CancellationToken cancellationToken = default);
        Task<ApplicationUser> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> UserExistsAsync(Guid userId);

        //Task<Address> AddUserAddressAsync(string userId, AddressDto addressDto,
        //     CancellationToken cancellationToken = default);
        Task<IEnumerable<Address>> GetUserAddressesAsync(string userId,
             CancellationToken cancellationToken = default);


    }
}
