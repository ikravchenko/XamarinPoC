using NUnit.Framework;
using System;
using Tasky.BL.Managers;

namespace Tests
{
	[TestFixture]
	public class NetworkTest
	{
		[Test]
		public void TestDBAfterRequest() {
			new NetworkManager ().RequestAndSaveWeather ();
			Assert.AreEqual (16, TaskManager.GetTasks ().Count);
		}
	}
}

