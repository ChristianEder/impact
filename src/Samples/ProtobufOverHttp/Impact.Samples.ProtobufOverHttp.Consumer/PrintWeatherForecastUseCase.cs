using Impact.Samples.ProtobufOverHttp.Consumer;
using System;
using System.Threading.Tasks;

namespace Impact.Samples.ProtobufOverHttp.Consumer
{
	public class PrintWeatherForecastUseCase
	{
		private readonly WeatherForecastApiClient apiClient;
		private readonly IConsole console;

		public PrintWeatherForecastUseCase(WeatherForecastApiClient apiClient, IConsole console)
		{
			this.apiClient = apiClient;
			this.console = console;
		}

		public async Task Print(string city)
		{
			var foreCast = await apiClient.Get(city, 7);
			console.Print($"The weather in {foreCast.City} on {foreCast.Date.ToDateTime().ToShortDateString()} will be {foreCast.Summary} at {foreCast.TemperatureC}°C");
		}
	}
}
