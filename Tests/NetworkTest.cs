using NUnit.Framework;
using System;
using Tasky.BL.Managers;

namespace Tests
{
	[TestFixture]
	public class NetworkTest
	{
		[SetUp]
		public void PrepareData()
		{
			RequestData ();
		}

		[Test]
		public void TestDBAfterRequest() {
			Assert.AreEqual (16, TaskManager.GetTasks ().Count);
		}

		public void RequestData() {
//			var nr = new NetworkManager().RequestAndSaveWeather ();
//			await nr;
		}
	}
}

