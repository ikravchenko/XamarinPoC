
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using BluetoothLEExplorer.iOS;

namespace Tasky
{
	public class ScanController : UITableViewController
	{
		public ScanController () : base (UITableViewStyle.Grouped)
		{
			UIBarButtonItem scanButton = new UIBarButtonItem ();
			scanButton.Title = "Scan";
			NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Done), false);
			NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => { DismissViewController(true, null); };
			NavigationItem.Title = "Bluetooth LE";
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Register the TableView's data source
			TableView.Source = new ScanControllerSource ();
		}
	}
}

