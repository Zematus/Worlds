using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveLoadTest : AutomatedTest {

	public enum Stage {

		Generation,
		Save,
		Load
	}

	public delegate bool SaveConditionDelegate (World world);

//	private int _initialSkip = 80;
//	private int _firstOffset = 1;
//	private int _secondOffset = 1;

	private int _offsetPerCheck;

	private int _eventCountBeforeSave;
	private int _eventCountAfterSave;
	private int _eventCountAfterLoad;

	private int _saveDate;

	private int _numChecks;

	private int[] _saveDatePlusOffsets;

	private int[] _afterSave_EventCounts;
	private int[] _afterSave_LastRandomIntegers;
	private int[] _afterSave_GroupCounts;
	private int[] _afterSave_PolityCounts;
	private int[] _afterSave_RegionCounts;
	private int[] _afterSave_LanguageCounts;

	private int[] _afterLoad_EventCounts;
	private int[] _afterLoad_LastRandomIntegers;
	private int[] _afterLoad_GroupCounts;
	private int[] _afterLoad_PolityCounts;
	private int[] _afterLoad_RegionCounts;
	private int[] _afterLoad_LanguageCounts;

//	private int _saveDatePlusOffset;
//
//	private int _eventCountAfterSaveOffset1;
//	private int _eventCountAfterSaveOffset2;
//
//	private int _eventCountAfterLoadOffset1;
//	private int _eventCountAfterLoadOffset2;
//
//	private int _lastRandomIntegerAfterSave1;
//	private int _lastRandomIntegerAfterSave2;
//
//	private int _lastRandomIntegerAfterLoad1;
//	private int _lastRandomIntegerAfterLoad2;
//
//	private int _numGroupsAfterSaveOffset2;
//	private int _numGroupsAfterLoadOffset2;
//
//	private int _numPolitiesAfterSaveOffset2;
//	private int _numPolitiesAfterLoadOffset2;

	private TestRecorder _saveRecorder = new TestRecorder ();
	private TestRecorder _loadRecorder = new TestRecorder ();

	private bool _result = true;

	private string _savePath;

	private World _world;

	private Stage _stage = Stage.Generation;

	private bool _validateRecording = false;

	private SaveConditionDelegate _saveCondition;

	public SaveLoadTest (int initialSkip, int offsetPerCheck, int numChecks, bool validateRecording) {

		_offsetPerCheck = offsetPerCheck;
		_numChecks = numChecks;

		_validateRecording = validateRecording;

		Name = "Save/Load Test with initialSkip: " + initialSkip
			+ ", offsetPerCheck: " + offsetPerCheck
			+ ", numChecks: " + numChecks;

		if (_validateRecording) {
			Name += " with recording validation";
		} else {
			Name += " without recording validation";
		}

		_saveCondition = (World world) => {
			return (world.CurrentDate >= initialSkip);
		};

		_saveDatePlusOffsets = new int[numChecks];

		_afterSave_EventCounts = new int[numChecks];
		_afterSave_LastRandomIntegers = new int[numChecks];
		_afterSave_GroupCounts = new int[numChecks];
		_afterSave_PolityCounts = new int[numChecks];
		_afterSave_RegionCounts = new int[numChecks];
		_afterSave_LanguageCounts = new int[numChecks];

		_afterLoad_EventCounts = new int[numChecks];
		_afterLoad_LastRandomIntegers = new int[numChecks];
		_afterLoad_GroupCounts = new int[numChecks];
		_afterLoad_PolityCounts = new int[numChecks];
		_afterLoad_RegionCounts = new int[numChecks];
		_afterLoad_LanguageCounts = new int[numChecks];
	}

	public SaveLoadTest (string subName, SaveConditionDelegate saveCondition, int offsetPerCheck, int numChecks, bool validateRecording) {
		
		_offsetPerCheck = offsetPerCheck;
		_numChecks = numChecks;

		_validateRecording = validateRecording;

		Name = "Save/Load Test " + subName
			+ ", offsetPerCheck: " + offsetPerCheck
			+ ", numChecks: " + numChecks;

		if (_validateRecording) {
			Name += " with recording validation";
		} else {
			Name += " without recording validation";
		}

		_saveCondition = saveCondition;

		_saveDatePlusOffsets = new int[numChecks];

		_afterSave_EventCounts = new int[numChecks];
		_afterSave_LastRandomIntegers = new int[numChecks];
		_afterSave_GroupCounts = new int[numChecks];
		_afterSave_PolityCounts = new int[numChecks];
		_afterSave_RegionCounts = new int[numChecks];
		_afterSave_LanguageCounts = new int[numChecks];

		_afterLoad_EventCounts = new int[numChecks];
		_afterLoad_LastRandomIntegers = new int[numChecks];
		_afterLoad_GroupCounts = new int[numChecks];
		_afterLoad_PolityCounts = new int[numChecks];
		_afterLoad_RegionCounts = new int[numChecks];
		_afterLoad_LanguageCounts = new int[numChecks];
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

			while (!_saveCondition (_world)) {

				_world.Iterate ();
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

			if (_validateRecording) {
				Manager.Recorder = _saveRecorder;
			}

			_eventCountAfterSave = _world.EventsToHappen.Count;
			Debug.Log ("Number of Events after save: " + _eventCountAfterSave);

			if (_eventCountBeforeSave != _eventCountAfterSave) {

				Debug.LogError ("Number of events before and after save are different");

				_result = false;

			} else {

				Debug.Log ("Number of Events remain equal after save");
			}

			int saveDatePlusOffset = _saveDate;

			for (int c = 0; c < _numChecks; c++) {

				string checkStr = "[with offset " + c + "]";

				Debug.Log ("Pushing simulation forward after save " + checkStr + "...");

				while (_world.CurrentDate < (saveDatePlusOffset + _offsetPerCheck)) {

					Manager.CurrentWorld.Iterate ();
				}

				_saveDatePlusOffsets[c] = _world.CurrentDate;

				Debug.Log ("Current Date: " + _world.CurrentDate);

				_afterSave_EventCounts[c] = _world.EventsToHappenCount;
				Debug.Log ("Number of Events after Save " + checkStr + ": " + _afterSave_EventCounts[c]);

				#if DEBUG
				_afterSave_LastRandomIntegers[c] = TerrainCell.LastRandomInteger;
				Debug.Log ("Last Random Integer after Save " + checkStr + ": " + _afterSave_LastRandomIntegers[c]);
				#endif

				_afterSave_GroupCounts[c] = _world.CellGroupCount;
				_afterSave_PolityCounts[c] = _world.PolityCount;
				_afterSave_RegionCounts[c] = _world.RegionCount;
				_afterSave_LanguageCounts[c] = _world.LanguageCount;
				Debug.Log ("Number of Cell Groups after Save " + checkStr + ": " + _afterSave_GroupCounts[c]);
				Debug.Log ("Number of Polities after Save " + checkStr + ": " + _afterSave_PolityCounts[c]);
				Debug.Log ("Number of Regions after Save " + checkStr + ": " + _afterSave_RegionCounts[c]);
				Debug.Log ("Number of Languages after Save " + checkStr + ": " + _afterSave_LanguageCounts[c]);

				saveDatePlusOffset = _saveDatePlusOffsets[c];
			}

			Debug.Log ("Loading world...");

			Manager.LoadWorldAsync (_savePath);

			_stage = Stage.Load;

			break;

		case Stage.Load:

			if (Manager.PerformingAsyncTask) {
				return;
			}

			if (_validateRecording) {
				Manager.Recorder = _loadRecorder;
			}

			_world = Manager.CurrentWorld;

			int loadDate = _world.CurrentDate;

			Debug.Log ("Date after Load: " + loadDate);

			if (loadDate != _saveDate) {

				Debug.LogError ("Load date different from Save date");

				_result = false;

			} else {

				Debug.Log ("Load date equal to Save date");
			}
			
			_eventCountAfterLoad = _world.EventsToHappen.Count;
			Debug.Log ("Number of Events after Load: " + _eventCountAfterLoad);

			if (_eventCountAfterLoad != _eventCountAfterSave) {

				Debug.LogError ("Number of events after Load different from after Save");

				_result = false;

			} else {

				Debug.Log ("Number of Events remain equal after Load");
			}

			int loadDatePlusOffset = loadDate;

			for (int c = 0; c < _numChecks; c++) {

				string checkStr = "[with offset " + c + "]";

				Debug.Log ("Pushing simulation forward after load " + checkStr + "...");

				while (_world.CurrentDate < (loadDatePlusOffset + _offsetPerCheck)) {

					_world.Iterate ();
				}

				loadDatePlusOffset = _world.CurrentDate;

				Debug.Log ("Current Date: " + _world.CurrentDate);

				if (_saveDatePlusOffsets[c] != loadDatePlusOffset) {

					Debug.LogError ("Load date after offset different from Save date with offset [" + c + "]:" + _saveDatePlusOffsets[c]);

					_result = false;

				} else {

					Debug.Log ("Load date after offset equal to Save date with offset [" + c + "]");
				}

				_afterLoad_EventCounts[c] = _world.EventsToHappenCount;
				Debug.Log ("Number of Events after load " + checkStr + ": " + _afterLoad_EventCounts[c]);

				if (_afterSave_EventCounts[c] != _afterLoad_EventCounts[c]) {

					Debug.LogError ("Number of events after load with offset not equal to : " + _afterSave_EventCounts[c]);

					_result = false;

				} else {

					Debug.Log ("Number of events after load with offset equal");
				}

				#if DEBUG
				_afterLoad_LastRandomIntegers[c] = TerrainCell.LastRandomInteger;
				Debug.Log ("Last Random Integer after Load " + checkStr + ": " + _afterLoad_LastRandomIntegers[c]);

				/// NOTE: TerrainCell.LastRandomInteger might not be consistent because the order in which events are executed for a particular date might change after Load
				/// 	this shouldn't have any effect on the simulation
				/// 
//			if (_lastRandomIntegerAfterSave1 != _lastRandomIntegerAfterLoad1) {
//
//				Debug.LogError ("First last random integer after load not equal to : " + _lastRandomIntegerAfterSave1);
//
//				_result = false;
//
//			} else {
//
//				Debug.Log ("First last random integer after load equal");
//			}
				#endif

				_afterLoad_GroupCounts[c] = _world.CellGroupCount;
				Debug.Log ("Number of Cell Groups after Load " + checkStr + ": " + _afterLoad_GroupCounts[c]);

				if (_afterSave_GroupCounts[c] != _afterLoad_GroupCounts[c]) {

					Debug.LogError ("Number of cell groups after Load with offset not equal to : " + _afterSave_GroupCounts[c]);

					_result = false;

				} else {

					Debug.Log ("Number of cell groups after load with offset equal");
				}

				_afterLoad_PolityCounts[c] = _world.PolityCount;
				Debug.Log ("Number of Polities after Load " + checkStr + ": " + _afterLoad_PolityCounts[c]);

				if (_afterSave_PolityCounts[c] != _afterLoad_PolityCounts[c]) {

					Debug.LogError ("Number of polities after load with offset not equal to : " + _afterSave_PolityCounts[c]);

					_result = false;

				} else {

					Debug.Log ("Number of polities after load with offset equal");
				}

				_afterLoad_RegionCounts[c] = _world.RegionCount;
				Debug.Log ("Number of Regions after Load " + checkStr + ": " + _afterLoad_RegionCounts[c]);

				if (_afterSave_RegionCounts[c] != _afterLoad_RegionCounts[c]) {

					Debug.LogError ("Number of regions after load with offset not equal to : " + _afterSave_RegionCounts[c]);

					_result = false;

				} else {

					Debug.Log ("Number of regions after load with offset equal");
				}

				_afterLoad_LanguageCounts[c] = _world.LanguageCount;
				Debug.Log ("Number of Regions after Load " + checkStr + ": " + _afterLoad_LanguageCounts[c]);

				if (_afterSave_LanguageCounts[c] != _afterLoad_LanguageCounts[c]) {

					Debug.LogError ("Number of languages after load with offset not equal to : " + _afterSave_LanguageCounts[c]);

					_result = false;

				} else {

					Debug.Log ("Number of languages after load with offset equal");
				}
			}

			if (_validateRecording) {
				int saveEntryCount = _saveRecorder.GetEntryCount ();
				int loadEntryCount = _loadRecorder.GetEntryCount ();

				if (saveEntryCount != loadEntryCount) {

					Debug.LogError ("Number of Test Recorder entries different: save=" + saveEntryCount + " load=" + loadEntryCount);
				} else {
					Debug.Log ("Number of Test Recorder entries: " + saveEntryCount);
				}

				int maxEntryErrors = 5;
				int entryErrorCount = 0;

				foreach (KeyValuePair<string,string> pair in _saveRecorder.RecordedData) {
			
					string entryKey = pair.Key;
					string saveEntry = pair.Value;
					string loadEntry = _loadRecorder.Recover (entryKey);

					if (saveEntry != loadEntry) {
					
						if (entryErrorCount < maxEntryErrors) {
							Debug.LogError ("Entries with key [" + entryKey + "] different...\n\tSave entry: " + saveEntry + "\n\tLoad entry: " + loadEntry);
						}

						entryErrorCount++;
					}
				}

				entryErrorCount += Mathf.Abs (saveEntryCount - loadEntryCount);

				if (entryErrorCount > maxEntryErrors) {
					Debug.LogError ("Entry errors not displayed: " + (entryErrorCount - maxEntryErrors));
				}

				if (entryErrorCount > 0) {
					Debug.LogError ("Total entry error count: " + entryErrorCount);
				} else {
					Debug.Log ("Total entry error count: " + entryErrorCount);
				}

				Manager.Recorder = DefaultRecorder.Default;
			}

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
