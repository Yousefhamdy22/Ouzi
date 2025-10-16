using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfsces
{
    public interface IAddressRepository : IGenaricRepository<Address>
    {
        Task<Address> AddAsync(Address address);
        Task<bool> DeleteAsync(int id);

    }
}
