using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using BluetoothLEExplorer.Droid.UI.Controls;
using Android.Bluetooth;
using BluetoothLEExplorer.Droid.UI.Adapters;
using Tasky.Droid;

namespace BluetoothLEExplorer.Droid.Screens.Scanner.Home
{
	[Activity (Label = "Scanner")]			
	public class ScannerHome : Activity
	{
		public const int BLE_ACTIVATION_CODE = 2;
		protected ListView listView;
		protected ScanButton scanButton;
		protected DevicesAdapter listAdapter;
		protected ProgressDialog progress;
		protected BluetoothDevice deviceToConnect; //not using State.SelectedDevice because it may not be connected yet

		// external handlers
		EventHandler<BluetoothLEManager.DeviceDiscoveredEventArgs> deviceDiscoveredHandler;
		EventHandler<BluetoothLEManager.DeviceConnectionEventArgs> deviceConnectedHandler;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ScannerHome);
			this.listView = FindViewById<ListView> (Resource.Id.DevicesTable);
			this.scanButton = FindViewById<ScanButton> (Resource.Id.ScanButton);

			this.listAdapter = new DevicesAdapter(this, BluetoothLEManager.Current.DiscoveredDevices);
			this.listView.Adapter = this.listAdapter;

			ActionBar.SetDisplayHomeAsUpEnabled(true);
			ActionBar.SetHomeButtonEnabled(true);
		}
		
		protected override void OnResume ()
		{
			base.OnResume ();
			
			this.WireupLocalHandlers ();
			this.WireupExternalHandlers ();
		}
		
		protected override void OnPause ()
		{
			base.OnPause ();
			this.StopScanning ();
			this.RemoveExternalHandlers ();
		}

		protected void WireupLocalHandlers ()
		{
			this.scanButton.Click += (object sender, EventArgs e) => {
				if (!BluetoothLEManager.Current.IsEnabled()) {
					StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), BLE_ACTIVATION_CODE);
				} else {
					ToggleScanning ();
				}
			};

			this.listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Console.Write ("ItemClick: " + this.listAdapter.Items[e.Position]);
				this.StopScanning();

				this.listView.ClearFocus();
				this.listView.Post( () => {
					this.listView.SetSelection (e.Position);
				});
				this.deviceToConnect = this.listAdapter.Items[e.Position];
				this.RunOnUiThread( () => {	
					this.progress = ProgressDialog.Show(this, "Connecting", "Connecting to " + this.deviceToConnect.Name, true);
				});
				BluetoothLEManager.Current.ConnectToDevice ( this.listAdapter[e.Position] );
			};
		}


		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			switch (requestCode) {
			case BLE_ACTIVATION_CODE:
				if (resultCode == Result.Ok) {
					ToggleScanning ();
				} else {
					Toast.MakeText (this, "Bluetooth is not activated", ToastLength.Short).Show ();
				}
				return;
			default:
				return;
			}
		}
		static void ToggleScanning ()
		{
			if (!BluetoothLEManager.Current.IsScanning) {
				BluetoothLEManager.Current.BeginScanningForDevices ();
			}
			else {
				BluetoothLEManager.Current.StopScanningForDevices ();
			}
		}

		protected void WireupExternalHandlers ()
		{
			this.deviceDiscoveredHandler = (object sender, BluetoothLEManager.DeviceDiscoveredEventArgs e) => {
				Console.WriteLine ("Discovered device: " + e.Device.Name);
				this.RunOnUiThread( () => {
					this.listAdapter = new DevicesAdapter(this, BluetoothLEManager.Current.DiscoveredDevices);
					this.listView.Adapter = this.listAdapter;
				});
			};
			BluetoothLEManager.Current.DeviceDiscovered += this.deviceDiscoveredHandler;

			this.deviceConnectedHandler = (object sender, BluetoothLEManager.DeviceConnectionEventArgs e) => {
				this.RunOnUiThread( () => {
					this.progress.Hide();
				});
				App.Current.State.SelectedDevice = e.Device;
				StartActivity (typeof(DeviceDetails.DeviceDetailsScreen));
			};
			BluetoothLEManager.Current.DeviceConnected += this.deviceConnectedHandler;
		}

		protected void RemoveExternalHandlers()
		{
			BluetoothLEManager.Current.DeviceDiscovered -= this.deviceDiscoveredHandler;
			BluetoothLEManager.Current.DeviceConnected -= this.deviceConnectedHandler;
		}

		protected void StopScanning()
		{
			new Task( () => {
				if(BluetoothLEManager.Current.IsScanning) {
					Console.WriteLine ("Still scanning, stopping the scan and reseting the right button");
					BluetoothLEManager.Current.StopScanningForDevices();
					this.scanButton.SetState (ScanButton.ScanButtonState.Normal);
				}
			}).Start();
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			Finish ();
			return base.OnOptionsItemSelected (item);
		}
	}
}

