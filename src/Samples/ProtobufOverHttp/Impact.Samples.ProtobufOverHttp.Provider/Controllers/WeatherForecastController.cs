using System;
using Google.Protobuf;
using Impact.Samples.ProtobufOverHttp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Impact.Samples.ProtobufOverHttp.Provider.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger)
		{
			_logger = logger;
		}

		[HttpGet("{city}/{days}")]
		public ActionResult Get(string city, int days)
		{
			var rng = new Random();
			var forecast = new WeatherForecast
			{
				City = city,
				Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(days)),
				TemperatureC = rng.Next(-20, 55),
				Summary = Summaries[rng.Next(Summaries.Length)]
			};

			forecast.TemperatureF = 32 + (int)(forecast.TemperatureC / 0.5556);

			return File(forecast.ToByteArray(), "application/octet-stream");
		}
	}
}
