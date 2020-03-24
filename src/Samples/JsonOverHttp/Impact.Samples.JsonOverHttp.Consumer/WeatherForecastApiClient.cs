using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Impact.Samples.JsonOverHttp.Consumer
{
	public class WeatherForecastApiClient
	{
		private HttpClient httpClient;

		public WeatherForecastApiClient(HttpClient httpClient)
		{
			this.httpClient = httpClient;
		}

		public async Task<WeatherForecast> Get(string city, int days)
		{
			var json = await httpClient.GetStringAsync("/weatherforecast/" + city + "/" + days);
			return JsonConvert.DeserializeObject<WeatherForecast>(json);
		}
	}
}
