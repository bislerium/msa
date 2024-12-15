using Grpc.Core;
using Msa.Extensions.Random;
using Msa.Protos.Common;
using Msa.Protos.Weather;

namespace Msa.Services.ServiceC.Services
{
    public class WeatherForecastService(ILogger<WeatherForecastService> logger) : Protos.Weather.WeatherForecastService.WeatherForecastServiceBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        public override async Task<WeatherForecastListResponse> GetWeatherForecasts(ResponseExpectationRequest request, ServerCallContext context)
        {
            logger.LogInformation("WeatherForecast data request received and is being processed!");

            await Random.Shared.Delay();

            request.Expect.Exception();

            var response = GenerateWeatherForecastListResponse();

            logger.LogInformation($"WeatherForecast data is processed and ready for response!");

            return response;
        }

        private WeatherForecastListResponse GenerateWeatherForecastListResponse()
        {
            var response = new WeatherForecastListResponse();
            response.Forecasts.AddRange(Enumerable.Range(0, Random.Shared.Next(30)).Select(index =>
            {
                var data = new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)).ToString("O"),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };
                logger.LogInformation("Weather Data: {WeatherData}", data);
                return data;
            }).ToArray());
            return response;
        }
    }
}
