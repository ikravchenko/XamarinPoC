using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Tasky.BL.Managers;
using Tasky.Droid;

namespace BluetoothLEExplorer.Droid.Screens.Scanner.ServiceDetails
{
	[Activity (Label = "Characteristics")]			
	public class ServiceDetailsScreen : ListActivity
	{
		protected List<string> _characteristics;
		private bool hrEnabled = false;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.ListAdapter = new CharachteristicAdapter (this, App.Current.State.SelectedService.Characteristics);
			ActionBar.SetDisplayHomeAsUpEnabled (true);
			ActionBar.SetHomeButtonEnabled (true);
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.menu_ble_service, menu);
			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Resource.Id.menu_ble_read) {
				foreach (var c in App.Current.State.SelectedService.Characteristics) {
					BluetoothLEManager.Current.ReadCharacteristic (App.Current.State.SelectedDevice, c);
					if (c.Uuid.Equals (UUID.FromString (BluetoothAssistant.HRS_SENSOR_UUID))) {
						if (!hrEnabled) {
							EnableHRNotification (c);
							hrEnabled = true;
						}
					}
				}
			} else {
				Finish ();
			}
			return true;
		}

		private void EnableHRNotification(BluetoothGattCharacteristic c) {
			BluetoothLEManager.Current.SetCharacteristicNotification(App.Current.State.SelectedDevice, c, true);
			BluetoothGattDescriptor descriptor = c.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
			descriptor.SetValue(new byte[]{0x01, 0x00});
			BluetoothLEManager.Current.WriteDescriptor(App.Current.State.SelectedDevice, descriptor);
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			BluetoothLEManager.Current.CharachteristicChanged += ReloadData;
		}

		private void ReloadData(object sender, EventArgs args) {
			RunOnUiThread (delegate {
				((CharachteristicAdapter)ListView.Adapter).NotifyDataSetChanged();
			});
		}

		protected override void OnPause ()
		{
			BluetoothLEManager.Current.CharachteristicChanged -= ReloadData;
			base.OnPause ();
		}

		public class CharachteristicAdapter : GenericAdapterBase<BluetoothGattCharacteristic>
		{
			public CharachteristicAdapter (Activity context, IList<BluetoothGattCharacteristic> items) 
				: base(context, Android.Resource.Layout.SimpleListItem2, items)
			{

			}

			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				View view = convertView; // re-use an existing view, if one is available
				if (view == null) // otherwise create a new one
					view = context.LayoutInflater.Inflate (resource, null);
				view.FindViewById<TextView> (Android.Resource.Id.Text1).Text = "Name:" + items [position].Uuid;
				view.FindViewById<TextView> (Android.Resource.Id.Text2).Text = "Value: ";

				if (items[position].Uuid.Equals(UUID.FromString(BluetoothAssistant.HRS_SENSOR_UUID))) {
					view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = "Heart Rate Charachteristic";
					view.FindViewById<TextView> (Android.Resource.Id.Text2).Text = "Value: " + GetHeartRate(items[position]);
				} else if (items[position].Uuid.Equals(UUID.FromString(BluetoothAssistant.HRS_SENSOR_LOCATION_UUID))) {
					view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = "Sensor Location Charachteristic";
					view.FindViewById<TextView> (Android.Resource.Id.Text2).Text = "Location: " + BluetoothAssistant.GetBodySensorPosition(items[position].GetValue());
				}
				return view;
			}
		}

		public static string GetHeartRate(BluetoothGattCharacteristic characteristic) {
			if (characteristic.GetValue () == null || characteristic.GetValue ().Length == 0) {
				return "insufficient data";
			}
			if (isHeartRateInUINT16(characteristic.GetValue()[0])) {
				return "" + characteristic.GetIntValue(GattFormat.Uint16, 1);
			} else {
				return "" + characteristic.GetIntValue(GattFormat.Uint8, 1);
			}
		}

		private static bool isHeartRateInUINT16(byte value) {
			return (value & 0x01) != 0;
		}
	}
}

