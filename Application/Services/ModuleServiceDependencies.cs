
using Application.Services.Implemntation;
using Application.Services.interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public static class ModuleServiceDependencies
    {
        public static IServiceCollection AddServiceDependencies(this IServiceCollection services)
        {
          
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryServices, CategoryService>();
            services.AddScoped<IFawaterakPaymentService, FawaterakPaymentService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserServce>();
            services.AddScoped<ICartServices, CartServices>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOrderDocumentService , OrderDocumentService>();

     
            return services;
        }
    }
}
