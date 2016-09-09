using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveLoadTest : AutomatedTest {

	public enum Stage {

		Generation,
		GenerationIterations,
		Save,
		Load
	}

	public delegate bool SaveConditionDelegate (World world);

	private const int MaxIterationsPerUpdate = 500;

	private int _seed;

	private int _offsetPerCheck;

	private int _eventCountBeforeSave;
	private int _eventCountAfterSave;
	private int _eventCountAfterLoad;

	private int _saveDate;

	private int _numChecks;
	private int _checksToSkip;

	private List<string> _debugMessages = new List<string> ();

	private List<string>[] _afterSave_DebugMessageLists;

//	private int _updateCellGroupEventCanTrigger_BaseCanTriggerFalse = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test
//	private int _updateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test
//	private int _updateCellGroupEventCanTrigger_True = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test

//	private int[] _afterSave_UpdateCellGroupEventCanTrigger_BaseCanTriggerFalseCounts;
//	private int[] _afterSave_UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDateCounts;
//	private int[] _afterSave_UpdateCellGroupEventCanTrigger_TrueCounts;

	private int _totalCallsToAddMigratingGroup = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test
	private int _totalCallsToAddGroupToUpdate = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test
	private int _totalCallsToGroupUpdate = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test
	private int _totalCallsToGetNextLocalRandom = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test

	private Dictionary<string, int> _afterSave_AddGroupToUpdateCallers = new Dictionary<string, int> ();
	private Dictionary<string, int> _afterLoad_AddGroupToUpdateCallers = new Dictionary<string, int> ();

	private Dictionary<string, int>[] _afterSave_AddGroupToUpdateCallersByCheck;

	private Dictionary<string, int> _afterSave_GetNextLocalRandomCallers = new Dictionary<string, int> ();
	private Dictionary<string, int> _afterLoad_GetNextLocalRandomCallers = new Dictionary<string, int> ();

	private Dictionary<string, int>[] _afterSave_GetNextLocalRandomCallersByCheck;

	private int[] _saveDatePlusOffsets;

	private int[] _afterSave_EventCounts;
	private int[] _afterSave_CallsToAddMigratingGroupCounts;
	private int[] _afterSave_CallsToAddGroupToUpdateCounts;
	private int[] _afterSave_CallsToGroupUpdateCounts;
	private int[] _afterSave_LastRandomIntegers;
	private int[] _afterSave_TotalCallsToGetNextLocalRandom;
	private int[] _afterSave_GroupCounts;
	private int[] _afterSave_PolityCounts;
	private int[] _afterSave_TotalTerritorySizes;
	private int[] _afterSave_RegionCounts;
	private int[] _afterSave_LanguageCounts;

	private TestRecorder _saveRecorder = new TestRecorder ();
	private TestRecorder _loadRecorder = new TestRecorder ();

	private bool _result = true;

	private string _savePath;

	private World _world;

	private Stage _stage = Stage.Generation;

	private bool _validateRecording = false;

	private SaveConditionDelegate _saveCondition;

	public SaveLoadTest (int seed, int initialDateSkip, int offsetPerCheck, int numChecks, int checksToSkip = 0, bool validateRecording = false) {

		Initialize ("with initialSkip: " + initialDateSkip, 
			seed, (World world) => {
				return (world.CurrentDate >= initialDateSkip);
			}, offsetPerCheck, numChecks, checksToSkip, validateRecording);
	}

	public SaveLoadTest (string conditionName, int seed, SaveConditionDelegate saveCondition, int offsetPerCheck, int numChecks, int checksToSkip = 0, bool validateRecording = false) {

		Initialize (conditionName, seed, saveCondition, offsetPerCheck, numChecks, checksToSkip, validateRecording);
	}

	private void Initialize (string conditionName, int seed, SaveConditionDelegate saveCondition, int offsetPerCheck, int numChecks, int checksToSkip, bool validateRecording) {

		_seed = seed;

		_offsetPerCheck = offsetPerCheck;
		_numChecks = numChecks;
		_checksToSkip = checksToSkip;

		_validateRecording = validateRecording;

		Name = "Save/Load Test " + conditionName
			+ ", world: " + seed
			+ ", numChecks: " + numChecks
			+ ", checksToSkip: " + checksToSkip
			+ ", offsetPerCheck: " + offsetPerCheck;

		if (_validateRecording) {
			Name += ", with recording validation";
		} else {
			Name += ", without recording validation";
		}

		_saveCondition = saveCondition;

		_afterSave_DebugMessageLists = new List<string>[_numChecks];

//		_afterSave_UpdateCellGroupEventCanTrigger_BaseCanTriggerFalseCounts = new int[_numChecks];
//		_afterSave_UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDateCounts = new int[_numChecks];
//		_afterSave_UpdateCellGroupEventCanTrigger_TrueCounts = new int[_numChecks];

		_afterSave_AddGroupToUpdateCallersByCheck = new Dictionary<string, int>[_numChecks];
		_afterSave_GetNextLocalRandomCallersByCheck = new Dictionary<string, int>[_numChecks];

		_saveDatePlusOffsets = new int[_numChecks];

		_afterSave_EventCounts = new int[_numChecks];
		_afterSave_CallsToAddMigratingGroupCounts = new int[_numChecks];
		_afterSave_CallsToAddGroupToUpdateCounts = new int[_numChecks];
		_afterSave_CallsToGroupUpdateCounts = new int[_numChecks];
		_afterSave_LastRandomIntegers = new int[_numChecks];
		_afterSave_TotalCallsToGetNextLocalRandom = new int[_numChecks];
		_afterSave_GroupCounts = new int[_numChecks];
		_afterSave_PolityCounts = new int[_numChecks];
		_afterSave_TotalTerritorySizes = new int[_numChecks];
		_afterSave_RegionCounts = new int[_numChecks];
		_afterSave_LanguageCounts = new int[_numChecks];
	}

	public override void Run () {

		switch (_stage) {

		case Stage.Generation:
			
			if (State == TestState.NotStarted) {

				Manager.LoadAppSettings (@"Worlds.settings");

				Manager.UpdateMainThreadReference ();
			
				_savePath = Manager.SavePath + "TestSaveLoad.plnt";

				Debug.Log ("Generating world " + _seed + "...");

				Manager.GenerateNewWorldAsync (_seed);

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

			_stage = Stage.GenerationIterations;

			break;

		case Stage.GenerationIterations:

			int iterations = 0;
			while (!_saveCondition (_world)) {

				_world.Iterate ();

				iterations++;

				if (iterations >= MaxIterationsPerUpdate) {

//					Debug.Log ("Current Date: " + _world.CurrentDate);
					return;
				}
			}

			_saveDate = _world.CurrentDate;

			Debug.Log ("Save Date: " + _saveDate);

			#if DEBUG
			Debug.Log ("Last Random Integer: " + TerrainCell.LastRandomInteger);
			#endif

			_eventCountBeforeSave = _world.EventsToHappenCount;
			Debug.Log ("Number of Events before save: " + _eventCountBeforeSave);

			Debug.Log ("Saving world...");

			#if DEBUG

			Manager.RegisterDebugEvent = (string eventType, string message) => {

				switch (eventType) {

//				case "UpdateCellGroupEvent:CanTrigger": 
//					switch (message) {
//
//					case "BaseCanTriggerFalse":
//						_updateCellGroupEventCanTrigger_BaseCanTriggerFalse++;
//						break;
//
//					case "GroupNextUpdateDateNotTriggerDate":
//						_updateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate++;
//						break;
//
//					case "True":
//						_updateCellGroupEventCanTrigger_True++;
//						break;
//					}
//					break;

				case "DebugMessage":

					int count = 0;

					string messagePlusCount = message;

					while (_debugMessages.Contains (messagePlusCount)) {

						messagePlusCount = message + " [" + count + "]";
					}

					_debugMessages.Add (messagePlusCount);

					break;
				}
			};

			_totalCallsToAddMigratingGroup = 0;
			_totalCallsToAddGroupToUpdate = 0;
			_totalCallsToGroupUpdate = 0;

			World.AddMigratingGroupCalled = () => {
				_totalCallsToAddMigratingGroup++;
			};

			World.AddGroupToUpdateCalled = (string callingMethod) => {
				_totalCallsToAddGroupToUpdate++;

				int callCount;

				if (!_afterSave_AddGroupToUpdateCallers.TryGetValue (callingMethod, out callCount)) {

					_afterSave_AddGroupToUpdateCallers.Add (callingMethod, 1);

				} else {

					_afterSave_AddGroupToUpdateCallers[callingMethod] = ++callCount;
				}
			};

			CellGroup.UpdateCalled = () => {
				_totalCallsToGroupUpdate++;
			};

			_totalCallsToGetNextLocalRandom = 0;

			TerrainCell.GetNextLocalRandomCalled = (string callingMethod) => {
				_totalCallsToGetNextLocalRandom++;

				int callCount;

				if (!_afterSave_GetNextLocalRandomCallers.TryGetValue (callingMethod, out callCount)) {

					_afterSave_GetNextLocalRandomCallers.Add (callingMethod, 1);

				} else {

					_afterSave_GetNextLocalRandomCallers[callingMethod] = ++callCount;
				}
			};

			#endif

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

				if (c >= _checksToSkip) {
					Debug.Log ("Pushing simulation forward after save " + checkStr + "...");
				}

				#if DEBUG

				_debugMessages.Clear ();

				// We want the count of events for each check only

//				_updateCellGroupEventCanTrigger_BaseCanTriggerFalse = 0;
//				_updateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate = 0;
//				_updateCellGroupEventCanTrigger_True = 0;

				#endif

				while (_world.CurrentDate < (saveDatePlusOffset + _offsetPerCheck)) {

					_world.Iterate ();
				}

				saveDatePlusOffset = _world.CurrentDate;

				if (c < _checksToSkip)
					continue;

				_world.Synchronize ();

				_saveDatePlusOffsets[c] = saveDatePlusOffset;

				Debug.Log ("Current Date: " + _world.CurrentDate);

				_afterSave_EventCounts[c] = _world.EventsToHappenCount;
				Debug.Log ("Number of Events after Save " + checkStr + ": " + _afterSave_EventCounts[c]);

				#if DEBUG
				_afterSave_DebugMessageLists[c] = new List<string> (_debugMessages);

//				_afterSave_UpdateCellGroupEventCanTrigger_BaseCanTriggerFalseCounts[c] = _updateCellGroupEventCanTrigger_BaseCanTriggerFalse;
//				Debug.Log ("Total instances of UpdateCellGroupEventCanTrigger_BaseCanTriggerFalse events after Save " + checkStr + ": " + _afterSave_UpdateCellGroupEventCanTrigger_BaseCanTriggerFalseCounts[c]);
//
//				_afterSave_UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDateCounts[c] = _updateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate;
//				Debug.Log ("Total instances of UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate events after Save " + checkStr + ": " + _afterSave_UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDateCounts[c]);
//
//				_afterSave_UpdateCellGroupEventCanTrigger_TrueCounts[c] = _updateCellGroupEventCanTrigger_True;
//				Debug.Log ("Total instances of UpdateCellGroupEventCanTrigger_True events after Save " + checkStr + ": " + _afterSave_UpdateCellGroupEventCanTrigger_TrueCounts[c]);

				_afterSave_CallsToAddMigratingGroupCounts[c] = _totalCallsToAddMigratingGroup;
				Debug.Log ("Total calls to AddMigratingGroup after Save " + checkStr + ": " + _afterSave_CallsToAddMigratingGroupCounts[c]);

				_afterSave_CallsToAddGroupToUpdateCounts[c] = _totalCallsToAddGroupToUpdate;
				Debug.Log ("Total calls to AddGroupToUpdate after Save " + checkStr + ": " + _afterSave_CallsToAddGroupToUpdateCounts[c]);

				_afterSave_AddGroupToUpdateCallersByCheck[c] = new Dictionary<string, int>(_afterSave_AddGroupToUpdateCallers);

				_afterSave_CallsToGroupUpdateCounts[c] = _totalCallsToGroupUpdate;
				Debug.Log ("Total calls to Group Update after Save " + checkStr + ": " + _afterSave_CallsToGroupUpdateCounts[c]);

				_afterSave_LastRandomIntegers[c] = TerrainCell.LastRandomInteger;
				Debug.Log ("Last Random Integer after Save " + checkStr + ": " + _afterSave_LastRandomIntegers[c]);

				_afterSave_TotalCallsToGetNextLocalRandom[c] = _totalCallsToGetNextLocalRandom;
				Debug.Log ("Total calls to GetNextLocalRandom after Save " + checkStr + ": " + _afterSave_TotalCallsToGetNextLocalRandom[c]);

//				foreach (KeyValuePair<string, int> pair in _afterSave_GetNextLocalRandomCallers) {
//
//					Debug.Log ("Total calls by " + pair.Key + " to GetNextLocalRandom after Save " + checkStr + ": " + pair.Value);
//				}

				_afterSave_GetNextLocalRandomCallersByCheck[c] = new Dictionary<string, int>(_afterSave_GetNextLocalRandomCallers);
				#endif

				_afterSave_GroupCounts[c] = _world.CellGroupCount;
				_afterSave_PolityCounts[c] = _world.PolityCount;
				_afterSave_RegionCounts[c] = _world.RegionCount;
				_afterSave_LanguageCounts[c] = _world.LanguageCount;
				Debug.Log ("Number of Cell Groups after Save " + checkStr + ": " + _afterSave_GroupCounts[c]);
				Debug.Log ("Number of Polities after Save " + checkStr + ": " + _afterSave_PolityCounts[c]);
				Debug.Log ("Number of Regions after Save " + checkStr + ": " + _afterSave_RegionCounts[c]);
				Debug.Log ("Number of Languages after Save " + checkStr + ": " + _afterSave_LanguageCounts[c]);

				int totalCellCount = 0;
				foreach (Polity polity in _world.Polities) {

					totalCellCount += polity.Territory.CellPositions.Count;
				}

				_afterSave_TotalTerritorySizes[c] = totalCellCount;
				Debug.Log ("Total size of territories after Save " + checkStr + ": " + totalCellCount);
			}

			Debug.Log ("Loading world...");

			#if DEBUG

			Manager.RegisterDebugEvent = (string eventType, string message) => {

				switch (eventType) {

//				case "UpdateCellGroupEvent:CanTrigger": 
//					switch (message) {
//
//					case "BaseCanTriggerFalse":
//						_updateCellGroupEventCanTrigger_BaseCanTriggerFalse++;
//						break;
//
//					case "GroupNextUpdateDateNotTriggerDate":
//						_updateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate++;
//						break;
//
//					case "True":
//						_updateCellGroupEventCanTrigger_True++;
//						break;
//					}
//					break;

				case "DebugMessage":

					int count = 0;

					string messagePlusCount = message;

					while (_debugMessages.Contains (messagePlusCount)) {

						messagePlusCount = message + " [" + count + "]";
					}

					_debugMessages.Add (messagePlusCount);

					break;
				}
			};

			_totalCallsToAddMigratingGroup = 0;
			_totalCallsToAddGroupToUpdate = 0;
			_totalCallsToGroupUpdate = 0;

			World.AddMigratingGroupCalled = () => {
				_totalCallsToAddMigratingGroup++;
			};

			World.AddGroupToUpdateCalled = (string callingMethod) => {
				_totalCallsToAddGroupToUpdate++;

				int callCount;

				if (!_afterLoad_AddGroupToUpdateCallers.TryGetValue (callingMethod, out callCount)) {

					_afterLoad_AddGroupToUpdateCallers.Add (callingMethod, 1);

				} else {

					_afterLoad_AddGroupToUpdateCallers[callingMethod] = ++callCount;
				}
			};

			CellGroup.UpdateCalled = () => {
				_totalCallsToGroupUpdate++;
			};

			_totalCallsToGetNextLocalRandom = 0;

			TerrainCell.GetNextLocalRandomCalled = (string callingMethod) => {
				_totalCallsToGetNextLocalRandom++;

				int callCount;

				if (!_afterLoad_GetNextLocalRandomCallers.TryGetValue (callingMethod, out callCount)) {

					_afterLoad_GetNextLocalRandomCallers.Add (callingMethod, 1);

				} else {

					_afterLoad_GetNextLocalRandomCallers[callingMethod] = ++callCount;
				}
			};

			#endif

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

			}
			
			_eventCountAfterLoad = _world.EventsToHappen.Count;
			Debug.Log ("Number of Events after Load: " + _eventCountAfterLoad);

			if (_eventCountAfterLoad != _eventCountAfterSave) {

				Debug.LogError ("Number of events after Load different from after Save");

				_result = false;

			}

			int loadDatePlusOffset = loadDate;

			for (int c = 0; c < _numChecks; c++) {

				string checkStr = "[with offset " + c + "]";

				if (c >= _checksToSkip) {
					Debug.Log ("Pushing simulation forward after load " + checkStr + "...");
				}

				#if DEBUG

				_debugMessages.Clear ();

				// We want the count of events for each check only

//				_updateCellGroupEventCanTrigger_BaseCanTriggerFalse = 0;
//				_updateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate = 0;
//				_updateCellGroupEventCanTrigger_True = 0;

				#endif

				while (_world.CurrentDate < (loadDatePlusOffset + _offsetPerCheck)) {

					_world.Iterate ();
				}

				loadDatePlusOffset = _world.CurrentDate;

				if (c < _checksToSkip)
					continue;

				_world.Synchronize ();

				// Validate current date

				Debug.Log ("Current Date: " + _world.CurrentDate);

				if (_saveDatePlusOffsets[c] != loadDatePlusOffset) {

					Debug.LogError ("Load date after offset different from Save date with offset [" + c + "]:" + _saveDatePlusOffsets[c]);

					_result = false;

				}

				// Validate Number of Events

				Debug.Log ("Number of Events after load " + checkStr + ": " + _world.EventsToHappenCount);

				if (_afterSave_EventCounts[c] != _world.EventsToHappenCount) {

					Debug.LogError ("Number of events after load with offset not equal to: " + _afterSave_EventCounts[c]);

					_result = false;

				}

				#if DEBUG

				// Validate Debug Messages Occurrences

				foreach (string message in _debugMessages) {

					if (!_afterSave_DebugMessageLists[c].Contains (message)) {

						Debug.LogError ("Debug message from Load data not found in Save data " + checkStr + ": " + message);
						_result = false;
					}
				}

				foreach (string message in _afterSave_DebugMessageLists[c]) {

					if (!_debugMessages.Contains (message)) {

						Debug.LogError ("Debug message from Save data not found in Load data " + checkStr + ": " + message);
						_result = false;
					}
				}

//				// Validate UpdateCellGroupEventCanTrigger events
//
//				Debug.Log ("Total UpdateCellGroupEventCanTrigger_BaseCanTriggerFalse events after Load " + checkStr + ": " + _updateCellGroupEventCanTrigger_BaseCanTriggerFalse);
//
//				if (_afterSave_UpdateCellGroupEventCanTrigger_BaseCanTriggerFalseCounts[c] != _updateCellGroupEventCanTrigger_BaseCanTriggerFalse) {
//
//					Debug.LogError ("Total UpdateCellGroupEventCanTrigger_BaseCanTriggerFalse events after load with offset not equal to: " + _afterSave_UpdateCellGroupEventCanTrigger_BaseCanTriggerFalseCounts[c]);
//
//					_result = false;
//
//				}
//
//				Debug.Log ("Total UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate events after Load " + checkStr + ": " + _updateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate);
//
//				if (_afterSave_UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDateCounts[c] != _updateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate) {
//
//					Debug.LogError ("Total UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDate events after load with offset not equal to: " + _afterSave_UpdateCellGroupEventCanTrigger_GroupNextUpdateDateNotTriggerDateCounts[c]);
//
//					_result = false;
//
//				}
//
//				Debug.Log ("Total UpdateCellGroupEventCanTrigger_True events after Load " + checkStr + ": " + _updateCellGroupEventCanTrigger_True);
//
//				if (_afterSave_UpdateCellGroupEventCanTrigger_TrueCounts[c] != _updateCellGroupEventCanTrigger_True) {
//
//					Debug.LogError ("Total UpdateCellGroupEventCanTrigger_True events after load with offset not equal to: " + _afterSave_UpdateCellGroupEventCanTrigger_TrueCounts[c]);
//
//					_result = false;
//
//				}

				// Validate calls to AddMigratingGroup

				Debug.Log ("Total calls to AddMigratingGroup after Load " + checkStr + ": " + _totalCallsToAddMigratingGroup);

				if (_afterSave_CallsToAddMigratingGroupCounts[c] != _totalCallsToAddMigratingGroup) {

					Debug.LogError ("Total calls to AddMigratingGroup after load with offset not equal to: " + _afterSave_CallsToAddMigratingGroupCounts[c]);

					_result = false;

				}

				// Validate calls to AddGroupToUpdate

				Debug.Log ("Total calls to AddGroupToUpdate after Load " + checkStr + ": " + _totalCallsToAddGroupToUpdate);

				if (_afterSave_CallsToAddGroupToUpdateCounts[c] != _totalCallsToAddGroupToUpdate) {

					Debug.LogError ("Total calls to AddGroupToUpdate after load with offset not equal to: " + _afterSave_CallsToAddGroupToUpdateCounts[c]);

					_result = false;

				}

				foreach (KeyValuePair<string, int> pair in _afterLoad_AddGroupToUpdateCallers) {

					int saveCallerCount;

					if (!_afterSave_AddGroupToUpdateCallersByCheck[c].TryGetValue (pair.Key, out saveCallerCount)) {
						saveCallerCount = 0;
					}

					if (saveCallerCount != pair.Value) {

						Debug.Log ("Total calls by " + pair.Key + " to AddGroupToUpdate after Load " + checkStr + ": " + pair.Value);
						Debug.LogError ("Total calls by " + pair.Key + " to AddGroupToUpdate after load with offset not equal to: " + saveCallerCount);

						_result = false;

					}
				}

				foreach (KeyValuePair<string, int> pair in _afterSave_AddGroupToUpdateCallersByCheck[c]) {

					if (!_afterLoad_AddGroupToUpdateCallers.ContainsKey (pair.Key)) {
						Debug.Log ("Total calls by " + pair.Key + " to AddGroupToUpdate after Load " + checkStr + ": 0");
						Debug.LogError ("Total calls by " + pair.Key + " to AddGroupToUpdate after load with offset not equal to: " + pair.Value);

						_result = false;
					}
				}

				// Validate calls to CellGroup:Update

				Debug.Log ("Total calls to Group Update after Load " + checkStr + ": " + _totalCallsToGroupUpdate);

				if (_afterSave_CallsToGroupUpdateCounts[c] != _totalCallsToGroupUpdate) {

					Debug.LogError ("Total calls to Group Update after load with offset not equal to: " + _afterSave_CallsToGroupUpdateCounts[c]);

					_result = false;

				}

				// Validate Last Random Integer

				Debug.Log ("Last Random Integer after Load " + checkStr + ": " + TerrainCell.LastRandomInteger);

				/// NOTE: TerrainCell.LastRandomInteger might not be consistent because the order in which events are executed for a particular date might change after Load
				/// 	this shouldn't have any effect on the simulation
				/// 
//			if (_lastRandomIntegerAfterSave1 != _lastRandomIntegerAfterLoad1) {
//
//				Debug.LogError ("First last random integer after load not equal to: " + _lastRandomIntegerAfterSave1);
//
//				_result = false;
//
//			} else {
//
//				Debug.Log ("First last random integer after load equal");
//			}

				// Validate calls to GetNextLocalRandom

				Debug.Log ("Total calls to GetNextLocalRandom after Load " + checkStr + ": " + _totalCallsToGetNextLocalRandom);

				if (_afterSave_TotalCallsToGetNextLocalRandom[c] != _totalCallsToGetNextLocalRandom) {

					Debug.LogError ("Total calls to GetNextLocalRandom after load with offset not equal to: " + _afterSave_TotalCallsToGetNextLocalRandom[c]);

					_result = false;

				}

				foreach (KeyValuePair<string, int> pair in _afterLoad_GetNextLocalRandomCallers) {

					int saveCallerCount;

					if (!_afterSave_GetNextLocalRandomCallersByCheck[c].TryGetValue (pair.Key, out saveCallerCount)) {
						saveCallerCount = 0;
					}

					if (saveCallerCount != pair.Value) {

						Debug.Log ("Total calls by " + pair.Key + " to GetNextLocalRandom after Load " + checkStr + ": " + pair.Value);
						Debug.LogError ("Total calls by " + pair.Key + " to GetNextLocalRandom after load with offset not equal to: " + saveCallerCount);

						_result = false;

					}
				}

				foreach (KeyValuePair<string, int> pair in _afterSave_GetNextLocalRandomCallersByCheck[c]) {

					if (!_afterLoad_GetNextLocalRandomCallers.ContainsKey (pair.Key)) {
						Debug.Log ("Total calls by " + pair.Key + " to GetNextLocalRandom after Load " + checkStr + ": 0");
						Debug.LogError ("Total calls by " + pair.Key + " to GetNextLocalRandom after load with offset not equal to: " + pair.Value);

						_result = false;
					}
				}
				#endif

				// Validate Cell Groups

				Debug.Log ("Number of Cell Groups after Load " + checkStr + ": " + _world.CellGroupCount);

				if (_afterSave_GroupCounts[c] != _world.CellGroupCount) {

					Debug.LogError ("Number of cell groups after Load with offset not equal to: " + _afterSave_GroupCounts[c]);

					_result = false;

				}

				// Validate Polities

				Debug.Log ("Number of Polities after Load " + checkStr + ": " + _world.PolityCount);

				if (_afterSave_PolityCounts[c] != _world.PolityCount) {

					Debug.LogError ("Number of polities after load with offset not equal to: " + _afterSave_PolityCounts[c]);

					_result = false;

				}

				// Validate Regions

				Debug.Log ("Number of Regions after Load " + checkStr + ": " + _world.RegionCount);

				if (_afterSave_RegionCounts[c] != _world.RegionCount) {

					Debug.LogError ("Number of regions after load with offset not equal to: " + _afterSave_RegionCounts[c]);

					_result = false;

				}

				// Validate Languages

				Debug.Log ("Number of Languages after Load " + checkStr + ": " + _world.LanguageCount);

				if (_afterSave_LanguageCounts[c] != _world.LanguageCount) {

					Debug.LogError ("Number of languages after load with offset not equal to: " + _afterSave_LanguageCounts[c]);

					_result = false;

				}

				// Validate TerritorySizes

				int totalCellCount = 0;
				foreach (Polity polity in _world.Polities) {

					totalCellCount += polity.Territory.CellPositions.Count;
				}

				Debug.Log ("Total size of territories after Load " + checkStr + ": " + totalCellCount);

				if (_afterSave_TotalTerritorySizes[c] != totalCellCount) {

					Debug.LogError ("Total size of territories after load with offset not equal to: " + _afterSave_TotalTerritorySizes[c]);

					_result = false;

				}
			}

			// Validate Recorded Entries

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

			#if DEBUG
			Manager.RegisterDebugEvent = null;
			World.AddGroupToUpdateCalled = null;
			World.AddMigratingGroupCalled = null;
			CellGroup.UpdateCalled = null;
			TerrainCell.GetNextLocalRandomCalled = null;
			#endif

			break;

		default:
			throw new System.Exception ("Unrecognized test stage: " + _stage);
		}
	}
}
