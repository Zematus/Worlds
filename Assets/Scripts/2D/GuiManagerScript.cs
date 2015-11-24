using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public delegate void PostProgressOperation ();

public delegate void MouseClickOperation (Vector2 position);

public class GuiManagerScript : MonoBehaviour {

	public Text MapViewButtonText;

	public Text InfoPanelText;

	public RawImage MapImage;

	public Button LoadButton;

	public PlanetScript PlanetScript;
	public MapScript MapScript;
	
	public TextInputDialogPanelScript SaveFileDialogPanelScript;
	public TextInputDialogPanelScript ExportMapDialogPanelScript;
	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public OverlayDialogPanelScript OverlayDialogPanelScript;
	public DialogPanelScript ViewsDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;
	public DialogPanelScript OptionsDialogPanelScript;
	public ProgressDialogPanelScript ProgressDialogPanelScript;
	public ActivityDialogPanelScript ActivityDialogPanelScript;
	public TextInputDialogPanelScript MessageDialogPanelScript;
	public WorldCustomizationDialogPanelScript SetSeedDialogPanelScript;
	public WorldCustomizationDialogPanelScript CustomizeWorldDialogPanelScript;
	public AddPopulationDialogScript AddPopulationDialogScript;

	public PaletteScript BiomePaletteScript;
	public PaletteScript MapPaletteScript;

	public SelectionPanelScript SelectionPanelScript;

	public QuickTipPanelScript QuickTipPanelScript;

	private bool _displayedTip_mapScroll = false;
	private bool _displayedTip_initialPopulation = false;
	
	private PlanetView _planetView = PlanetView.Biomes;
	private PlanetOverlay _planetOverlay = PlanetOverlay.Population;
	private string _planetOverlaySubtype = "None";

	private bool menusNeedUpdate = true;

	private bool _regenTextures = false;

	private Vector2 _beginDragPosition;
	private Rect _beginDragMapUvRect;

	private bool _displayProgressDialogs = false;
	
	private string _progressMessage = null;
	private float _progressValue = 0;

	private PostProgressOperation _postProgressOp = null;

	private MouseClickOperation _mapLeftClickOperation = null;
	
	private const float _maxAccTime = 0.0f;
	private const int _iterationsPerRefresh = 5;

	private float _accDeltaTime = 0;
	private int _accIterations = 0;

	private int _mapUpdateCount = 0;
	private int _lastMapUpdateCount = 0;
	private float _timeSinceLastMapUpdate = 0;

	// Use this for initialization
	void Start () {

		Manager.UpdateMainThreadReference ();
		
		SaveFileDialogPanelScript.SetVisible (false);
		ExportMapDialogPanelScript.SetVisible (false);
		LoadFileDialogPanelScript.SetVisible (false);
		OverlayDialogPanelScript.SetVisible (false);
		ViewsDialogPanelScript.SetVisible (false);
		MainMenuDialogPanelScript.SetVisible (false);
		ProgressDialogPanelScript.SetVisible (false);
		ActivityDialogPanelScript.SetVisible (false);
		OptionsDialogPanelScript.SetVisible (false);
		SetSeedDialogPanelScript.SetVisible (false);
		CustomizeWorldDialogPanelScript.SetVisible (false);
		MessageDialogPanelScript.SetVisible (false);
		AddPopulationDialogScript.SetVisible (false);

		QuickTipPanelScript.SetVisible (false);
		
		if (!Manager.WorldReady) {

			GenerateWorld ();
		} else if (!Manager.SimulationCanRun) {

			SetInitialPopulation ();
		} else {

			DisplayTip_MapScroll ();
		}

		UpdateMapViewButtonText ();

		LoadButton.interactable = HasFilesToLoad ();

		Manager.SetBiomePalette (BiomePaletteScript.Colors);
		Manager.SetMapPalette (MapPaletteScript.Colors);

		_regenTextures = true;
	}
	
