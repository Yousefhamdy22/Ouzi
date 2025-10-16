using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Interfsces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AddressRepository :  GenericRepository<Address> , IAddressRepository
    {
        private readonly RestaurantDbContext _context;

        public AddressRepository(RestaurantDbContext context) : base(context)
        {
            _context = context;
        }

        private async Task<bool> UserExistsInCorrectFormat(string userId, Address address)
        {
            var formatsToTry = new List<string>
            {
                userId,
                userId.ToLower(),
                userId.ToUpper()
            };

            if (Guid.TryParse(userId, out var guid))
            {
                formatsToTry.AddRange(new[]
                {
                    guid.ToString("D"),
                    guid.ToString("D").ToLower(),
                    guid.ToString("D").ToUpper(),
                    guid.ToString("N"),
                    guid.ToString("N").ToLower(),
                    guid.ToString("N").ToUpper()
                });
            }

            foreach (var format in formatsToTry.Distinct())
            {
                var exists = await _context.Users.AnyAsync(u => u.Id == format);
                if (exists)
                {
                    address.UserId = format;
                    return true;
                }
            }

            return false;
        }

        public async Task<Address> AddAsync(Address address)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(address.Street))
                throw new ArgumentException("Street is required");

            if (string.IsNullOrWhiteSpace(address.City))
                throw new ArgumentException("City is required");

            if (string.IsNullOrWhiteSpace(address.UserId))
                throw new ArgumentException("UserId is required");

            // **FIXED: Better user existence check**
            var userExists = await UserExistsInCorrectFormat(address.UserId, address);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID '{address.UserId}' " +
                    $"does not exist. User must be registered before adding addresses.");
            }

            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
            return address;
        }



        public async Task<bool> DeleteAsync(int id)
        {
            var address = await GetByIdAsync(id);
            if (address == null) return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }

}
    }
