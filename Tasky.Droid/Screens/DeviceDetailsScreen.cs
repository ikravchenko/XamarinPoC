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
using Android.Bluetooth;
using Java.Util;

namespace BluetoothLEExplorer.Droid.Screens.Scanner.DeviceDetails
{
	[Activity (Label = "Services")]			
	public class DeviceDetailsScreen : ListActivity
	{
		protected IList<BluetoothGattService> services = new List<BluetoothGattService>();

		EventHandler<BluetoothLEManager.ServiceDiscoveredEventArgs> serviceDiscoveredHandler;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// set the activity title
			base.Title = App.Current.State.SelectedDevice.Name;

			// create our adapter
			this.ListAdapter = new ServicesAdapter (this, this.services);

			BluetoothLEManager.Current.ConnectedDevices [App.Current.State.SelectedDevice].DiscoverServices ();

			ActionBar.SetDisplayHomeAsUpEnabled (true);
			ActionBar.SetHomeButtonEnabled (true);
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			this.WireUpLocalHandlers ();
			this.WireUpExternalHandlers ();
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			this.RemoveExternalHandlers ();
		}

		protected void WireUpLocalHandlers ()
		{
			this.ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				// set the selection
				this.ListView.SetSelection(e.Position);

				// persist the selected service service
				App.Current.State.SelectedService = this.services[e.Position];
				// launch the service details screen
				StartActivity (typeof(ServiceDetails.ServiceDetailsScreen));
			};
		}

		protected void WireUpExternalHandlers ()
		{
			serviceDiscoveredHandler = (object sender, BluetoothLEManager.ServiceDiscoveredEventArgs e) => {
				if (BluetoothLEManager.Current.ConnectedDevices[App.Current.State.SelectedDevice] != null) {

					this.services = BluetoothLEManager.Current.Services[App.Current.State.SelectedDevice];
					this.RunOnUiThread( () => {
						this.ListAdapter = new ServicesAdapter (this, this.services);
					});
				}
			};
			BluetoothLEManager.Current.ServiceDiscovered += serviceDiscoveredHandler;
		}

		protected void RemoveExternalHandlers()
		{
			BluetoothLEManager.Current.ServiceDiscovered -= serviceDiscoveredHandler;
		}

		public override void OnBackPressed ()
		{
			base.OnBackPressed ();

			// disconnect from device
			if (App.Current.State.SelectedDevice != null)
				BluetoothLEManager.Current.DisconnectDevice (App.Current.State.SelectedDevice);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			Finish ();
			return base.OnOptionsItemSelected (item);
		}

	}
}

