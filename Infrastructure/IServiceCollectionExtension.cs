
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddServiceLibrary(this IServiceCollection services)
        {
            services.AddSingleton<IJWTManagerRepository, JWTManagerRepository>();
            services.AddScoped<IFileHandleRepository, FileHandleRepository>();
            return services;
        }
    }
}
