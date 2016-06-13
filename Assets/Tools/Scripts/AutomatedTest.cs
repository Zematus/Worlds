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
}

public class SaveLoadTest : AutomatedTest {

	private int _eventCountBeforeSave;
	private int _eventCountAfterSave;
	private int _eventCountAfterLoad;

	private int _saveDate;

	private bool _result = true;

	public SaveLoadTest () {
	}

	public override void Run () {

		string path = Manager.SavePath + "TestSaveLoad.plnt";

		Manager.GenerateNewWorld (407252633);

		int population = (int)Mathf.Ceil (World.StartPopulationDensity * TerrainCell.MaxArea);

		Manager.GenerateRandomHumanGroup (population);

		World world = Manager.CurrentWorld;

		while (world.CurrentDate < 100) {

			Manager.CurrentWorld.Iterate ();
		}

		_saveDate = world.CurrentDate;

		Debug.Log ("Save Date: " + _saveDate);

		_eventCountBeforeSave = world.EventsToHappenCount;
		Debug.Log ("Number of Events before save: " + _eventCountBeforeSave);

		Manager.SaveWorld (path);

		_eventCountAfterSave = world.EventsToHappen.Count;
		Debug.Log ("Number of Events after save: " + _eventCountAfterSave);

		if (_eventCountBeforeSave != _eventCountAfterSave) {

			Debug.LogError ("Number of events before and after save are different");

			_result = false;

		} else {

			Debug.Log ("Number of Events remain equal after save");
		}

		while (world.CurrentDate == _saveDate) {

			Manager.CurrentWorld.Iterate ();
		}

		Debug.Log ("Current Date: " + world.CurrentDate);

		Manager.LoadWorld (path);

		_eventCountAfterLoad = world.EventsToHappen.Count;
		Debug.Log ("Number of Events after load: " + _eventCountAfterLoad);

		if (_eventCountAfterLoad != _eventCountAfterSave) {

			Debug.LogError ("Number of events after load different from after save");

			_result = false;

		} else {

			Debug.Log ("Number of Events remain equal after load");
		}

		if (world.CurrentDate == _saveDate) {

			Manager.CurrentWorld.Iterate ();
		}

		Debug.Log ("Current Date: " + world.CurrentDate);

		if (_result)
			State = TestState.Succeded;
		else
			State = TestState.Failed;
	}
}
