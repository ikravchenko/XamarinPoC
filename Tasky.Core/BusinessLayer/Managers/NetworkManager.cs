using System;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using Tasky.BL;
//using System.Threading.Tasks;

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

			XElement xelement = XElement.Parse (responseFromServer);
			var data = xelement.Element ("forecast");
			IEnumerable<XElement> forecasts = data.Elements ();
			foreach (var item in forecasts) {
				var newTask = new Task ();
				newTask.Name = item.Element ("symbol").Attribute ("name").Value.ToString ();
				float temperature = float.Parse (item.Element ("temperature").Attribute ("value").Value, System.Globalization.CultureInfo.InvariantCulture);
				newTask.Notes = temperature.ToString ();
				newTask.Done = temperature > 15;
				TaskManager.SaveTask (newTask);
			}
			NetworkDataLoaded.Invoke (this, new EventArgs ());
			return "finished";
		}
	}
}

