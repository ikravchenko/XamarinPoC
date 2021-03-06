using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Tasky.Screens.iPhone;

namespace Tasky {
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate {
		// class-level declarations
		UIWindow window;
		UINavigationController navController;
		UITableViewController homeViewController;
		
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			window.MakeKeyAndVisible ();


			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) {
				homeViewController = new HomeScreen();
			} else {
				homeViewController = new HomeScreen(); // TODO: replace with iPad screen if we implement for iPad
			}
			navController = new UINavigationController (homeViewController);

			UINavigationBar.Appearance.TintColor = UIColor.FromRGB (38, 117 ,255); // nice blue
			UITextAttributes ta = new UITextAttributes();
			ta.Font = UIFont.FromName ("AmericanTypewriter-Bold", 0f);
			UINavigationBar.Appearance.SetTitleTextAttributes(ta);
			ta.Font = UIFont.FromName ("AmericanTypewriter", 0f);
			UIBarButtonItem.Appearance.SetTitleTextAttributes(ta, UIControlState.Normal);	

			//navController.PushViewController(homeViewController, false);
			window.RootViewController = navController;
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}