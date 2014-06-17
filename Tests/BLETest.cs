using NUnit.Framework;
using System;
using Tasky.BL.Managers;

namespace Tests
{
	[TestFixture ()]
	public class Test
	{
		[Test ()]
		public void TestHeartRateLocationByByte ()
		{
			Assert.AreEqual("Chest", BluetoothAssistant.GetBodySensorPosition (0x01));
		}

		[Test ()]
		public void TestHeartRateLocationByByteArray ()
		{
			Assert.AreEqual("Chest", BluetoothAssistant.GetBodySensorPosition (new byte[]{0x01}));
		}
	}
}

