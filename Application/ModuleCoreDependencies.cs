using AutoMapper;
using Microsoft.Extensions.DependencyInjection;


namespace Application
{
    public static class ModuleCoreDependencies
    {
        public static IServiceCollection AddModuleCoreDependencies(this IServiceCollection services)
        {
            //services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IMapper, Mapper>();

            return services;
        }
    }
}
