using UnityEngine;
using System.Collections;

public enum TestState {

	NotStarted,
	Running,
	Failed,
	Succeded
}

public abstract class AutomatedTest {

	public TestState State { get; protected set; }

	public abstract void Run ();

	protected AutomatedTest () {
	
		State = TestState.NotStarted;
	}
}

public class SaveLoadTest : AutomatedTest {

	public enum Stage {

		Generation,
		Save,
		Load
	}

	private int _eventCountBeforeSave;
	private int _eventCountAfterSave;
	private int _eventCountAfterLoad;

	private int _saveDate;

	private bool _result = true;

	private string _savePath;

	private World _world;

	private Stage _stage = Stage.Generation;

	public SaveLoadTest () {
		
	}

	public override void Run () {

		switch (_stage) {

		case Stage.Generation:
			
			if (State == TestState.NotStarted) {

				Manager.LoadAppSettings (@"Worlds.settings");

				Manager.UpdateMainThreadReference ();
			
				_savePath = Manager.SavePath + "TestSaveLoad.plnt";

				Manager.GenerateNewWorldAsync (407252633);

				State = TestState.Running;
			}

			if (!Manager.WorldReady) {

				return;
			}

			if (Manager.PerformingAsyncTask) {
				return;
			}

			int population = (int)Mathf.Ceil (World.StartPopulationDensity * TerrainCell.MaxArea);

			Manager.GenerateRandomHumanGroup (population);

			_world = Manager.CurrentWorld;

			Debug.Log ("Pushing simulation forward at least 100 years...");

			while (_world.CurrentDate < 100) {

				Manager.CurrentWorld.Iterate ();
			}

			_saveDate = _world.CurrentDate;

			Debug.Log ("Save Date: " + _saveDate);

			_eventCountBeforeSave = _world.EventsToHappenCount;
			Debug.Log ("Number of Events before save: " + _eventCountBeforeSave);

			Manager.SaveWorldAsync (_savePath);

			_stage = Stage.Save;

			break;

		case Stage.Save:
			
			if (Manager.PerformingAsyncTask) {
				return;
			}

			_eventCountAfterSave = _world.EventsToHappen.Count;
			Debug.Log ("Number of Events after save: " + _eventCountAfterSave);

			if (_eventCountBeforeSave != _eventCountAfterSave) {

				Debug.LogError ("Number of events before and after save are different");

				_result = false;

			} else {

				Debug.Log ("Number of Events remain equal after save");
			}

			Debug.Log ("Pushing simulation forward...");

			while (_world.CurrentDate < (_saveDate + 100)) {

				Manager.CurrentWorld.Iterate ();
			}

			Debug.Log ("Current Date: " + _world.CurrentDate);

			Manager.LoadWorldAsync (_savePath);

			_stage = Stage.Load;

			break;

		case Stage.Load:

			if (Manager.PerformingAsyncTask) {
				return;
			}

			_world = Manager.CurrentWorld;

			Debug.Log ("Date after Load: " + _world.CurrentDate);
			
			_eventCountAfterLoad = _world.EventsToHappen.Count;
			Debug.Log ("Number of Events after load: " + _eventCountAfterLoad);

			if (_eventCountAfterLoad != _eventCountAfterSave) {

				Debug.LogError ("Number of events after load different from after save");

				_result = false;

			} else {

				Debug.Log ("Number of Events remain equal after load");
			}

			Debug.Log ("Pushing simulation forward...");

			while (_world.CurrentDate < (_saveDate + 100)) {

				Manager.CurrentWorld.Iterate ();
			}

			Debug.Log ("Current Date: " + _world.CurrentDate);

			if (_result)
				State = TestState.Succeded;
			else
				State = TestState.Failed;

			break;

		default:
			throw new System.Exception ("Unrecognized test stage: " + _stage);
		}
	}
}
