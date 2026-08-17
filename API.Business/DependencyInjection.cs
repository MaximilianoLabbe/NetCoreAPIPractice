using API.Business.Interfaces;
using API.Business.Mappings;
using API.Business.Services;
using API.Data;
using Microsoft.Extensions.DependencyInjection;

namespace API.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDataLayer(connectionString);

        services.AddAutoMapper(
            cfg => { },
            typeof(ProductProfile)
        );

        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}