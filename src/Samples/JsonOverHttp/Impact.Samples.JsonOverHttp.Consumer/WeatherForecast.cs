using System;

namespace Impact.Samples.JsonOverHttp.Consumer
{
	public class WeatherForecast
	{
		public string City { get; set; }

		public DateTime Date { get; set; }

		public int TemperatureC { get; set; }

		public int TemperatureF { get; set; }

		public string Summary { get; set; }
	}
}
