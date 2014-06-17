
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Tasky
{
	public class ScanControllerCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("ScanControllerCell");

		public ScanControllerCell () : base (UITableViewCellStyle.Value1, Key)
		{
			// TODO: add subviews to the ContentView, set various colors, etc.
			TextLabel.Text = "TextLabel";
		}
	}
}

