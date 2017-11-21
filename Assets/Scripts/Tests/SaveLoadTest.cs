using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveLoadTest : AutomatedTest {

	public enum Stage {

		Generation,
		GenerationIterations,
		SavePrecheck,
		Save,
		SaveIterations,
		LoadPrecheck,
		Load,
		LoadIterations
	}

	public delegate bool SaveConditionDelegate (World world);

	private const int MaxIterationsPerUpdate = 500;
	private const int MaxCallsToGroupUpdate = 1000;

	private const float MaxTimePerUpdate = 0.1f;

	private bool _enhancedTracing = false;
	private bool _trackGenRandomCallers = false;

	private int _seed;

	private int _offsetPerCheck;

	private int _filteredEventCountBeforeSave;
	private int _filteredEventCountAfterSave;
	private int _filteredEventCountAfterLoad;
	private int _eventCountAfterSave;
	private int _eventCountAfterLoad;
	private int _eventCountAfterSaveSkip;
	private int _eventCountAfterLoadSkip;

	private List<WorldEvent> _eventsAfterSave;
	private List<WorldEventSnapshot> _eventSnapshotsAfterSave;

	private int _saveDate;
	private int _saveDatePlusOffset;
	private int _loadDatePlusOffset;

	private int _numChecks;
	private int _beforeCheckDateSkipOffset;
	private int _currentCheck;

	private Dictionary<string, List<string>> _debugMessages = new Dictionary<string, List<string>> ();

	private Dictionary<string, List<string>>[] _afterSave_DebugMessageLists;

	private int _totalCallsToAddMigratingGroup = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test
	private int _totalCallsToAddGroupToUpdate = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test
	private int _totalCallsToGroupUpdate = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test
	private int _totalCallsToGetNextLocalRandom = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test

	private int _callsToGroupUpdate = int.MinValue; // If a log displays a negative value for this var then something is wrong with the test

//	private Dictionary<string, int> _afterSave_AddGroupToUpdateCallers = new Dictionary<string, int> ();
//	private Dictionary<string, int> _afterLoad_AddGroupToUpdateCallers = new Dictionary<string, int> ();

//	private Dictionary<string, int>[] _afterSave_AddGroupToUpdateCallersByCheck;

	private Dictionary<string, int> _afterSave_GetNextLocalRandomCallers = new Dictionary<string, int> ();
	private Dictionary<string, int> _afterLoad_GetNextLocalRandomCallers = new Dictionary<string, int> ();

	private Dictionary<string, int>[] _afterSave_GetNextLocalRandomCallersByCheck;

	private int[] _saveDatePlusOffsets;

	private int[] _afterSave_EventCounts;
	private int[] _afterSave_CallsToAddMigratingGroupCounts;
	private int[] _afterSave_CallsToAddGroupToUpdateCounts;
	private int[] _afterSave_CallsToGroupUpdateCounts;
	private int[] _afterSave_TotalCallsToGetNextLocalRandom;
	private int[] _afterSave_GroupCounts;
	private int[] _afterSave_PolityCounts;
	private int[] _afterSave_TotalTerritorySizes;
	private int[] _afterSave_RegionCounts;
	private int[] _afterSave_LanguageCounts;

//	private TestRecorder _saveRecorder = new TestRecorder ();
//	private TestRecorder _loadRecorder = new TestRecorder ();

	private bool _result = true;

	private string _savePath;

	private World _world;

	private Stage _stage = Stage.Generation;

//	private bool _validateRecording = false;

	private SaveConditionDelegate _saveCondition;

//	public SaveLoadTest (int seed, int initialDateSkip, int offsetPerCheck, int numChecks, int checksToSkip = 0, bool enhancedTracing = false, bool trackGenRandomCallers = false, bool validateRecording = false) {
//
//		Initialize ("with initialSkip: " + initialDateSkip, 
//			seed, (World world) => {
//				return (world.CurrentDate >= initialDateSkip);
//			}, offsetPerCheck, numChecks, checksToSkip, enhancedTracing, trackGenRandomCallers, validateRecording);
//	}

	public SaveLoadTest (string conditionName, int seed, SaveConditionDelegate saveCondition, int offsetPerCheck, int numChecks, int beforeCheckDateSkipOffset = 0, bool enhancedTracing = false, bool trackGenRandomCallers = false, bool validateRecording = false) {

		Initialize (conditionName, seed, saveCondition, offsetPerCheck, numChecks, beforeCheckDateSkipOffset, enhancedTracing, trackGenRandomCallers, validateRecording);
	}

	private void Initialize (string conditionName, int seed, SaveConditionDelegate saveCondition, int offsetPerCheck, int numChecks, int beforeCheckDateSkipOffset, bool enhancedTracing, bool trackGenRandomCallers, bool validateRecording) {

		_seed = seed;

		_offsetPerCheck = offsetPerCheck;
		_numChecks = numChecks;
		_beforeCheckDateSkipOffset = beforeCheckDateSkipOffset;

		_enhancedTracing = enhancedTracing;
//		_validateRecording = validateRecording;
		_trackGenRandomCallers = trackGenRandomCallers;

		Name = "Save/Load Test " + conditionName
			+ ", world: " + seed
			+ ", numChecks: " + numChecks
			+ ", offsetPerCheck: " + offsetPerCheck
			+ ", beforeCheckDateSkipOffset: " + beforeCheckDateSkipOffset;

		if (_enhancedTracing) {
			Name += ", with enhanced tracing";
		} else {
			Name += ", without enhanced tracing";
		}

		if (_trackGenRandomCallers) {
			Name += ", tracking RNG callers";
		} else {
			Name += ", not tracking RNG callers";
		}

//		if (_validateRecording) {
//			Name += ", with recording validation";
//		} else {
//			Name += ", without recording validation";
//		}

		_saveCondition = saveCondition;

		if (_enhancedTracing) {
			_afterSave_DebugMessageLists = new Dictionary<string,List<string>>[_numChecks];

//			_afterSave_AddGroupToUpdateCallersByCheck = new Dictionary<string, int>[_numChecks];
		}

		if (_trackGenRandomCallers) {
			_afterSave_GetNextLocalRandomCallersByCheck = new Dictionary<string, int>[_numChecks];
		}
			
		_saveDatePlusOffsets = new int[_numChecks];

		_afterSave_EventCounts = new int[_numChecks];
		_afterSave_CallsToAddMigratingGroupCounts = new int[_numChecks];
		_afterSave_CallsToAddGroupToUpdateCounts = new int[_numChecks];
		_afterSave_CallsToGroupUpdateCounts = new int[_numChecks];
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

			#if DEBUG
			CellGroup.UpdateCalled = () => {
				_callsToGroupUpdate++;
			};
			#endif

			_stage = Stage.GenerationIterations;

			break;

		case Stage.GenerationIterations:
			
			_callsToGroupUpdate = 0;
			int iterations = 0;

			float startTimeUpdate = Time.realtimeSinceStartup;

			while (!_saveCondition (_world)) {

				_world.Iterate ();

				iterations++;

				if ((_callsToGroupUpdate > MaxCallsToGroupUpdate) || ((startTimeUpdate + MaxTimePerUpdate) < Time.realtimeSinceStartup)) {

//					Debug.Log ("Current Date: " + _world.CurrentDate);
//					Debug.Log ("Calls to Group Update: " + _callsToGroupUpdate + ", Iterations: " + iterations);
					return;
				}
			}

			_saveDate = _world.CurrentDate;

			Debug.Log ("Save Date: " + _saveDate);

//			#if DEBUG
//			Debug.Log ("Last Random Integer: " + TerrainCell.LastRandomInteger);
//			#endif

			_filteredEventCountBeforeSave = _world.EventsToHappenCount;
			List<WorldEvent> filteredEventsToHappen = _world.GetFilteredEventsToHappenForSerialization ();

			Debug.Log ("Number of events before save: " + _filteredEventCountBeforeSave);
			Debug.Log ("Number of filtered events before save: " + filteredEventsToHappen.Count);

			_filteredEventCountBeforeSave = filteredEventsToHappen.Count;

			Debug.Log ("Saving world...");

			Manager.SaveWorldAsync (_savePath);

			_stage = Stage.SavePrecheck;

			break;

		case Stage.SavePrecheck:

			if (Manager.PerformingAsyncTask) {
				return;
			}

			_filteredEventCountAfterSave = _world.EventsToHappen.Count;
			Debug.Log ("Number of filtered events after save: " + _filteredEventCountAfterSave);

			if (_filteredEventCountBeforeSave != _filteredEventCountAfterSave) {

				Debug.LogError ("Number of filtered events before and after save are different");

				_result = false;

			} else {

				Debug.Log ("Number of filtered events remain equal after save");
			}

			_eventCountAfterSave = _world.EventsToHappenCount;
			Debug.Log ("Number of remaining events after save: " + _eventCountAfterSave);

			_eventsAfterSave = _world.GetEventsToHappen ();
			_eventSnapshotsAfterSave = new List<WorldEventSnapshot> (_eventsAfterSave.Count);

			foreach (WorldEvent e in _eventsAfterSave) {
			
				_eventSnapshotsAfterSave.Add (e.GetSnapshot ());
			}

			Debug.Log ("Pushing simulation forward after save by at least " + _beforeCheckDateSkipOffset + " years");

			#if DEBUG
			CellGroup.UpdateCalled = () => {
				_totalCallsToGroupUpdate++;
				_callsToGroupUpdate++;
			};
			#endif

			_totalCallsToAddMigratingGroup = 0;
			_totalCallsToAddGroupToUpdate = 0;
			_totalCallsToGroupUpdate = 0;

			#if DEBUG
			World.AddMigratingGroupCalled = () => {
				_totalCallsToAddMigratingGroup++;
			};
			#endif

			#if DEBUG
			World.AddGroupToUpdateCalled = (string callingMethod) => {
				_totalCallsToAddGroupToUpdate++;

//				int callCount;
//
//				if (!_afterSave_AddGroupToUpdateCallers.TryGetValue (callingMethod, out callCount)) {
//
//					_afterSave_AddGroupToUpdateCallers.Add (callingMethod, 1);
//
//				} else {
//
//					_afterSave_AddGroupToUpdateCallers[callingMethod] = ++callCount;
//				}
			};
			#endif

			_totalCallsToGetNextLocalRandom = 0;

			#if DEBUG
			Manager.TrackGenRandomCallers = false;

			TerrainCell.GetNextLocalRandomCalled = (string callingMethod) => {
				_totalCallsToGetNextLocalRandom++;
			};
			#endif

			_stage = Stage.Save;

			break;

		case Stage.Save:

			_callsToGroupUpdate = 0;
			iterations = 0;

			startTimeUpdate = Time.realtimeSinceStartup;

			while (_world.CurrentDate < (_saveDate + _beforeCheckDateSkipOffset)) {

				_world.Iterate ();

				iterations++;

				if ((_callsToGroupUpdate > MaxCallsToGroupUpdate) || ((startTimeUpdate + MaxTimePerUpdate) < Time.realtimeSinceStartup)) {

//					Debug.Log ("Current Date: " + _world.CurrentDate);
//					Debug.Log ("Calls to Group Update: " + _callsToGroupUpdate + ", Iterations Pre Check: " + iterations);
					return;
				}
			}

			_saveDatePlusOffset = _world.CurrentDate;

			Debug.Log ("Save plus offset date: " + _saveDatePlusOffset);

			#if DEBUG
			if (_enhancedTracing) {
				Manager.RegisterDebugEvent = ProcessDebugEvent;
			}
			#endif

			#if DEBUG
			Manager.TrackGenRandomCallers = _trackGenRandomCallers;

			TerrainCell.GetNextLocalRandomCalled = (string callingMethod) => {
				_totalCallsToGetNextLocalRandom++;

				if (_trackGenRandomCallers) {
					int callCount;

					if (!_afterSave_GetNextLocalRandomCallers.TryGetValue (callingMethod, out callCount)) {

						_afterSave_GetNextLocalRandomCallers.Add (callingMethod, 1);

					} else {

						_afterSave_GetNextLocalRandomCallers[callingMethod] = ++callCount;
					}
				}
			};
			#endif

//			if (_validateRecording) {
//				Manager.Recorder = _saveRecorder;
//			}

			_currentCheck = 0;

			if (_enhancedTracing) {
				_debugMessages.Clear ();
			}

			Debug.Log ("Pushing simulation forward after save [with offset 0]...");

			_stage = Stage.SaveIterations;

			break;

		case Stage.SaveIterations:

			for (int c = _currentCheck; c < _numChecks; c++) {

				int offset = _beforeCheckDateSkipOffset + (c * _offsetPerCheck);

				string checkStr = "[with iteration offset: " + c + " - date offset: " + offset + "]";

				if (c > _currentCheck) {
					if (_enhancedTracing) {
						_debugMessages.Clear ();
					}

					Debug.Log ("Pushing simulation forward after save " + checkStr + "...");
				}

				_callsToGroupUpdate = 0;
				iterations = 0;

				startTimeUpdate = Time.realtimeSinceStartup;

				while (_world.CurrentDate < (_saveDatePlusOffset + _offsetPerCheck)) {

					_world.Iterate ();

					iterations++;

					if ((_callsToGroupUpdate > MaxCallsToGroupUpdate) || ((startTimeUpdate + MaxTimePerUpdate) < Time.realtimeSinceStartup)) {

//						Debug.Log ("Current Date: " + _world.CurrentDate);
//						Debug.Log ("Calls to Group Update: " + _callsToGroupUpdate + ", Iterations: " + iterations);
						_currentCheck = c;
						return;
					}
				}

				_saveDatePlusOffset = _world.CurrentDate;

				_world.Synchronize ();

				_saveDatePlusOffsets[c] = _saveDatePlusOffset;

				Debug.Log ("Current Date: " + _world.CurrentDate);

				_afterSave_EventCounts[c] = _world.EventsToHappenCount;
				Debug.Log ("Number of Events after Save " + checkStr + ": " + _afterSave_EventCounts[c]);

				if (_enhancedTracing) {

					int count = 0;
					foreach (KeyValuePair<string, List<string>> pair in _debugMessages) {

						count += pair.Value.Count;
					}

					Debug.Log ("Number of debugMessage objects stored for " + checkStr + ": " + _debugMessages.Count);
					Debug.Log ("Number of total messages stored for " + checkStr + ": " + count);

					_afterSave_DebugMessageLists[c] = new Dictionary<string, List<string>> (_debugMessages);
				}

				_afterSave_CallsToAddMigratingGroupCounts[c] = _totalCallsToAddMigratingGroup;
				Debug.Log ("Total calls to AddMigratingGroup after Save " + checkStr + ": " + _afterSave_CallsToAddMigratingGroupCounts[c]);

				_afterSave_CallsToAddGroupToUpdateCounts[c] = _totalCallsToAddGroupToUpdate;
				Debug.Log ("Total calls to AddGroupToUpdate after Save " + checkStr + ": " + _afterSave_CallsToAddGroupToUpdateCounts[c]);

//				_afterSave_AddGroupToUpdateCallersByCheck[c] = new Dictionary<string, int>(_afterSave_AddGroupToUpdateCallers);

				_afterSave_CallsToGroupUpdateCounts[c] = _totalCallsToGroupUpdate;
				Debug.Log ("Total calls to Group Update after Save " + checkStr + ": " + _afterSave_CallsToGroupUpdateCounts[c]);

				_afterSave_TotalCallsToGetNextLocalRandom[c] = _totalCallsToGetNextLocalRandom;
				Debug.Log ("Total calls to GetNextLocalRandom after Save " + checkStr + ": " + _afterSave_TotalCallsToGetNextLocalRandom[c]);

				if (_trackGenRandomCallers) {
					foreach (KeyValuePair<string, int> pair in _afterSave_GetNextLocalRandomCallers) {
						
						Debug.Log ("Total calls by " + pair.Key + " to GetNextLocalRandom after Save " + checkStr + ": " + pair.Value);
					}

					_afterSave_GetNextLocalRandomCallersByCheck [c] = new Dictionary<string, int> (_afterSave_GetNextLocalRandomCallers);
				}

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

			#if DEBUG
			if (_enhancedTracing) {
				Manager.RegisterDebugEvent = null;
			}
			#endif

			Debug.Log ("Loading world...");

			Manager.LoadWorldAsync (_savePath);

			_stage = Stage.LoadPrecheck;

			break;

		case Stage.LoadPrecheck:

			if (Manager.PerformingAsyncTask) {
				return;
			}

			_world = Manager.CurrentWorld;

			int loadDate = _world.CurrentDate;

			Debug.Log ("Date after Load: " + loadDate);

			if (loadDate != _saveDate) {

				Debug.LogError ("Load date different from Save date");

				_result = false;
			}

			_filteredEventCountAfterLoad = _world.EventsToHappen.Count;
			Debug.Log ("Number of filtered events after Load: " + _filteredEventCountAfterLoad);

			if (_filteredEventCountAfterLoad != _filteredEventCountAfterSave) {

				Debug.LogError ("Number of filtered events after Load (" + _filteredEventCountAfterLoad + ") different from after Save (" + _filteredEventCountAfterSave + ")");

				_result = false;
			}

			_eventCountAfterLoad = _world.EventsToHappenCount;
			Debug.Log ("Number of events after Load: " + _eventCountAfterLoad);

			if (_eventCountAfterLoad != _eventCountAfterSave) {

				Debug.LogError ("Event count after Load (" + _eventCountAfterLoad + ") different from after Save (" + _eventCountAfterSave + ")");

				_result = false;
			}

			List<WorldEvent> eventsAfterLoad = _world.GetEventsToHappen ();

			foreach (WorldEventSnapshot eSave in _eventSnapshotsAfterSave) {

				WorldEvent foundEvent = null;

				foreach (WorldEvent eLoad in eventsAfterLoad) {
				
					if ((eLoad.GetType () == eSave.EventType) && (eLoad.Id == eSave.Id)) {
					
						foundEvent = eLoad;
						break;
					}
				}

				if (foundEvent != null) {
				
					eventsAfterLoad.Remove (foundEvent);
				} else {

					Debug.LogError ("Event of type '" + eSave.EventType + "' with Id (" + eSave.Id + ") not found after Load");

					CellGroupEventSnapshot geSave = eSave as CellGroupEventSnapshot;

					if (geSave != null) {

						CellGroup g = _world.GetGroup (geSave.GroupId);

						if (g == null) {
							Debug.LogError ("No group with Id (" + geSave.GroupId + ") foun after Load");
						} else {
							bool debug = true;
						}
					}
				}
			}

			foreach (WorldEvent eLoad in eventsAfterLoad) {
				
				Debug.LogError ("Event of type '" + eLoad.GetType () + "' with Id (" + eLoad.Id + ") from Load not found after Save");
			}

			Debug.Log ("Pushing simulation forward after load by at least " + _beforeCheckDateSkipOffset + " years");

			#if DEBUG
			CellGroup.UpdateCalled = () => {
				_totalCallsToGroupUpdate++;
				_callsToGroupUpdate++;
			};
			#endif

			_totalCallsToAddMigratingGroup = 0;
			_totalCallsToAddGroupToUpdate = 0;
			_totalCallsToGroupUpdate = 0;

			#if DEBUG
			World.AddMigratingGroupCalled = () => {
				_totalCallsToAddMigratingGroup++;
			};
			#endif

			#if DEBUG
			World.AddGroupToUpdateCalled = (string callingMethod) => {
				_totalCallsToAddGroupToUpdate++;

//				int callCount;
//
//				if (!_afterLoad_AddGroupToUpdateCallers.TryGetValue (callingMethod, out callCount)) {
//
//					_afterLoad_AddGroupToUpdateCallers.Add (callingMethod, 1);
//
//				} else {
//
//					_afterLoad_AddGroupToUpdateCallers[callingMethod] = ++callCount;
//				}
			};
			#endif

			_totalCallsToGetNextLocalRandom = 0;

			#if DEBUG
			Manager.TrackGenRandomCallers = false;

			TerrainCell.GetNextLocalRandomCalled = (string callingMethod) => {
				_totalCallsToGetNextLocalRandom++;
			};
			#endif

			_stage = Stage.Load;

			break;

		case Stage.Load:

			_callsToGroupUpdate = 0;
			iterations = 0;

			startTimeUpdate = Time.realtimeSinceStartup;

			while (_world.CurrentDate < (_saveDate + _beforeCheckDateSkipOffset)) {

				_world.Iterate ();

				iterations++;

				if ((_callsToGroupUpdate > MaxCallsToGroupUpdate) || ((startTimeUpdate + MaxTimePerUpdate) < Time.realtimeSinceStartup)) {

//					Debug.Log ("Current Date: " + _world.CurrentDate);
//					Debug.Log ("Calls to Group Update: " + _callsToGroupUpdate + ", Iterations Pre Check: " + iterations);
					return;
				}
			}

			_loadDatePlusOffset = _world.CurrentDate;

			Debug.Log ("Load plus offset date: " + _loadDatePlusOffset);

			#if DEBUG
			if (_enhancedTracing) {
				Manager.RegisterDebugEvent = ProcessDebugEvent;
			}
			#endif

			#if DEBUG
			Manager.TrackGenRandomCallers = _trackGenRandomCallers;

			TerrainCell.GetNextLocalRandomCalled = (string callingMethod) => {
				_totalCallsToGetNextLocalRandom++;

				if (_trackGenRandomCallers) {
					int callCount;

					if (!_afterLoad_GetNextLocalRandomCallers.TryGetValue (callingMethod, out callCount)) {

						_afterLoad_GetNextLocalRandomCallers.Add (callingMethod, 1);

					} else {

						_afterLoad_GetNextLocalRandomCallers[callingMethod] = ++callCount;
					}
				}
			};
			#endif

//			if (_validateRecording) {
//				Manager.Recorder = _loadRecorder;
//			}

			_currentCheck = 0;

			if (_enhancedTracing) {
				_debugMessages.Clear ();
			}

			Debug.Log ("Pushing simulation forward after load [with offset 0]...");

			_stage = Stage.LoadIterations;

			break;

		case Stage.LoadIterations:

			for (int c = _currentCheck; c < _numChecks; c++) {

				int offset = _beforeCheckDateSkipOffset + (c * _offsetPerCheck);

				string checkStr = "[with iteration offset: " + c + " - date offset: " + offset + "]";

				if (c > _currentCheck) {
					if (_enhancedTracing) {
						_debugMessages.Clear ();
					}

					Debug.Log ("Pushing simulation forward after load " + checkStr + "...");
				}

				_callsToGroupUpdate = 0;
				iterations = 0;

				startTimeUpdate = Time.realtimeSinceStartup;

				while (_world.CurrentDate < (_loadDatePlusOffset + _offsetPerCheck)) {

					_world.Iterate ();

					iterations++;

					if ((_callsToGroupUpdate > MaxCallsToGroupUpdate) || ((startTimeUpdate + MaxTimePerUpdate) < Time.realtimeSinceStartup)) {

//						Debug.Log ("Current Date: " + _world.CurrentDate);
//						Debug.Log ("Calls to Group Update: " + _callsToGroupUpdate + ", Iterations: " + iterations);
						_currentCheck = c;
						return;
					}
				}

				_loadDatePlusOffset = _world.CurrentDate;

				_world.Synchronize ();

				// Validate current date

				Debug.Log ("Current Date: " + _world.CurrentDate);

				if (_saveDatePlusOffsets[c] != _loadDatePlusOffset) {

					Debug.LogError ("Load date after offset different from Save date with offset [" + c + "]:" + _saveDatePlusOffsets[c]);

					_result = false;

				}

				// Validate Number of Events

				Debug.Log ("Number of Events after load " + checkStr + ": " + _world.EventsToHappenCount);

				if (_afterSave_EventCounts[c] != _world.EventsToHappenCount) {

					Debug.LogError ("Number of events after load " + checkStr + " not equal to: " + _afterSave_EventCounts[c]);

					_result = false;

				}

				if (_enhancedTracing) {

					// Validate Debug Messages Occurrences

					int count = 0;
					int savedCount = 0;
					int savedDebugMessageObjectCount = _afterSave_DebugMessageLists [c].Count;

					foreach (KeyValuePair<string, List<string>> pair in _debugMessages) {

						count += pair.Value.Count;

						List<string> messages;

						if (!_afterSave_DebugMessageLists [c].TryGetValue (pair.Key, out messages)) {

							foreach (string message in pair.Value) {

								Debug.LogError ("Debug message of type [" + pair.Key + "] from Load data not found in Save data " + checkStr + ": " + message);
							}

							_result = false;
							continue;
						}

						savedCount += messages.Count;

						_afterSave_DebugMessageLists [c].Remove (pair.Key);

						foreach (string message in pair.Value) {
							if (!messages.Remove (message)) {

								Debug.LogError ("Debug message of type [" + pair.Key + "] from Load data not found in Save data " + checkStr + ": " + message);
								_result = false;
							}
						}

						foreach (string message in messages) {
							Debug.LogError ("Debug message of type [" + pair.Key + "] from Save data not found in Load data " + checkStr + ": " + message);
							_result = false;
						}
					}

					foreach (KeyValuePair<string, List<string>> pair in _afterSave_DebugMessageLists[c]) {

						savedCount += pair.Value.Count;

						foreach (string message in pair.Value) {

							Debug.LogError ("Debug message of type [" + pair.Key + "] from Save data not found in Load data " + checkStr + ": " + message);
						}

						_result = false;
					}

					Debug.Log ("Number of debugMessage objects stored for " + checkStr + " after Load: " + _debugMessages.Count);

					if (_debugMessages.Count != savedDebugMessageObjectCount) {

						Debug.LogError ("Number of debugMessage objects stored for " + checkStr + " after Load no equal to : " + savedDebugMessageObjectCount);

						_result = false;

					}

					Debug.Log ("Number of total messages stored for " + checkStr + " after Load: " + count);

					if (count != savedCount) {

						Debug.LogError ("Number of total messages stored for " + checkStr + " after Load no equal to : " + savedCount);

						_result = false;

					}
				}

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

//				foreach (KeyValuePair<string, int> pair in _afterLoad_AddGroupToUpdateCallers) {
//
//					int saveCallerCount;
//
//					if (!_afterSave_AddGroupToUpdateCallersByCheck[c].TryGetValue (pair.Key, out saveCallerCount)) {
//						saveCallerCount = 0;
//					}
//
//					if (saveCallerCount != pair.Value) {
//
//						Debug.Log ("Total calls by " + pair.Key + " to AddGroupToUpdate after Load " + checkStr + ": " + pair.Value);
//						Debug.LogError ("Total calls by " + pair.Key + " to AddGroupToUpdate after load with offset not equal to: " + saveCallerCount);
//
//						_result = false;
//
//					}
//				}
//
//				foreach (KeyValuePair<string, int> pair in _afterSave_AddGroupToUpdateCallersByCheck[c]) {
//
//					if (!_afterLoad_AddGroupToUpdateCallers.ContainsKey (pair.Key)) {
//						Debug.Log ("Total calls by " + pair.Key + " to AddGroupToUpdate after Load " + checkStr + ": 0");
//						Debug.LogError ("Total calls by " + pair.Key + " to AddGroupToUpdate after load with offset not equal to: " + pair.Value);
//
//						_result = false;
//					}
//				}

				// Validate calls to CellGroup:Update

				Debug.Log ("Total calls to Group Update after Load " + checkStr + ": " + _totalCallsToGroupUpdate);

				if (_afterSave_CallsToGroupUpdateCounts[c] != _totalCallsToGroupUpdate) {

					Debug.LogError ("Total calls to Group Update after load with offset not equal to: " + _afterSave_CallsToGroupUpdateCounts[c]);

					_result = false;

				}


				/// NOTE: TerrainCell.LastRandomInteger might not be consistent because the order in which events are executed for a particular date might change after Load
				/// 	this shouldn't have any effect on the simulation
				/// 
//				// Validate Last Random Integer
//
//				Debug.Log ("Last Random Integer after Load " + checkStr + ": " + TerrainCell.LastRandomInteger);
//
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

					Debug.LogError ("Total calls to GetNextLocalRandom after Load " + checkStr + ": " + _totalCallsToGetNextLocalRandom + " not equal to: " + _afterSave_TotalCallsToGetNextLocalRandom[c]);

					_result = false;

				}

				if (_trackGenRandomCallers) {
					foreach (KeyValuePair<string, int> pair in _afterLoad_GetNextLocalRandomCallers) {

						int saveCallerCount;

						if (!_afterSave_GetNextLocalRandomCallersByCheck [c].TryGetValue (pair.Key, out saveCallerCount)) {
							saveCallerCount = 0;
						}

						if (saveCallerCount != pair.Value) {
							Debug.LogError ("Total calls by " + pair.Key + " to GetNextLocalRandom after Load " + checkStr + ": " + pair.Value + " not equal to: " + saveCallerCount);

							_result = false;

						}
					}

					foreach (KeyValuePair<string, int> pair in _afterSave_GetNextLocalRandomCallersByCheck[c]) {

						if (!_afterLoad_GetNextLocalRandomCallers.ContainsKey (pair.Key)) {
							Debug.LogError ("Total calls by " + pair.Key + " to GetNextLocalRandom after Load " + checkStr + ": 0 not equal to: " + pair.Value);

							_result = false;
						}
					}
				}

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

				if (!_result) {
					Debug.Log ("Interrupting Test after encountering discrepancy");
					break;
				}
			}

//			// Validate Recorded Entries
//
//			if (_validateRecording) {
//				int saveEntryCount = _saveRecorder.GetEntryCount ();
//				int loadEntryCount = _loadRecorder.GetEntryCount ();
//
//				if (saveEntryCount != loadEntryCount) {
//
//					Debug.LogError ("Number of Test Recorder entries different: save=" + saveEntryCount + " load=" + loadEntryCount);
//				} else {
//					Debug.Log ("Number of Test Recorder entries: " + saveEntryCount);
//				}
//
//				int maxEntryErrors = 5;
//				int entryErrorCount = 0;
//
//				foreach (KeyValuePair<string,string> pair in _saveRecorder.RecordedData) {
//			
//					string entryKey = pair.Key;
//					string saveEntry = pair.Value;
//					string loadEntry = _loadRecorder.Recover (entryKey);
//
//					if (saveEntry != loadEntry) {
//					
//						if (entryErrorCount < maxEntryErrors) {
//							Debug.LogError ("Entries with key [" + entryKey + "] different...\n\tSave entry: " + saveEntry + "\n\tLoad entry: " + loadEntry);
//						}
//
//						entryErrorCount++;
//					}
//				}
//
//				entryErrorCount += Mathf.Abs (saveEntryCount - loadEntryCount);
//
//				if (entryErrorCount > maxEntryErrors) {
//					Debug.LogError ("Entry errors not displayed: " + (entryErrorCount - maxEntryErrors));
//				}
//
//				if (entryErrorCount > 0) {
//					Debug.LogError ("Total entry error count: " + entryErrorCount);
//				} else {
//					Debug.Log ("Total entry error count: " + entryErrorCount);
//				}
//
//				Manager.Recorder = DefaultRecorder.Default;
//			}

			if (_result)
				State = TestState.Succeded;
			else
				State = TestState.Failed;

			#if DEBUG
			Manager.TrackGenRandomCallers = false;
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

	public class DebugMessage
	{
		public string Identifier;
		public string Message;

		public DebugMessage (string identifier, string message) {

			Identifier = identifier;
			Message = message;
		}
	}

	private void ProcessDebugEvent (string eventType, object data) {

		switch (eventType) {

		case "DebugMessage":

			int count = 0;

			DebugMessage debugMessage = data as DebugMessage;

			string identifier = debugMessage.Identifier;
			string message = debugMessage.Message;
			string messagePlusCount = message + " [Count:" + count++ + "]";

			List<string> messages;

			if (_debugMessages.TryGetValue (identifier, out messages)) {

				while (messages.Contains (messagePlusCount)) {

					messagePlusCount = message + " [Count:" + count++ + "]";
				}

				messages.Add (messagePlusCount);

			} else {

				messages = new List<string> ();
				messages.Add (messagePlusCount);

				_debugMessages.Add (identifier, messages);
			}

			break;
		}
	}
}