	// Update is called once per frame
	void Update () {

		_timeSinceLastMapUpdate += Time.deltaTime;

		if (_timeSinceLastMapUpdate > 1) {
		
			_lastMapUpdateCount = _mapUpdateCount;
			_mapUpdateCount = 0;

			_timeSinceLastMapUpdate -= 1;
		}

		UpdateMenus ();

		Manager.ExecuteTasks (100);
		
		if (_displayProgressDialogs) {
			
			if (_progressMessage != null) ProgressDialogPanelScript.SetDialogText (_progressMessage);
			
			ProgressDialogPanelScript.SetProgress (_progressValue);
		}
		
		if (!Manager.WorldReady) {
			return;
		}
		
		if (Manager.PerformingAsyncTask) {
			return;
		}

		if (_displayProgressDialogs) {

			ProgressDialogPanelScript.SetVisible (false);
			ActivityDialogPanelScript.SetVisible (false);
			_displayProgressDialogs = false;
			
			if (_postProgressOp != null) 
				_postProgressOp ();

		}
		
		bool updateTextures = false;

		_accDeltaTime += Time.deltaTime;

		if (_accDeltaTime > _maxAccTime) {

			if (Manager.SimulationCanRun && Manager.SimulationRunning) {

				int minDateSpan = CellGroup.GenerationTime * 1000;
				int lastUpdateDate = Manager.CurrentWorld.CurrentDate;

				float startIterations = Time.realtimeSinceStartup;
				float maxDeltaIterations = 0.02f;

				while ((lastUpdateDate + minDateSpan) >= Manager.CurrentWorld.CurrentDate) {

					Manager.CurrentWorld.Iterate();

					float deltaIterations = Time.realtimeSinceStartup - startIterations;

					if (deltaIterations > maxDeltaIterations)
						break;
				}
				
				updateTextures = true;
			}
			
			_accDeltaTime -= _maxAccTime;
			_accIterations++;
		}
	
		if (_regenTextures) {
			_regenTextures = false;

			Manager.SetPlanetOverlay (_planetOverlay, _planetOverlaySubtype);
			Manager.SetPlanetView (_planetView);

			Manager.GenerateTextures ();

			//PlanetScript.RefreshTexture ();
			MapScript.RefreshTexture ();

			_mapUpdateCount++;

		} else if (updateTextures) {

			if (_accIterations >= _iterationsPerRefresh)
			{
				_accIterations -= _iterationsPerRefresh;

				if ((_planetOverlay == PlanetOverlay.Population) || 
				    (_planetOverlay == PlanetOverlay.CulturalSkill) || 
				    (_planetOverlay == PlanetOverlay.CulturalKnowledge)) {
					Manager.UpdateTextures ();
					
					_mapUpdateCount++;
				}
			}
		}

		if (MapImage.enabled) {
			UpdateInfoPanel();
			UpdateSelectionMenu();
		}
	}

	public void ProgressUpdate (float value, string message = null, bool reset = false) {
		
		if (reset || (value >= _progressValue)) {
			
			if (message != null) 
				_progressMessage = message;
			
			_progressValue = value;
		}
	}
	
	public void CloseMainMenu () {
		
		MainMenuDialogPanelScript.SetVisible (false);
		
		Manager.InterruptSimulation (false);
	}
	
	public void CloseOptionsMenu () {
		
		OptionsDialogPanelScript.SetVisible (false);
	}
	
	public void Exit () {
		
		Application.Quit();
	}

	public void SetGenerationSeed () {
		
		MainMenuDialogPanelScript.SetVisible (false);

		int seed = Random.Range (0, int.MaxValue);
		
		SetSeedDialogPanelScript.SetSeedString (seed.ToString());

		SetSeedDialogPanelScript.SetVisible (true);
		
		Manager.InterruptSimulation (true);

	}
	
	public void CancelGenerateAction () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		CustomizeWorldDialogPanelScript.SetVisible (false);
		
