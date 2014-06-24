using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using Tasky.AL;
using Tasky.BL;
using Tasky.BL.Managers;

using System.Xml.Linq;
using BluetoothLEExplorer.iOS.UI.Screens.Scanner.Home;

namespace Tasky.Screens.iPhone {
	public class HomeScreen : DialogViewController {
		List<Task> tasks;
		NetworkManager NetworkManager;
		LocalizableBindingContext context;
		TaskDialog taskDialog;
		Task currentTask;
		DialogViewController detailsScreen;

		
		public HomeScreen () : base (UITableViewStyle.Plain, null)
		{
			Initialize ();
		}
		
		protected void Initialize()
		{
			NetworkManager = new NetworkManager ();
			NetworkManager.NetworkDataLoaded += (object sender, EventArgs e) => {
				PopulateTable ();
			};
			NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Add), false);
			NavigationItem.RightBarButtonItem.Clicked += (sender, e) => { ShowTaskDetails(new Task()); };
			NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Refresh), false);
			NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => { 
				RequestData();
			};
		}

		public async void RequestData() {
			var nr = NetworkManager.RequestAndSaveWeather ();
			await nr;
		}
			
		protected void ShowTaskDetails (Task task)
		{
			currentTask = task;
			taskDialog = new TaskDialog (task);
			
			var title = MonoTouch.Foundation.NSBundle.MainBundle.LocalizedString ("Task Details", "Task Details");
			context = new LocalizableBindingContext (this, taskDialog, title);
			detailsScreen = new DialogViewController (context.Root, true);
			ActivateController(detailsScreen);
		}

		public void GoToScan() {
			var scanController = new ScannerHome ();
			UINavigationController nav = new UINavigationController (scanController);
			PresentViewController (nav, true, null);
		}

		public void SaveTask()
		{
			context.Fetch (); // re-populates with updated values
			currentTask.Name = taskDialog.Name;
			currentTask.Notes = taskDialog.Notes;
			currentTask.Done = taskDialog.Done;
			TaskManager.SaveTask(currentTask);
			NavigationController.PopViewControllerAnimated (true);
		//	context.Dispose (); // documentation suggests this is required, but appears to cause a crash sometimes
		}
		public void DeleteTask ()
		{
			if (currentTask.ID >= 0)
			TaskManager.DeleteTask (currentTask.ID);
			NavigationController.PopViewControllerAnimated (true);
		}


		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			PopulateTable ();
		}
		
		protected void PopulateTable ()
		{
			tasks = TaskManager.GetTasks ().ToList ();
			var newTaskDefaultName = MonoTouch.Foundation.NSBundle.MainBundle.LocalizedString ("<new task>", "<new task>");
			// make into a list of MT.D elements to display
			List<Element> le = new List<Element>();
			foreach (var t in tasks) {
				le.Add (new CheckboxElement((t.Name == "" ? newTaskDefaultName:t.Name + " " + t.Notes ), t.Done));
			}
			var s = new Section ();
			s.AddAll (le);
			Root = new RootElement ("Tasky") { s }; 
		}

		public override void Selected (MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var task = tasks[indexPath.Row];
			ShowTaskDetails(task);
		}

		public override Source CreateSizingSource (bool unevenRows)
		{
			return new EditingSource (this);
		}

		public void DeleteTaskRow(int rowId)
		{
			TaskManager.DeleteTask(tasks[rowId].ID);
		}
	}
}