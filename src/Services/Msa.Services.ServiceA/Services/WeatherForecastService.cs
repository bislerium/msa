using Msa.Protos.Common;
using Msa.ServiceDefaults.Redis.Service;
using System.Text.Json;

namespace Msa.Services.ServiceA.Services
{
    public class WeatherForecastService(IHttpClientFactory httpClientFactory,
        Protos.Actor.ActorService.ActorServiceClient actorServiceClient,
        ILogger<WeatherForecastService> logger,
        ICacheService redisService)
    {
        private const string WeatherForecastKey = "WeatherForecast";
        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastsAsync(Expect expect, CancellationToken cancellationToken)
        {
            logger.LogInformation("WeatherForecast Request received witb Paramaeter {Exppect}!", expect);

            if (expect == Expect.Failure) await redisService.RemoveDataAsync(WeatherForecastKey);

            return await redisService.GetDataAsync(WeatherForecastKey, async () =>
            {
                await actorServiceClient.ActAsync(new ResponseExpectationRequest()
                {
                    Expect = expect
                }, cancellationToken: cancellationToken);

                using var serviceB = httpClientFactory.CreateClient("ServiceB");
                using var response = await serviceB.GetAsync($"/WeatherForecast?expect={(int)expect}", cancellationToken);
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var jsondoc = JsonDocument.Parse(stream);
                return jsondoc.RootElement.GetProperty("forecasts").Deserialize<IEnumerable<WeatherForecast>>() ?? [];
            });
        }
    }
}
