using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfsces
{
    public interface IUserRepository : IGenaricRepository<ApplicationUser>
    {
        Task<ApplicationUser> GetUserWithAddressesAsync(string userId);
        Task<ApplicationUser> GetUserWithCartAsync(string userId);
        Task<bool> ExistsAsync(string userId);
    }
}
