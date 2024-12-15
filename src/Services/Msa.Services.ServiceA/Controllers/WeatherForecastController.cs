using Microsoft.AspNetCore.Mvc;
using Msa.Protos.Common;
using Msa.Services.ServiceA.Services;

namespace Msa.Services.ServiceA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(ILogger<WeatherForecastController> logger, WeatherForecastService weatherForecastService) : ControllerBase
    {

        [HttpGet(nameof(Expect.Success))]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetSuccessAsync(CancellationToken cancellationToken)
        {
            return Ok(await weatherForecastService.GetWeatherForecastsAsync(Expect.Success, cancellationToken));
        }

        [HttpGet(nameof(Expect.Failure))]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetFailureAsync(CancellationToken cancellationToken)
        {
            return Ok(await weatherForecastService.GetWeatherForecastsAsync(Expect.Failure, cancellationToken));
        }

        [HttpGet(nameof(Expect.Random))]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetRandomAsync(CancellationToken cancellationToken)
        {
            return Ok(await weatherForecastService.GetWeatherForecastsAsync(Expect.Random, cancellationToken));
        }
    }
}
