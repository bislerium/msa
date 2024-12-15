namespace Msa.ServiceDefaults.Redis.Service
{
    public interface ICacheService
    {
        T? GetData<T>(string key) where T : class;
        Task<T?> GetDataAsync<T>(string key) where T : class;
        Task<T> GetDataAsync<T>(string key, Func<Task<T>> factory, TimeSpan? lifeSpan = null) where T : class;
        Task SetDataAsync<T>(string key, T value, TimeSpan? lifeSpan = null) where T : class;
        Task SetStringAsync(string key, string value, TimeSpan? lifeSpan = null);
        Task<bool> RemoveDataAsync(string key);
        Task<bool> DoesKeyExistAsync(string key);
    }
}
