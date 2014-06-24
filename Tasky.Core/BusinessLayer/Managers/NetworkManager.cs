using System;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using Tasky.BL;
using System.Xml.Serialization;
//using System.Xml.Schema;

namespace Tasky.BL.Managers
{
	public class NetworkManager
	{
		public NetworkManager ()
		{
		}

		public event EventHandler NetworkDataLoaded = delegate {};

		public async System.Threading.Tasks.Task<string> RequestAndSaveWeather() {
			TaskManager.DeleteAllTasks ();
			WebRequest request = WebRequest.Create ("http://api.openweathermap.org/data/2.5/forecast?q=Bonn&mode=xml&APPID=7732b247cdb20b941ea963a87e8b8269");
			request.Method = "GET";
			request.ContentType = "application/xml";
			WebResponse response = await request.GetResponseAsync ();
			var dataStream = response.GetResponseStream ();
			StreamReader reader = new StreamReader (dataStream);
			string responseFromServer = reader.ReadToEnd ();
			reader.Close ();
			dataStream.Close ();
			response.Close ();

			//var doc = XDocument.Parse (responseFromServer);
			XmlSerializer serializer = new XmlSerializer(typeof(WeatherData));
			WeatherData weather;
			using (StringReader xreader = new StringReader (responseFromServer)) {
				weather = (WeatherData)(serializer.Deserialize (xreader));
				if (weather != null && weather.Forecast != null && weather.Forecast.Entries != null) {
					foreach (TimeEntry t in weather.Forecast.Entries) {
						TaskManager.SaveTask (new Task () { 
							Name = "From " + t.StartTime.ToShortTimeString () + " to " + t.EndTime.ToShortTimeString (),
							Notes = t.Symbol.Name + ", temperature is " + t.Temperature.Value.ToString (),
							Done = t.Temperature.Value > 15
						});
					}
				}
			}
			NetworkDataLoaded.Invoke (this, new EventArgs ());
			return "finished";
		}
	}
	[XmlRoot(ElementName="weatherdata")]
	public class WeatherData
	{
		[XmlElement(ElementName="forecast")]
		public Forecast Forecast { get; set;}

	}

	[XmlRoot(ElementName="forecast")]
	public class Forecast
	{
		[XmlElement(ElementName = "time")]
		public List<TimeEntry> Entries { get; set;}

		public Forecast()
		{
			Entries = new List<TimeEntry>();
		}
	}

	[XmlRoot(ElementName="time")]
	public class TimeEntry
	{
		[XmlAttribute(AttributeName="from")]
		public DateTime StartTime { get; set;}
		[XmlAttribute(AttributeName="to")]
		public DateTime EndTime {get; set;}
		[XmlElement(ElementName="symbol")]
		public Symbol Symbol { get; set;}
		[XmlElement(ElementName="temperature")]
		public Temperature Temperature { get; set;}
	}

	[XmlRoot(ElementName="symbol")]
	public class Symbol {
		[XmlAttribute(AttributeName="name")]
		public String Name;
	}

	[XmlRoot(ElementName="temperature")]
	public class Temperature {
		[XmlAttribute(AttributeName="value")]
		public float Value;
	}
}

