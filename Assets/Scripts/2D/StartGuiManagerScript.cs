using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class StartGuiManagerScript : MonoBehaviour
{
    public CanvasScaler CanvasScaler;

    public Button LoadButton;

    public LoadFileDialogPanelScript LoadFileDialogPanelScript;
    public DialogPanelScript MainMenuDialogPanelScript;
    public DialogPanelScript ExceptionDialogPanelScript;
    public SettingsDialogPanelScript SettingsDialogPanelScript;
    public ProgressDialogPanelScript ProgressDialogPanelScript;
    public DialogPanelScript MessageDialogPanelScript;
    public WorldCustomizationDialogPanelScript SetSeedDialogPanelScript;
    public ModalPanelScript CreditsDialogPanelScript;

    public Text VersionText;

    private bool _preparingWorld = false;

    private string _progressMessage = null;
    private float _progressValue = 0;

    private PostProgressOperation _postProgressOp = null;

    private bool _changingScene = false;

    private Texture2D _heightmap = null;

    private System.Exception _cachedException = null;

    /// <summary>Called when the StartGuiManager GameObject is enabled.</summary>
    void OnEnable()
    {
        Manager.InitializeDebugLog();

        Application.logMessageReceivedThreaded += HandleLog;
    }

    /// <summary>Called when the StartGuiManager GameObject is disabled.</summary>
    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;

        if (_changingScene)
            return;

        Manager.CloseDebugLog();
    }

    /// <summary>Handler used for logging, tracing and debugging.</summary>
    /// <param name="logString">The string to be logged.</param>
    /// <param name="stackTrace">The stack trace.</param>
    /// <param name="type">The type of log message.</param>
    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        Manager.HandleLog(logString, stackTrace, type);

        if (type == LogType.Exception)
        {
            Manager.EnableLogBackup();

            Manager.EnqueueTaskAndWait(() =>
            {
                ExceptionDialogPanelScript.SetDialogText(logString);
                ExceptionDialogPanelScript.SetVisible(true);

                return true;
            });
        }
    }

    /// <summary>Called on the frame when the StartGuiManager script is enabled.</summary>
    /// <remarks>Use this for initialization.</remarks>
    void Start()
    {
        Manager.InitializeScreen();

        SetUIScaling(Manager.UIScalingEnabled);

        Manager.UpdateMainThreadReference();
        
        ProgressDialogPanelScript.SetVisible(false);
        MessageDialogPanelScript.SetVisible(false);
        ExceptionDialogPanelScript.SetVisible(false);
        MainMenuDialogPanelScript.SetVisible(true);

        LoadButton.interactable = HasSaveFilesToLoad();
    }

    /// <summary>Called when the StartGuiManager script instance is being loaded.</summary>
    /// <remarks>Used to initialize settings before the game starts.</remarks>
    void Awake()
    {
        try
        {
            Manager.LoadAppSettings(@"Worlds.settings");
        }
        catch (System.Exception e)
        {
            // store the exception to report it on screen as soon as possible
            _cachedException = e;
        }

        VersionText.text = "v" + Application.version;
    }

    /// <summary>Occurs when exiting the game.</summary>
    void OnDestroy()
    {
        Manager.SaveAppSettings(@"Worlds.settings");
    }

    /// <summary>Checks world generation progress and switches scene on completion.</summary>
    /// <remarks>Update is called once per frame.</remarks>
    void Update()
    {
        if (_cachedException != null)
        {
            System.Exception ce = _cachedException;
            _cachedException = null;

            throw ce;
        }

        ReadKeyboardInput();

        Manager.ExecuteTasks(100);

        if (_preparingWorld)
        {
            if (_progressMessage != null) ProgressDialogPanelScript.SetDialogText(_progressMessage);

            ProgressDialogPanelScript.SetProgress(_progressValue);
        }

        if (!Manager.WorldIsReady)
        {
            return;
        }

        if (_preparingWorld)
        {
            _postProgressOp?.Invoke();

            _preparingWorld = false;

            _changingScene = true;

            SceneManager.LoadScene("WorldView");
        }
    }

    private void ToggleFullscreen()
    {
        SetFullscreen(!Manager.FullScreenEnabled);
    }

    /// <summary>Reads the keyboard input.</summary>
    private void ReadKeyboardInput()
    {
        if (_preparingWorld)
            return; // Don't read keyboard while the world is being generated/loaded...

        Manager.HandleKeyUp(KeyCode.L, true, false, LoadWorld);
        Manager.HandleKeyUp(KeyCode.G, true, false, SetGenerationSeed);
        //Manager.HandleKeyUp(KeyCode.F, true, false, ToggleFullscreen);
    }

    /// <summary>Called after closing the exception message dialog panel.</summary>
    public void CloseExceptionMessageAction()
    {
        Exit();
    }

    /// <summary>Called once the world is finished generating.</summary>
    private void PostProgressOp_GenerateWorld()
    {
        Debug.Log("Finished generating world with seed: " + Manager.CurrentWorld.Seed);

        Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;

        _postProgressOp -= PostProgressOp_GenerateWorld;
    }

    /// <summary>Called once the world is finished loading.</summary>
    public void PostProgressOp_LoadAction()
    {
        Debug.Log(string.Format(
            "Finished loading world. Seed: {0}, Altitude Scale: {1}, Sea Level Offset: {2}, River Strength: {3}, Avg. Temperature: {4}, Avg. Rainfall: {5}, Current Date: {6}",
            Manager.CurrentWorld.Seed,
            Manager.CurrentWorld.AltitudeScale,
            Manager.CurrentWorld.SeaLevelOffset,
            Manager.CurrentWorld.RiverStrength,
            Manager.CurrentWorld.TemperatureOffset,
            Manager.CurrentWorld.RainfallOffset,
            Manager.GetDateString(Manager.CurrentWorld.CurrentDate)));

        _postProgressOp -= PostProgressOp_LoadAction;
    }

    /// <summary>Starts to load the heightmap image.</summary>
    public void LoadHeightmapImage()
    {
        LoadFileDialogPanelScript.Initialize(
            "Select Heightmap Image to Load...",
            "Load",
            LoadHeightmapAction,
            CancelLoadHeightmapAction,
            Manager.HeightmapsPath,
            Manager.SupportedHeightmapFormats);

        LoadFileDialogPanelScript.SetVisible(true);
    }

    /// <summary>Determines whether there are save files to load.</summary>
    /// <returns>
    ///   <c>true</c> if there are save files to load; otherwise, <c>false</c>.
    /// </returns>
    private bool HasSaveFilesToLoad()
    {
        string dirPath = Manager.SavePath;

        string[] files = Directory.GetFiles(dirPath, "*.PLNT");

        return files.Length > 0;
    }

    /// <summary>Initializes world loading.</summary>
    public void LoadWorld()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        LoadFileDialogPanelScript.Initialize(
            "Select World to Load...", 
            "Load", 
            LoadSaveAction,
            CancelLoadSaveAction,
            Manager.SavePath, 
            new string[] { ".PLNT" });

        LoadFileDialogPanelScript.SetVisible(true);
    }

    /// <summary>Loads the heightmap.</summary>
    private void LoadHeightmapAction()
    {
        string path = LoadFileDialogPanelScript.GetPathToLoad();
        Texture2D texture = Manager.LoadTexture(path);

        if (texture == null)
        {
            SetSeedDialogPanelScript.SetImageTexture(Path.GetFileName(path), null, TextureValidationResult.Unknown);
        }
        else
        {
            TextureValidationResult result = Manager.ValidateTexture(texture);

            SetSeedDialogPanelScript.SetImageTexture(Path.GetFileName(path), texture, result);
        }

        _heightmap = texture;

        SetSeedDialogPanelScript.SetVisible(true);
    }

    /// <summary>Loads the world.</summary>
    public void LoadSaveAction()
    {
        ProgressDialogPanelScript.SetVisible(true);

        ProgressUpdate(0, "Loading World...", true);

        string path = LoadFileDialogPanelScript.GetPathToLoad();

        Manager.LoadWorldAsync(path, ProgressUpdate);

        Manager.WorldName = Manager.RemoveDateFromWorldName(Path.GetFileNameWithoutExtension(path));

        _postProgressOp += PostProgressOp_LoadAction;

        _preparingWorld = true;
    }

    /// <summary>Cancels the load heightmap action.</summary>
    public void CancelLoadHeightmapAction()
    {
        SetSeedDialogPanelScript.SetVisible(true);
    }

    /// <summary>Cancels the load save action.</summary>
    public void CancelLoadSaveAction()
    {
        MainMenuDialogPanelScript.SetVisible(true);
    }

    /// <summary>Sets the generation seed.</summary>
    public void SetGenerationSeed()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        int seed = Random.Range(0, int.MaxValue);

        SetSeedDialogPanelScript.SetSeedString(seed.ToString());

        SetSeedDialogPanelScript.SetVisible(true);
    }

    /// <summary>Opens the settings dialog.</summary>
    public void OpenSettingsDialog()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        SettingsDialogPanelScript.FullscreenToggle.isOn = Manager.FullScreenEnabled;
        SettingsDialogPanelScript.UIScalingToggle.isOn = Manager.UIScalingEnabled;
        SettingsDialogPanelScript.DebugModeToggle.isOn = Manager.DebugModeEnabled;
        SettingsDialogPanelScript.AnimationShadersToggle.isOn = Manager.AnimationShadersEnabled;

        SettingsDialogPanelScript.SetVisible(true);
    }

    /// <summary>Opens the credits dialog.</summary>
    public void OpenCreditsDialog()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        CreditsDialogPanelScript.SetVisible(true);
    }

    /// <summary>Sets the game's fullscreen option.</summary>
    /// <param name="state">If set to <c>true</c>, the game goes fullscreen.</param>
    public void SetFullscreen(bool state)
    {
        Manager.SetFullscreen(state);
    }

    /// <summary>Sets the game's UI scaling option.</summary>
    /// <param name="state">If set to <c>true</c>, the UI is scaling to screen size.</param>
    public void SetUIScaling(bool state)
    {
        Manager.SetUIScaling(state);

        if (state)
        {
            CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }
        else
        {
            CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }
    }

    /// <summary>Closes the settings dialog.</summary>
    public void CloseSettingsDialog()
    {
        SettingsDialogPanelScript.SetVisible(false);

        MainMenuDialogPanelScript.SetVisible(true);
    }

    /// <summary>Closes the credits dialog.</summary>
    public void CloseCreditsDialog()
    {
        CreditsDialogPanelScript.SetVisible(false);

        MainMenuDialogPanelScript.SetVisible(true);
    }

    /// <summary>Closes the seed error message action.</summary>
    public void CloseSeedErrorMessageAction()
    {
        MessageDialogPanelScript.SetVisible(false);

        SetGenerationSeed();
    }

    /// <summary>Generates the world with a custom seed.</summary>
    public void GenerateWorldWithCustomSeed()
    {
        string seedStr = SetSeedDialogPanelScript.GetSeedString();

        if (!int.TryParse(seedStr, out int seed))
        {
            MessageDialogPanelScript.SetVisible(true);
            return;
        }

        if (seed < 0)
        {
            MessageDialogPanelScript.SetVisible(true);
            return;
        }

        GenerateWorldInternal(seed);
    }

    /// <summary>Generates the world.</summary>
    /// <param name="seed">The seed.</param>
    private void GenerateWorldInternal(int seed)
    {
        ProgressDialogPanelScript.SetVisible(true);

        ProgressUpdate(0, "Generating World...", true);

        _preparingWorld = true;

        if (SetSeedDialogPanelScript.UseHeightmapToggle.isOn)
        {
            Manager.GenerateNewWorldAsync(seed, _heightmap, ProgressUpdate);
        }
        else
        {
            Manager.GenerateNewWorldAsync(seed, null, ProgressUpdate);
        }

        _postProgressOp += PostProgressOp_GenerateWorld;
    }

    /// <summary>Updates the progress bar for generating/loading the world.</summary>
    /// <param name="value">Current progess value.</param>
    /// <param name="message">Message to display on progress bar.</param>
    /// <param name="reset">If set to <c>true</c>, the progress value is reset.</param>
    public void ProgressUpdate(float value, string message = null, bool reset = false)
    {
        if (reset || (value >= _progressValue))
        {
            if (message != null)
                _progressMessage = message;

            _progressValue = value;
        }
    }

    /// <summary>Exits the game.</summary>
    public void Exit()
    {
        Application.Quit();
    }
}
