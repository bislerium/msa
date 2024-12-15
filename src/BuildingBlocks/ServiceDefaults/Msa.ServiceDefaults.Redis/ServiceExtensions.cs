using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msa.ServiceDefaults.Redis.Service;
using StackExchange.Redis;

namespace Msa.ServiceDefaults.Redis
{
    public static class RedisServiceExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration["Redis:Url"];
            ArgumentException.ThrowIfNullOrEmpty(redisConnectionString);

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddSingleton<ICacheService, CacheService>();

            return services;
        }
    }
}
