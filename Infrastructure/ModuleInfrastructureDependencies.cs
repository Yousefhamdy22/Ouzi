using Domain.Interfaces;
using Infrastructure.Interfsces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure
{
    public static class ModuleInfrastructureDependencies
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
        {
            services.AddScoped<ICategory, CategoryRepository>();
            services.AddScoped<IProduct, ProductRepository>();
            services.AddScoped<ICart, CartRepository>();
            services.AddScoped<ICartItems, CartItemRepository>();
            services.AddScoped<IOrder, OrderRepository>();
            services.AddScoped<IOrderItems , OrderItemsRepository >();
            services.AddScoped<IUserRepository , UserRepository >();
            services.AddScoped<IAddressRepository , AddressRepository >();
            services.AddScoped<ITokenService , TokenService >();
            services.AddScoped<IRefreshTokenRepository , RefreshTokenRepository>();



            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenaricRepository<>), typeof(GenericRepository<>));

            
            return services;


        }


    }
}
