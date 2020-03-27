using Google.Protobuf;
using Impact.Samples.ProtobufOverHttp.Model;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Impact.Samples.ProtobufOverHttp.Consumer
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
			var stream = await httpClient.GetStreamAsync("/weatherforecast/" + city + "/" + days);
			return WeatherForecast.Parser.ParseFrom(stream);
		}
	}
}
