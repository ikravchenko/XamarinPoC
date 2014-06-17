using System;

namespace Tasky.BL.Managers
{
	public static class BluetoothAssistant
	{
		static BluetoothAssistant ()
		{
		}

		public static string HRS_SERVICE_UUID = "0000180D-0000-1000-8000-00805F9B34FB";
		public static string HRS_SENSOR_UUID = "00002A37-0000-1000-8000-00805F9B34FB";
		public static string HRS_SENSOR_LOCATION_UUID = "00002A38-0000-1000-8000-00805F9B34FB";

		public static String GetBodySensorPosition(byte value) {
			return GetBodySensorPosition(new byte[]{value});
		}

		public static String GetBodySensorPosition(byte[] bodySensorPositionValueArray) {
			if (bodySensorPositionValueArray == null) {
				return "cannot detect";
			}

			if (bodySensorPositionValueArray.Length == 0) {
				return "cannot detect, insuffucient data length";
			}

			byte bodySensorPositionValue = bodySensorPositionValueArray [0];
			if (bodySensorPositionValue == 0x00)
				return "Other";
			else if (bodySensorPositionValue == 0x01)
				return "Chest";
			else if (bodySensorPositionValue == 0x02)
				return "Wrist";
			else if (bodySensorPositionValue == 0x03)
				return "Finger";
			else if (bodySensorPositionValue == 0x04)
				return "Hand";
			else if (bodySensorPositionValue == 0x05)
				return "Ear Lobe";
			else if (bodySensorPositionValue == 0x06)
				return "Foot";
			return "reserved for future use";
		}
	}
}

