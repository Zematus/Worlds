using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TestState {

	NotStarted,
	Running,
	Failed,
	Succeded
}

public abstract class AutomatedTest {

	public string Name { get; protected set; }

	public TestState State { get; protected set; }

	public abstract void Run ();

	protected AutomatedTest () {
	
		State = TestState.NotStarted;
	}
}

public class LanguageGenerationTest : AutomatedTest {

	public LanguageGenerationTest () {
	
		Name = "Languange Generation Test";
	}

	public float GetRandomFloat () {
	
		return Random.Range (0, int.MaxValue) / (float)int.MaxValue;
	}

	public override void Run ()
	{
		State = TestState.Running;

//		for (int i = 0; i < 10; i++) {
//
//			Language.CharacterGroup charGroup = Language.GenerateRandomCharacterGroup (Language.OnsetLetters, 1.0f, 0.25f, GetRandomFloat);
//
//			Debug.Log ("Generated Character Group: " + charGroup.Characters + ", Chance: " + charGroup.Weight);
//		}

//		Language.CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.25f, GetRandomFloat, 10);
//		Language.CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, GetRandomFloat, 5);
//		Language.CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.5f, 0.25f, GetRandomFloat, 4);
//
////		for (int i = 0; i < 100; i++) {
////			string syllabe = Language.GenerateRandomSyllable (onsetGroups, nucleusGroups, codaGroups, GetRandomFloat);
////
////			Debug.Log ("Generated Syllabe: " + syllabe);
////		}
//
//		string[] startSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, GetRandomFloat, 50);
//		string[] nextSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, GetRandomFloat, 50);
//
//		for (int i = 0; i < 100; i++) {
//			string word = Language.GenerateSimpleWord (startSyllables, nextSyllables, 0.25f, GetRandomFloat);
//
//			Debug.Log ("Generated Word: " + word);
//		}

		for (int i = 0; i < 5; i++) {
			Language language = new Language ();

			language.GenerateGeneralArticleProperties (GetRandomFloat);
			language.GenerateArticleSyllables (GetRandomFloat);
			language.GenerateArticles (GetRandomFloat);

			string entry = "Articles Set " + i;
			entry += "\nGeneral properties: " + language.GeneralArticleProperties;

			foreach (KeyValuePair<string, Language.Word> pair in language.Articles) {

				entry += "\n\t" + pair.Key + " : " + pair.Value.String;
			}

			Debug.Log (entry);
		}

		State = TestState.Succeded;
	}
}

public class SaveLoadTest : AutomatedTest {

	public enum Stage {

		Generation,
		Save,
		Load
	}

	private int _initialSkip = 80;
	private int _firstOffset = 1;
	private int _secondOffset = 1;

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

	private int _numGroupsAfterSaveOffset2;
	private int _numGroupsAfterLoadOffset2;

	private int _numPolitiesAfterSaveOffset2;
	private int _numPolitiesAfterLoadOffset2;

	private int _saveDate;
	private int _saveDatePlusOffset;

	private TestRecorder _saveRecorder = new TestRecorder ();
	private TestRecorder _loadRecorder = new TestRecorder ();

	private bool _result = true;

	private string _savePath;

	private World _world;

	private Stage _stage = Stage.Generation;

	private bool _validateRecording = false;

	public SaveLoadTest (int initialSkip, int firstOffset, int secondOffset, bool validateRecording) {

		_initialSkip = initialSkip;
		_firstOffset = firstOffset;
		_secondOffset = secondOffset;

		_validateRecording = validateRecording;

		Name = "Save/Load Test with initialSkip: " + initialSkip
			+ ", firstOffset: " + firstOffset
			+ ",  _secondOffset: " + _secondOffset;

		if (_validateRecording) {
			Name += " with recording validation";
		} else {
			Name += " without recording validation";
		}
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

			while (_world.CurrentDate < _initialSkip) {

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

			Debug.Log ("Pushing simulation forward after save...");

			while (_world.CurrentDate < (_saveDate + _firstOffset)) {

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

			while (_world.CurrentDate < (_saveDatePlusOffset + _secondOffset)) {

				Manager.CurrentWorld.Iterate ();
			}

			Debug.Log ("Current Date: " + _world.CurrentDate);

			_eventCountAfterSaveOffset2 = _world.EventsToHappenCount;
			Debug.Log ("Number of Events after save with offset 2: " + _eventCountAfterSaveOffset2);

			#if DEBUG
			_lastRandomIntegerAfterSave2 = TerrainCell.LastRandomInteger;
			Debug.Log ("Last Random Integer after Save with offset 2: " + _lastRandomIntegerAfterSave2);
			#endif

			_numGroupsAfterSaveOffset2 = _world.CellGroupCount;
			_numPolitiesAfterSaveOffset2 = _world.PolityCount;
			Debug.Log ("Number of Cell Groups after save with offset 2: " + _numGroupsAfterSaveOffset2);
			Debug.Log ("Number of Polities after save with offset 2: " + _numPolitiesAfterSaveOffset2);

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

			while (_world.CurrentDate < (_saveDate + _firstOffset)) {

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

			Debug.Log ("Pushing simulation forward after load again...");

			while (_world.CurrentDate < (_saveDatePlusOffset + _secondOffset)) {

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

			/// NOTE: TerrainCell.LastRandomInteger might not be consistent because the order in which events are executed for a particular date might change after Load
			/// 	this shouldn't have any effect on the simulation
			/// 
//			if (_lastRandomIntegerAfterSave2 != _lastRandomIntegerAfterLoad2) {
//
//				Debug.LogError ("Second last random integer after load not equal to : " + _lastRandomIntegerAfterSave2);
//
//				_result = false;
//
//			} else {
//
//				Debug.Log ("Second last random integer after load equal");
//			}
			#endif

			_numGroupsAfterLoadOffset2 = _world.CellGroupCount;
			Debug.Log ("Number of Cell Groups after load with offset 2: " + _numGroupsAfterLoadOffset2);

			if (_numGroupsAfterSaveOffset2 != _numGroupsAfterLoadOffset2) {

				Debug.LogError ("Second number of cell groups after load with offset not equal to : " + _numGroupsAfterSaveOffset2);

				_result = false;

			} else {

				Debug.Log ("Second number of cell groups after load with offset equal");
			}

			_numPolitiesAfterLoadOffset2 = _world.PolityCount;
			Debug.Log ("Number of Polities after load with offset 2: " + _numPolitiesAfterLoadOffset2);

			if (_numPolitiesAfterSaveOffset2 != _numPolitiesAfterLoadOffset2) {

				Debug.LogError ("Second number of polities after load with offset not equal to : " + _numPolitiesAfterSaveOffset2);

				_result = false;

			} else {

				Debug.Log ("Second number of polities after load with offset equal");
			}

			if (_validateRecording) {
				int saveEntryCount = _saveRecorder.GetEntryCount ();
				int loadEntryCount = _loadRecorder.GetEntryCount ();

				int minEntries = Mathf.Min (saveEntryCount, loadEntryCount);

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
