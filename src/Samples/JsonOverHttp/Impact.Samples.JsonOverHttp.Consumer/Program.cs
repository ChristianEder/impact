using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Impact.Samples.JsonOverHttp.Consumer
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri("http://localhost:60374");
			var apiClient = new WeatherForecastApiClient(httpClient);
			var useCase = new PrintWeatherForecastUseCase(apiClient, new StandardOut());
			await useCase.Print(args[0]);
			Console.ReadLine();
		}
	}
}
