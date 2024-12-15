using Microsoft.AspNetCore.Mvc;
using Msa.Extensions.Random;
using Msa.Protos.Common;
using Msa.Protos.Weather;

namespace Msa.Services.ServiceB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(ILogger<WeatherForecastController> logger, WeatherForecastService.WeatherForecastServiceClient weatherForecastServiceClient) : ControllerBase
    {
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<WeatherForecastListResponse> Get(Expect expect = Expect.Success)
        {
            logger.LogInformation("Requested WeatherForecast data for {Expected} Response!", nameof(expect));

            await Random.Shared.Delay();
            expect.Exception();

            var response = await weatherForecastServiceClient.GetWeatherForecastsAsync(new ResponseExpectationRequest()
            {
                Expect = expect
            });

            logger.LogInformation("Requested WeatherForecast data succeed with {NumForecastRecords} records!", response.Forecasts.Count);

            return response;
        }
    }
}
