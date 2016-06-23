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

	private const int _skipBeforeSave = 1;
	private const int _firstOffsetAfter = 1;
	private const int _secondOffsetAfter = 1;

	private int _eventCountBeforeSave;
	private int _eventCountAfterSave;
	private int _eventCountAfterLoad;

	private int _eventCountAfterSaveOffset1;
	private int _eventCountAfterSaveOffset2;

	private int _eventCountAfterLoadOffset1;
	private int _eventCountAfterLoadOffset2;

	private int _lastRandomIntegerAfterSave1;
	private int _lastRandomIntegerAfterSave2;

	private int _lastRandomIntegerAfterLoad1;
	private int _lastRandomIntegerAfterLoad2;

	private int _saveDate;
	private int _saveDatePlusOffset;

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

				Debug.Log ("Generating world...");

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

			Debug.Log ("Pushing simulation forward before save...");

			while (_world.CurrentDate < _skipBeforeSave) {

				Manager.CurrentWorld.Iterate ();
			}

			_saveDate = _world.CurrentDate;

			Debug.Log ("Save Date: " + _saveDate);

			#if DEBUG
			Debug.Log ("Last Random Integer: " + TerrainCell.LastRandomInteger);
			#endif

			_eventCountBeforeSave = _world.EventsToHappenCount;
			Debug.Log ("Number of Events before save: " + _eventCountBeforeSave);

			Debug.Log ("Saving world...");

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

			Debug.Log ("Pushing simulation forward after save...");

			while (_world.CurrentDate < (_saveDate + _firstOffsetAfter)) {

				Manager.CurrentWorld.Iterate ();
			}

			_saveDatePlusOffset = _world.CurrentDate;

			Debug.Log ("Current Date: " + _world.CurrentDate);

			_eventCountAfterSaveOffset1 = _world.EventsToHappenCount;
			Debug.Log ("Number of Events after save with offset 1: " + _eventCountAfterSaveOffset1);

			#if DEBUG
			_lastRandomIntegerAfterSave1 = TerrainCell.LastRandomInteger;
			Debug.Log ("Last Random Integer after Save with offset 1: " + _lastRandomIntegerAfterSave1);
			#endif

			Debug.Log ("Pushing simulation forward after save again...");

			while (_world.CurrentDate < (_saveDatePlusOffset + _secondOffsetAfter)) {

				Manager.CurrentWorld.Iterate ();
			}

			Debug.Log ("Current Date: " + _world.CurrentDate);

			_eventCountAfterSaveOffset2 = _world.EventsToHappenCount;
			Debug.Log ("Number of Events after save with offset 2: " + _eventCountAfterSaveOffset2);

			#if DEBUG
			_lastRandomIntegerAfterSave2 = TerrainCell.LastRandomInteger;
			Debug.Log ("Last Random Integer after Save with offset 2: " + _lastRandomIntegerAfterSave2);
			#endif

			Debug.Log ("Loading world...");

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

			Debug.Log ("Pushing simulation forward after load...");

			while (_world.CurrentDate < (_saveDate + _firstOffsetAfter)) {

				Manager.CurrentWorld.Iterate ();
			}

			Debug.Log ("Current Date: " + _world.CurrentDate);

			_eventCountAfterLoadOffset1 = _world.EventsToHappenCount;
			Debug.Log ("Number of Events after load with offset 1: " + _eventCountAfterLoadOffset1);

			if (_eventCountAfterSaveOffset1 != _eventCountAfterLoadOffset1) {

				Debug.LogError ("First number of events after load with offset not equal to : " + _eventCountAfterSaveOffset1);

				_result = false;

			} else {

				Debug.Log ("First number of events after load with offset equal");
			}

			#if DEBUG
			_lastRandomIntegerAfterLoad1 = TerrainCell.LastRandomInteger;
			Debug.Log ("Last Random Integer after Load with offset 1: " + _lastRandomIntegerAfterLoad1);

			if (_lastRandomIntegerAfterSave1 != _lastRandomIntegerAfterLoad1) {

				Debug.LogError ("First last random integer after load not equal to : " + _lastRandomIntegerAfterSave1);

				_result = false;

			} else {

				Debug.Log ("First last random integer after load equal");
			}
			#endif

			Debug.Log ("Pushing simulation forward after load again...");

			while (_world.CurrentDate < (_saveDatePlusOffset + _secondOffsetAfter)) {

				Manager.CurrentWorld.Iterate ();
			}

			Debug.Log ("Current Date: " + _world.CurrentDate);

			_eventCountAfterLoadOffset2 = _world.EventsToHappenCount;
			Debug.Log ("Number of Events after load with offset 2: " + _eventCountAfterLoadOffset2);

			if (_eventCountAfterSaveOffset2 != _eventCountAfterLoadOffset2) {

				Debug.LogError ("Second number of events after load with offset not equal to : " + _eventCountAfterSaveOffset2);

				_result = false;

			} else {

				Debug.Log ("Second number of events after load with offset equal");
			}

			#if DEBUG
			_lastRandomIntegerAfterLoad2 = TerrainCell.LastRandomInteger;
			Debug.Log ("Last Random Integer after Load with offset 2: " + _lastRandomIntegerAfterLoad2);

			if (_lastRandomIntegerAfterSave2 != _lastRandomIntegerAfterLoad2) {

				Debug.LogError ("Second last random integer after load not equal to : " + _lastRandomIntegerAfterSave2);

				_result = false;

			} else {

				Debug.Log ("Second last random integer after load equal");
			}
			#endif

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
