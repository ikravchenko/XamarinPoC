using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Tasky.BL;
using Android.Views;
using System.Threading.Tasks;
using BluetoothLEExplorer.Droid.Screens.Scanner.Home;
using Tasky.BL.Managers;

namespace Tasky.Droid.Screens {
	[Activity (Label = "Weather", MainLauncher = true)]			
	public class HomeScreen : Activity {
		protected Adapters.TaskListAdapter taskList;
		protected IList<Tasky.BL.Task> tasks;
		protected ListView taskListView = null;
		private NetworkManager networkManager;

		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			networkManager = new NetworkManager ();
			networkManager.NetworkDataLoaded += (object sender, System.EventArgs e) => {
				tasks = TaskManager.GetTasks();
				taskList = new Adapters.TaskListAdapter(this, tasks);
				taskListView.Adapter = taskList;
			};
            // Enable the ActionBar
            RequestWindowFeature(WindowFeatures.ActionBar);

			// set our layout to be the home screen
			SetContentView(Resource.Layout.HomeScreen);

			//Find our controls
			taskListView = FindViewById<ListView> (Resource.Id.lstTasks);
			
			// wire up task click handler
			if(taskListView != null) {
				taskListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
					var taskDetails = new Intent (this, typeof (TaskDetailsScreen));
					taskDetails.PutExtra ("TaskID", tasks[e.Position].ID);
					StartActivity (taskDetails);
				};
			}
		}
		
		protected override void OnResume ()
		{
			base.OnResume ();

			tasks = Tasky.BL.Managers.TaskManager.GetTasks();
			
			// create our adapter
			taskList = new Adapters.TaskListAdapter(this, tasks);

			//Hook up our adapter to our ListView
			taskListView.Adapter = taskList;
		}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // Create the actions in the ActionBar.
            MenuInflater.Inflate(Resource.Menu.menu_homescreen, menu);
            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_add_task:
                    // The user has tapped the add task button
                    StartActivity(typeof(TaskDetailsScreen));
                    return true;
			case Resource.Id.menu_refresh:
				RequestData();
				return true;
			case Resource.Id.menu_ble:
				StartActivity(typeof(ScannerHome));
				return true;
				default:
	                return base.OnOptionsItemSelected(item);
            }

        }

		public async void RequestData() {
			var nr = networkManager.RequestAndSaveWeather ();
			await nr;
		}
	}
}