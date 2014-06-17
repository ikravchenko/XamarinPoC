using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreBluetooth;
using System.Collections.Generic;
using BluetoothLEExplorer.iOS.UI.Controls;
using System.Threading.Tasks;
using MBProgressHUD;

namespace BluetoothLEExplorer.iOS.UI.Screens.Scanner.Home
{
	public partial class ScannerHome : UIViewController
	{
		ScanButton scanButton;
		UITableView bleDevicesTable;
		BleDeviceTableSource tableSource;
		MTMBProgressHUD connectingDialog;
		DeviceDetails.DeviceDetailsScreen detailsScreen;

		public ScannerHome (IntPtr handle) : base (handle) 
		{
			this.Initialize ();
		}

		public ScannerHome ()
		{
			this.Initialize ();
		}

		protected void Initialize()
		{
			this.Title = "Scanner";
			NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Done), false);
			NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => { DismissViewController(true, null); };
			// configure our scan button
			this.scanButton = new ScanButton ();
			this.scanButton.TouchUpInside += (s,e) => {
				if ( !BluetoothLEManager.Current.IsScanning ) {
					BluetoothLEManager.Current.BeginScanningForDevices ();
				} else {
					BluetoothLEManager.Current.StopScanningForDevices ();
				}
			};			 
			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (this.scanButton), false);
		}

		public override void LoadView () {
			base.LoadView ();
			// setup the table
			this.bleDevicesTable = new UITableView ();
			this.tableSource = new BleDeviceTableSource ();
			this.View = bleDevicesTable;
			this.tableSource.PeripheralSelected += (object sender, BleDeviceTableSource.PeripheralSelectedEventArgs e) => {

				// stop scanning
				new Task( () => {
					if(BluetoothLEManager.Current.IsScanning) {
						Console.WriteLine ("Still scanning, stopping the scan and reseting the right button");
						BluetoothLEManager.Current.StopScanningForDevices();
						this.scanButton.SetState (ScanButton.ScanButtonState.Normal);
					}
				}).Start();

				// show our connecting... overlay
				//this.connectingDialog.LabelText = "Connecting to " + e.SelectedPeripheral.Name;
				//this.connectingDialog.Show(true);

				// when the peripheral connects, load our details screen
				BluetoothLEManager.Current.DeviceConnected += (object s, CBPeripheralEventArgs periphE) => {
					//this.connectingDialog.Hide(false);

					this.detailsScreen = new BluetoothLEExplorer.iOS.UI.Screens.Scanner.DeviceDetails.DeviceDetailsScreen() as DeviceDetails.DeviceDetailsScreen;
					this.detailsScreen.ConnectedPeripheral = periphE.Peripheral;
					this.NavigationController.PushViewController ( this.detailsScreen, true);

				};

				// try and connect to the peripheral
				BluetoothLEManager.Current.CentralBleManager.ConnectPeripheral (e.SelectedPeripheral, new PeripheralConnectionOptions());
			};

			bleDevicesTable.Source = this.tableSource;

			// wire up the DiscoveredPeripheral event to update the table
			BluetoothLEManager.Current.DeviceDiscovered += (object sender, CBDiscoveredPeripheralEventArgs e) => {
				Console.WriteLine("delegate for device discovering invoked");
				this.tableSource.Peripherals = BluetoothLEManager.Current.DiscoveredDevices;
				this.bleDevicesTable.ReloadData();
			};

			BluetoothLEManager.Current.ScanTimeoutElapsed += (sender, e) => {
				Console.WriteLine("delegate for discovering timeout invoked");
				this.scanButton.SetState ( ScanButton.ScanButtonState.Normal );
			};

			// add our 'connecting' overlay
			this.connectingDialog = new MTMBProgressHUD (View) {
				LabelText = "Connecting to device...",
				RemoveFromSuperViewOnHide = false
			};
			this.View.AddSubview (this.connectingDialog);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Console.WriteLine ("Scanner Home, View Did Load");
		}

		protected class BleDeviceTableSource : UITableViewSource
		{
			protected const string cellID = "BleDeviceCell";

			public event EventHandler<PeripheralSelectedEventArgs> PeripheralSelected = delegate {};

			public List<CBPeripheral> Peripherals
			{
				get { return this._peripherals; }
				set { this._peripherals = value; }
			}
			protected List<CBPeripheral> _peripherals = new List<CBPeripheral> ();

			public BleDeviceTableSource () {}

			public BleDeviceTableSource (List<CBPeripheral> peripherals)
			{
				_peripherals = peripherals;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return this._peripherals.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell (cellID);
				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Subtitle, cellID);
				}

				CBPeripheral peripheral = this._peripherals [indexPath.Row];
				//TODO: convert to async and update?
				peripheral.ReadRSSI ();
				cell.TextLabel.Text = peripheral.Name;
				cell.DetailTextLabel.Text = "UUID: " + peripheral.UUID.ToString () + ", Signal Strength: " + peripheral.RSSI;

				return cell;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				CBPeripheral peripheral = this._peripherals [indexPath.Row];
				tableView.DeselectRow (indexPath, false);
				this.PeripheralSelected (this, new PeripheralSelectedEventArgs (peripheral));
			}

			public class PeripheralSelectedEventArgs : EventArgs
			{
				public CBPeripheral SelectedPeripheral
				{
					get { return this._peripheral; }
				} protected CBPeripheral _peripheral;

				public PeripheralSelectedEventArgs (CBPeripheral peripheral)
				{
					this._peripheral = peripheral;
				}
			}
		}
	}

}