		Manager.InterruptSimulation (false);
	}
	
	public void CloseSeedErrorMessageAction () {
		
		MessageDialogPanelScript.SetVisible (false);

		SetGenerationSeed ();
	}
	
	public void GenerateWorld () {

		int seed = Random.Range (0, int.MaxValue);
		
		GenerateWorldInternal (seed);
	}
	
	public void GenerateWorldWithCustomSeed () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		
		int seed = 0;
		string seedStr = SetSeedDialogPanelScript.GetSeedString ();
		
		if (!int.TryParse (seedStr, out seed)) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		GenerateWorldInternal (seed);
	}
	
	public void GenerateWorldWithCustomParameters () {
		
		CustomizeWorldDialogPanelScript.SetVisible (false);
		
		Manager.TemperatureOffset = CustomizeWorldDialogPanelScript.TemperatureOffset;
		Manager.RainfallOffset = CustomizeWorldDialogPanelScript.RainfallOffset;
		Manager.SeaLevelOffset = CustomizeWorldDialogPanelScript.SeaLevelOffset;
		
		int seed = 0;
		string seedStr = CustomizeWorldDialogPanelScript.GetSeedString ();
		
		if (!int.TryParse (seedStr, out seed)) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		GenerateWorldInternal (seed);
	}
	
	private void GenerateWorldInternal (int seed) {
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Generating World...", true);
		
		Manager.GenerateNewWorldAsync (seed, ProgressUpdate);
		
		_postProgressOp = () => {
			
			Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;
			
			SelectionPanelScript.RemoveAllOptions ();

			SetInitialPopulation();
			
			_postProgressOp = null;
		};
		
		_displayProgressDialogs = true;
		
		_regenTextures = true;
	}

	private void SetInitialPopulation () {

		AddPopulationDialogScript.SetDialogText ("Add Initial Population Group");

		int defaultPopulationValue = (int)Mathf.Ceil (World.StartPopulationDensity * TerrainCell.MaxArea);

		defaultPopulationValue = Mathf.Clamp (defaultPopulationValue, World.MinStartingPopulation, World.MaxStartingPopulation);

		AddPopulationDialogScript.SetPopulationValue (defaultPopulationValue);
	
		AddPopulationDialogScript.SetVisible (true);
		
		Manager.InterruptSimulation (true);
	}

	public void CancelPopulationPlacement () {
		
		AddPopulationDialogScript.SetVisible (false);

		DisplayTip_MapScroll ();
	}
	
	public void RandomPopulationPlacement () {

		int population = AddPopulationDialogScript.Population;
		
		AddPopulationDialogScript.SetVisible (false);

		if (population <= 0)
			return;

		Manager.GenerateRandomHumanGroup (population);
		
		Manager.InterruptSimulation (false);
		
		DisplayTip_MapScroll ();
	}
	
	public void SelectPopulationPlacement () {
		
		int population = AddPopulationDialogScript.Population;
		
		AddPopulationDialogScript.SetVisible (false);
		
		if (population <= 0)
			return;

		DisplayTip_InitialPopulationPlacement ();

		_mapLeftClickOperation = (position) => {
			
			Vector2 point;
			
			if (GetMapCoordinatesFromCursor (out point)) {
				if (AddPopulationGroupAtPosition (point, population))
				{
					_mapLeftClickOperation = null;
					
					Manager.InterruptSimulation (false);

					DisplayTip_MapScroll();
				}
			}
		};
	}
	
	public bool AddPopulationGroupAtPosition (Vector2 mapPosition, int population) {

		World world = Manager.CurrentWorld;
		
		int longitude = (int)mapPosition.x;
		int latitude = (int)mapPosition.y;
		
		if ((longitude < 0) || (longitude >= world.Width))
			return false;
		
		if ((latitude < 0) || (latitude >= world.Height))
			return false;

		TerrainCell cell = world.GetCell (longitude, latitude);

		if (cell.Altitude <= Biome.Ocean.MaxAltitude)
			return false;

		Manager.GenerateHumanGroup (longitude, latitude, population);

		return true;
	}

	public void DisplayTip_InitialPopulationPlacement () {
		
		if (_displayedTip_initialPopulation) {
			
			QuickTipPanelScript.SetVisible (false);
			return;
		}

		string message = "Left click on any non-ocean position in the map to place the initial population group\n";

		if (!_displayedTip_mapScroll) {
		
			message += "Right click and drag with the mouse to scroll the map left or right\n";
		}

		message += "\n(Click anywhere on this message to close)";
	
		QuickTipPanelScript.SetText (message);
		QuickTipPanelScript.Reset (10);

		QuickTipPanelScript.SetVisible (true);
		
		_displayedTip_initialPopulation = true;
		_displayedTip_mapScroll = true;
	}
	
	public void DisplayTip_MapScroll () {

		if (_displayedTip_mapScroll) {
			
			QuickTipPanelScript.SetVisible (false);
			return;
		}
		
		QuickTipPanelScript.SetText (
			"Right click and drag with the mouse to scroll the map left or right\n" +
			"\n(Click anywhere on this message to close)");
		QuickTipPanelScript.Reset (10);
		
		QuickTipPanelScript.SetVisible (true);

		_displayedTip_mapScroll = true;
	}
	
	public void CustomizeGeneration () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		
		string seedStr = SetSeedDialogPanelScript.GetSeedString ();
		
		CustomizeWorldDialogPanelScript.SetVisible (true);
		
		CustomizeWorldDialogPanelScript.SetSeedString (seedStr);
		
		CustomizeWorldDialogPanelScript.SetTemperatureOffset(Manager.TemperatureOffset);
		CustomizeWorldDialogPanelScript.SetRainfallOffset(Manager.RainfallOffset);
		CustomizeWorldDialogPanelScript.SetSeaLevelOffset(Manager.SeaLevelOffset);
		
		Manager.InterruptSimulation (true);
	}

	private bool HasFilesToLoad () {

		string dirPath = Manager.SavePath;
		
		string[] files = Directory.GetFiles (dirPath, "*.PLNT");

		return files.Length > 0;
	}
	
	public void ExportMapAction () {
		
		ExportMapDialogPanelScript.SetVisible (false);
		
		ActivityDialogPanelScript.SetVisible (true);
		
		ActivityDialogPanelScript.SetDialogText ("Exporting map to PNG file...");
		
		string imageName = ExportMapDialogPanelScript.GetName ();
		
		string path = Manager.ExportPath + imageName + ".png";
		
		Manager.ExportMapTextureToFileAsync (path, MapImage.uvRect);
		
		_displayProgressDialogs = true;
	}
	
	public void CancelExportAction () {
		
		ExportMapDialogPanelScript.SetVisible (false);
	}
	
	public void ExportImageAs () {
		
		OptionsDialogPanelScript.SetVisible (false);
		
		string planetViewStr = "";
		
		switch (_planetView) {
		case PlanetView.Biomes: planetViewStr = "_biomes"; break;
		case PlanetView.Coastlines: planetViewStr = "_coastlines"; break;
		case PlanetView.Elevation: planetViewStr = "_elevation"; break;
		default: throw new System.Exception("Unexpected planet view type: " + _planetView);
		}
		
		string planetOverlayStr;
		
		switch (_planetOverlay) {
		case PlanetOverlay.None: planetOverlayStr = ""; break;
		case PlanetOverlay.Rainfall: planetOverlayStr = "_rainfall"; break;
		case PlanetOverlay.Temperature: planetOverlayStr = "_temperature"; break;
		case PlanetOverlay.Population: planetOverlayStr = "_population"; break;
		case PlanetOverlay.CulturalSkill: 
			planetOverlayStr = "_cultural_skill_" + _planetOverlaySubtype; 
			break;
		case PlanetOverlay.CulturalKnowledge: 
			planetOverlayStr = "_cultural_knowledge_" + _planetOverlaySubtype; 
			break;
		default: throw new System.Exception("Unexpected planet overlay type: " + _planetOverlay);
		}

		ExportMapDialogPanelScript.SetName (Manager.WorldName + planetViewStr + planetOverlayStr);
		
		ExportMapDialogPanelScript.SetVisible (true);
	}

	public void SaveAction () {
		
		SaveFileDialogPanelScript.SetVisible (false);
		
		ActivityDialogPanelScript.SetVisible (true);
		
		ActivityDialogPanelScript.SetDialogText ("Saving World...");
		
		Manager.WorldName = SaveFileDialogPanelScript.GetName ();
		
		string path = Manager.SavePath + Manager.WorldName + ".plnt";

		Manager.SaveWorldAsync (path);
		
		_postProgressOp = () => {

			LoadButton.interactable = HasFilesToLoad ();
			
			_postProgressOp = null;
		};
		
		_displayProgressDialogs = true;
	}

	public void CancelSaveAction () {
		
		SaveFileDialogPanelScript.SetVisible (false);
		
		Manager.InterruptSimulation (false);
	}

	public void SaveWorldAs () {

		MainMenuDialogPanelScript.SetVisible (false);

		SaveFileDialogPanelScript.SetName (Manager.WorldName);
		
		SaveFileDialogPanelScript.SetVisible (true);

		Manager.InterruptSimulation (true);
	}
	
	public void LoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Loading World...", true);
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();
		
		Manager.LoadWorldAsync (path, ProgressUpdate);
		
		Manager.WorldName = Path.GetFileNameWithoutExtension (path);
		
		_postProgressOp = () => {
			
			SelectionPanelScript.RemoveAllOptions ();

			if (!Manager.SimulationCanRun) {
				
				SetInitialPopulation();
			}
			
			_postProgressOp = null;
		};
		
		_displayProgressDialogs = true;
		
		_regenTextures = true;
	}
	
	public void CancelLoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		Manager.InterruptSimulation (false);
	}
	
	public void LoadWorld () {

		MainMenuDialogPanelScript.SetVisible (false);
		
		LoadFileDialogPanelScript.SetVisible (true);

		LoadFileDialogPanelScript.SetLoadAction (LoadAction);
		
		Manager.InterruptSimulation (true);
	}
	
	public void CloseOverlayMenuAction () {
		
		SelectionPanelScript.RemoveAllOptions ();
		SelectionPanelScript.SetVisible (false);

		if (OverlayDialogPanelScript.RainfallToggle.isOn) {
			SetRainfallOverlay ();
		} else if (OverlayDialogPanelScript.TemperatureToggle.isOn) {
			SetTemperatureOverlay ();
		} else if (OverlayDialogPanelScript.PopulationToggle.isOn) {
			SetPopulationOverlay ();
		} else if (OverlayDialogPanelScript.CulturalSkillToggle.isOn) {
			SetCulturalSkillOverlay ();
		} else if (OverlayDialogPanelScript.CulturalKnowledgeToggle.isOn) {
			SetCulturalKnowledgeOverlay ();
		} else {
			UnsetOverlay();
		}
		
		OverlayDialogPanelScript.SetVisible (false);
	}
	
	public void UpdateMenus () {

		if (!menusNeedUpdate)
			return;

		menusNeedUpdate = false;
		
		OverlayDialogPanelScript.CulturalKnowledgeToggle.isOn = false;
		OverlayDialogPanelScript.CulturalSkillToggle.isOn = false;
		OverlayDialogPanelScript.PopulationToggle.isOn = false;
		OverlayDialogPanelScript.RainfallToggle.isOn = false;
		OverlayDialogPanelScript.TemperatureToggle.isOn = false;
		
		SelectionPanelScript.SetVisible (false);

		switch (_planetOverlay) {
			
		case PlanetOverlay.CulturalKnowledge:
			OverlayDialogPanelScript.CulturalKnowledgeToggle.isOn = true;
			
			SelectionPanelScript.SetVisible (true);
			break;
			
		case PlanetOverlay.CulturalSkill:
			OverlayDialogPanelScript.CulturalSkillToggle.isOn = true;

			SelectionPanelScript.SetVisible (true);
			break;
		
		case PlanetOverlay.Population:
			OverlayDialogPanelScript.PopulationToggle.isOn = true;
			break;
			
		case PlanetOverlay.Rainfall:
			OverlayDialogPanelScript.RainfallToggle.isOn = true;
			break;
			
		case PlanetOverlay.Temperature:
			OverlayDialogPanelScript.TemperatureToggle.isOn = true;
			break;
			
		case PlanetOverlay.None:
			break;
			
		default:
			throw new System.Exception ("Unhandled Planet Overlay type: " + _planetOverlay);
		}
	}
	
	public void SelectOverlays () {
		
		OverlayDialogPanelScript.SetVisible (true);
	}
	
	public void SelectViews () {
		
		ViewsDialogPanelScript.SetVisible (true);
	}
	
	public void OpenMainMenu () {
		
		MainMenuDialogPanelScript.SetVisible (true);
		
		Manager.InterruptSimulation (true);
	}

	public void OpenOptionsMenu () {
		
		OptionsDialogPanelScript.SetVisible (true);
	}

	public void UpdateMapView () {

		MapScript.SetVisible (!MapScript.IsVisible());

		UpdateMapViewButtonText ();
	}

	public void UpdateMapViewButtonText () {
		
		if (MapImage.enabled) {
			MapViewButtonText.text = "View World";
		} else {
			MapViewButtonText.text = "View Map";
		}
	}
	
	public void SetRainfallOverlay () {

		_regenTextures |= _planetOverlay != PlanetOverlay.Rainfall;

		_planetOverlay = PlanetOverlay.Rainfall;
	}
	
	public void SetTemperatureOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.Temperature;
		
		_planetOverlay = PlanetOverlay.Temperature;
	}
	
	public void SetPopulationOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.Population;
		
		_planetOverlay = PlanetOverlay.Population;
	}
	
	public void SetCulturalSkillOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.CulturalSkill;
		
		_planetOverlay = PlanetOverlay.CulturalSkill;

		SelectionPanelScript.Title.text = "Displayed Cultural Skill:";

		foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList) {

			string skillName = skillInfo.Name;
			string skillId = skillInfo.Id;
			
			SelectionPanelScript.AddOption (skillName, (state) => {
				if (state) {
					_planetOverlaySubtype = skillId;
				} else if (_planetOverlaySubtype == skillId) {
					_planetOverlaySubtype = "None";
				}

				_regenTextures = true;
			});

			if (_planetOverlaySubtype == skillId) {
				SelectionPanelScript.SetStateOption (skillName, true);
			}
		}

		SelectionPanelScript.SetVisible (true);
	}
	
	public void SetCulturalKnowledgeOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.CulturalKnowledge;
		
		_planetOverlay = PlanetOverlay.CulturalKnowledge;
		
		SelectionPanelScript.Title.text = "Displayed Cultural Knowledge:";
		
		foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList) {
			
			string knowledgeName = knowledgeInfo.Name;
			string knowledgeId = knowledgeInfo.Id;
			
			SelectionPanelScript.AddOption (knowledgeName, (state) => {
				if (state) {
					_planetOverlaySubtype = knowledgeId;
				} else if (_planetOverlaySubtype == knowledgeId) {
					_planetOverlaySubtype = "None";
				}
				
				_regenTextures = true;
			});
			
			if (_planetOverlaySubtype == knowledgeId) {
				SelectionPanelScript.SetStateOption (knowledgeName, true);
			}
		}
		
		SelectionPanelScript.SetVisible (true);
	}

	public void UpdateSelectionMenu () {

		if (!SelectionPanelScript.IsVisible ())
			return;

		if (_planetOverlay == PlanetOverlay.CulturalSkill) {
			
			foreach (CulturalSkillInfo skillInfo in Manager.CurrentWorld.CulturalSkillInfoList) {
				
				string skillName = skillInfo.Name;
				string skillId = skillInfo.Id;
				
				SelectionPanelScript.AddOption (skillName, (state) => {
					if (state) {
						_planetOverlaySubtype = skillId;
					} else if (_planetOverlaySubtype == skillId) {
						_planetOverlaySubtype = "None";
					}
					
					_regenTextures = true;
				});
				
				if (_planetOverlaySubtype == skillId) {
					SelectionPanelScript.SetStateOption (skillName, true);
				}
			}
		}
		
		if (_planetOverlay == PlanetOverlay.CulturalKnowledge) {
			
			foreach (CulturalKnowledgeInfo knowledgeInfo in Manager.CurrentWorld.CulturalKnowledgeInfoList) {
				
				string knowledgeName = knowledgeInfo.Name;
				string knowledgeId = knowledgeInfo.Id;
				
				SelectionPanelScript.AddOption (knowledgeName, (state) => {
					if (state) {
						_planetOverlaySubtype = knowledgeId;
					} else if (_planetOverlaySubtype == knowledgeId) {
						_planetOverlaySubtype = "None";
					}
					
					_regenTextures = true;
				});
				
				if (_planetOverlaySubtype == knowledgeId) {
					SelectionPanelScript.SetStateOption (knowledgeName, true);
				}
			}
		}
	}
	
	public void UnsetOverlay () {
		
		_regenTextures |= _planetOverlay != PlanetOverlay.None;
		
		_planetOverlay = PlanetOverlay.None;
	}
	
	public void SetBiomeView () {
		
		_regenTextures |= _planetView != PlanetView.Biomes;
		
		_planetView = PlanetView.Biomes;
		
		ViewsDialogPanelScript.SetVisible (false);
	}
	
	public void SetElevationView () {
		
		_regenTextures |= _planetView != PlanetView.Elevation;
		
		_planetView = PlanetView.Elevation;
		
		ViewsDialogPanelScript.SetVisible (false);
	}
	
	public void SetCoastlineView () {
		
		_regenTextures |= _planetView != PlanetView.Coastlines;
		
		_planetView = PlanetView.Coastlines;
		
		ViewsDialogPanelScript.SetVisible (false);
	}

	public void UpdateInfoPanel () {
		
		World world = Manager.CurrentWorld;
		
		InfoPanelText.text = "Year: " + world.CurrentDate;

		Vector2 point;
		
		if (GetMapCoordinatesFromCursor (out point)) {
			AddCellDataToInfoPanel (point);
		}
		
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nNumber of Migration Events: " + MigrateGroupEvent.EventCount;
		InfoPanelText.text += "\nMean Migration Travel Time: " + MigrateGroupEvent.MeanTravelTime.ToString("0.0");
		
		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nMUPS: " + _lastMapUpdateCount;
	}
	
	public void AddCellDataToInfoPanel (int longitude, int latitude) {
		
		World world = Manager.CurrentWorld;
		
		if ((longitude < 0) || (longitude >= world.Width))
			return;
		
		if ((latitude < 0) || (latitude >= world.Height))
			return;

		InfoPanelText.text += "\n";
		
		TerrainCell cell = world.TerrainCells[longitude][latitude];

		world.SetObservedCell (cell);
		
		InfoPanelText.text += string.Format("\nPosition: Longitude {0}, Latitude {1}", longitude, latitude);
		InfoPanelText.text += "\nAltitude: " + cell.Altitude + " meters";
		InfoPanelText.text += "\nRainfall: " + cell.Rainfall + " mm / year";
		InfoPanelText.text += "\nTemperature: " + cell.Temperature + " C";
		InfoPanelText.text += "\n";

		for (int i = 0; i < cell.PresentBiomeNames.Count; i++)
		{
			float percentage = cell.BiomePresences[i];
			
			InfoPanelText.text += "\nBiome: " + cell.PresentBiomeNames[i];
			InfoPanelText.text += " (" + percentage.ToString ("P") + ")";
		}

		InfoPanelText.text += "\n";
		InfoPanelText.text += "\nSurvivability: " + cell.Survivability.ToString("P");
		InfoPanelText.text += "\nForaging Capacity: " + cell.ForagingCapacity.ToString("P");
		InfoPanelText.text += "\nAccessibility: " + cell.Accessibility.ToString("P");

		int population = 0;
		int optimalPopulation = 0;
		int lastUpdateDate = 0;
		int nextUpdateDate = 0;

		if (cell.Group != null) {
		
			float modifiedSurvivability = 0;
			float modifiedForagingCapacity = 0;

			population = cell.Group.Population;
			optimalPopulation = cell.Group.OptimalPopulation;
			lastUpdateDate = cell.Group.LastUpdateDate;
			nextUpdateDate = cell.Group.NextUpdateDate;
		
			cell.Group.CalculateAdaptionToCell (cell, out modifiedForagingCapacity, out modifiedSurvivability);

			if (population > 0) {
				
				InfoPanelText.text += "\n";
				InfoPanelText.text += "\nPopulation: " + population;
				InfoPanelText.text += "\nOptimal Population: " + optimalPopulation;
				
				InfoPanelText.text += "\n";
				InfoPanelText.text += "\nModified Survivability: " + modifiedSurvivability.ToString("P");
				InfoPanelText.text += "\nModified Foraging Capacity: " + modifiedForagingCapacity.ToString("P");
				
				InfoPanelText.text += "\n";
				InfoPanelText.text += "\nLast Update Date: " + lastUpdateDate;
				InfoPanelText.text += "\nNext Update Date: " + nextUpdateDate;
				InfoPanelText.text += "\nTime between updates: " + (nextUpdateDate - lastUpdateDate);

				bool firstSkill = true;

				foreach (CulturalSkill skill in cell.Group.Culture.Skills) {

					float skillValue = skill.Value;

					if (skillValue >= 0.001) {

						if (firstSkill) {
							InfoPanelText.text += "\n";
							InfoPanelText.text += "\nCultural Skills";

							firstSkill = false;
						}

						InfoPanelText.text += "\n\t" + skill.Id + " - Value: " + skill.Value.ToString("0.000");
					}
				}

				bool firstKnowledge = true;
				
				foreach (CulturalKnowledge knowledge in cell.Group.Culture.Knowledges) {
					
					float knowledgeValue = knowledge.Value;
					
					if (knowledgeValue >= 0.001) {
						
						if (firstKnowledge) {
							InfoPanelText.text += "\n";
							InfoPanelText.text += "\nCultural Knowledges";
							
							firstKnowledge = false;
						}

						InfoPanelText.text += "\n\t" + knowledge.Id + " - Value: " + knowledge.Value.ToString("0.000");
					}
				}
			}
		}
	}
	
	public void AddCellDataToInfoPanel (Vector2 mapPosition) {

		int longitude = (int)mapPosition.x;
		int latitude = (int)mapPosition.y;

		AddCellDataToInfoPanel (longitude, latitude);
	}
	
	public bool GetMapCoordinatesFromCursor (out Vector2 point) {
		
		Rect mapImageRect = MapImage.rectTransform.rect;

		Vector3 mapMousePos = MapImage.rectTransform.InverseTransformPoint (Input.mousePosition);
		
		Vector2 mousePos = new Vector2 (mapMousePos.x, mapMousePos.y);

		if (mapImageRect.Contains (mousePos)) {

			Vector2 relPos = mousePos - mapImageRect.min;

			Vector2 uvPos = new Vector2 (relPos.x / mapImageRect.size.x, relPos.y / mapImageRect.size.y);

			uvPos += MapImage.uvRect.min;

			float worldLong = Mathf.Repeat (uvPos.x * Manager.CurrentWorld.Width, Manager.CurrentWorld.Width);
			float worldLat = uvPos.y * Manager.CurrentWorld.Height;

			point = new Vector2 (Mathf.Floor(worldLong), Mathf.Floor(worldLat));

			return true;
		}

		point = -Vector2.one;

		return false;
	}
	
	public void DragMap (BaseEventData data) {
		
		Rect mapImageRect = MapImage.rectTransform.rect;
		
		PointerEventData pointerData = data as PointerEventData;
		
		if (pointerData.button != PointerEventData.InputButton.Right)
			return;

		Vector2 delta = pointerData.position - _beginDragPosition;

		float uvDelta = delta.x / mapImageRect.width;

		Rect newUvRect = _beginDragMapUvRect;
		newUvRect.x -= uvDelta;

		MapImage.uvRect = newUvRect;
	}
	
	public void BeginDragMap (BaseEventData data) {
		
		PointerEventData pointerData = data as PointerEventData;

		if (pointerData.button != PointerEventData.InputButton.Right)
			return;

		_beginDragPosition = pointerData.position;
		_beginDragMapUvRect = MapImage.uvRect;
	}
	
	public void EndDragMap (BaseEventData data) {
	}
	
	public void SelectCellOnMap (BaseEventData data) {
		
		PointerEventData pointerData = data as PointerEventData;
		
		if (pointerData.button != PointerEventData.InputButton.Left)
			return;

		if (_mapLeftClickOperation != null) {
		
			_mapLeftClickOperation (pointerData.position);
		}
	}
}
