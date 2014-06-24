using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using Tasky.AL;
using Tasky.BL;
using Tasky.BL.Managers;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using System.Xml.Linq;
using BluetoothLEExplorer.iOS.UI.Screens.Scanner.Home;

namespace Tasky.Screens.iPhone {
	[Register ("HomeScreen")]
	public partial class HomeScreen : UITableViewController {
		List<Task> Tasks;
		NetworkManager NetworkManager;
		LocalizableBindingContext context;
		TaskDialog taskDialog;
		Task currentTask;
		DialogViewController detailsScreen;
		WeatherDataSource TableSource;

		public HomeScreen (IntPtr handle) : base (handle) 
		{
			this.Initialize ();
		}

		public HomeScreen ()
		{
			this.Initialize ();
		}
		
		protected void Initialize()
		{
			NetworkManager = new NetworkManager ();
			NetworkManager.NetworkDataLoaded += (object sender, EventArgs e) => {
				PopulateTable ();
			};
			NavigationItem.Title = "Weather";
			NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Add), false);
			NavigationItem.RightBarButtonItem.Clicked += (sender, e) => { ShowTaskDetails(new Task()); };
			NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Refresh), false);
			NavigationItem.LeftBarButtonItem.Clicked += (sender, e) => { 
				RequestData();
			};
			TableSource = new WeatherDataSource ();
		}

		public async void RequestData() {
			var nr = NetworkManager.RequestAndSaveWeather ();
			await nr;
		}

		public override void LoadView() {
			base.LoadView ();
			this.TableView.DataSource = TableSource;

			this.TableView.Delegate = new WeatherTableDelegate() {
				Home = this
			};
		}

		public class WeatherTableDelegate : UITableViewDelegate {
			public HomeScreen Home {get; set;}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				Home.ShowTaskDetails (Home.Tasks [indexPath.Row]);
			}
		}
			
		protected void ShowTaskDetails (Task task)
		{
			currentTask = task;
			taskDialog = new TaskDialog (task);
			
			var title = MonoTouch.Foundation.NSBundle.MainBundle.LocalizedString ("Task Details", "Task Details");
			context = new LocalizableBindingContext (this, taskDialog, title);
			detailsScreen = new DialogViewController (context.Root, true);
			this.NavigationController.PushViewController(detailsScreen, true);
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
			Tasks = TaskManager.GetTasks ().ToList ();
			TableSource.Data = Tasks;
			this.TableView.ReloadData ();
		}

		public void DeleteTaskRow(int rowId)
		{
			TaskManager.DeleteTask(Tasks[rowId].ID);
		}

		public class WeatherDataSource : UITableViewDataSource {
			protected const string cellID = "WeatherCell";

			public List<Task> Data {get; set;}

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell (cellID);
				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Subtitle, cellID);
				}
				cell.TextLabel.Text = Data [indexPath.Row].Name;
				cell.DetailTextLabel.Text = Data [indexPath.Row].Notes;
				cell.Accessory = Data [indexPath.Row].Done ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

				return cell;
			}

			public override int RowsInSection (UITableView tableView, int section)
			{
				return Data.Count == 0 ? 0 : Data.Count - 1;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}
				
		}
	}
}