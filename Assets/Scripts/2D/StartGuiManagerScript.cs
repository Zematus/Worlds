using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class StartGuiManagerScript : MonoBehaviour
{
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

    void OnEnable()
    {
        Manager.InitializeDebugLog();

        Application.logMessageReceivedThreaded += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;

        if (_changingScene)
            return;

        Manager.CloseDebugLog();
    }

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

    // Use this for initialization
    void Start()
    {
        Manager.InitializeScreen();

        Manager.UpdateMainThreadReference();
        
        ProgressDialogPanelScript.SetVisible(false);
        MessageDialogPanelScript.SetVisible(false);
        ExceptionDialogPanelScript.SetVisible(false);
        MainMenuDialogPanelScript.SetVisible(true);

        LoadButton.interactable = HasSaveFilesToLoad();
    }

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

    void OnDestroy()
    {
        Manager.SaveAppSettings(@"Worlds.settings");
    }

    // Update is called once per frame
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
            if (_postProgressOp != null)
                _postProgressOp();

            _preparingWorld = false;

            _changingScene = true;

            SceneManager.LoadScene("WorldView");
        }
    }

    private void ToogleFullscreen()
    {
        ToogleFullscreen(!Manager.FullScreenEnabled);
    }

    private void ReadKeyboardInput()
    {
        if (_preparingWorld)
            return; // Don't read keyboard while the world is being generated/loaded...

        Manager.HandleKeyUp(KeyCode.L, true, false, LoadWorld);
        Manager.HandleKeyUp(KeyCode.G, true, false, SetGenerationSeed);
        Manager.HandleKeyUp(KeyCode.F, true, false, ToogleFullscreen);
    }

    public void CloseExceptionMessageAction()
    {
        Exit();
    }

    private void PostProgressOp_GenerateWorld()
    {
        Debug.Log("Finished generating world with seed: " + Manager.CurrentWorld.Seed);

        Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;

        _postProgressOp -= PostProgressOp_GenerateWorld;
    }

    public void PostProgressOp_LoadAction()
    {
        Debug.Log(string.Format(
            "Finished loading world. Seed: {0}, Altitude Scale: {1}, Sea Level Offset: {2}, Avg. Temperature: {3}, Avg. Rainfall: {4}, Current Date: {5}",
            Manager.CurrentWorld.Seed,
            Manager.CurrentWorld.AltitudeScale,
            Manager.CurrentWorld.SeaLevelOffset,
            Manager.CurrentWorld.TemperatureOffset,
            Manager.CurrentWorld.RainfallOffset,
            Manager.GetDateString(Manager.CurrentWorld.CurrentDate)));

        _postProgressOp -= PostProgressOp_LoadAction;
    }

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

    private bool HasSaveFilesToLoad()
    {
        string dirPath = Manager.SavePath;

        string[] files = Directory.GetFiles(dirPath, "*.PLNT");

        return files.Length > 0;
    }

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

    public void CancelLoadHeightmapAction()
    {
        SetSeedDialogPanelScript.SetVisible(true);
    }

    public void CancelLoadSaveAction()
    {
        MainMenuDialogPanelScript.SetVisible(true);
    }

    public void SetGenerationSeed()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        int seed = Random.Range(0, int.MaxValue);

        SetSeedDialogPanelScript.SetSeedString(seed.ToString());

        SetSeedDialogPanelScript.SetVisible(true);
    }

    public void OpenSettingsDialog()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        SettingsDialogPanelScript.FullscreenToggle.isOn = Manager.FullScreenEnabled;

        SettingsDialogPanelScript.SetVisible(true);
    }

    public void OpenCreditsDialog()
    {
        MainMenuDialogPanelScript.SetVisible(false);

        CreditsDialogPanelScript.SetVisible(true);
    }

    public void ToogleFullscreen(bool state)
    {
        Manager.SetFullscreen(state);
    }

    public void CloseSettingsDialog()
    {
        SettingsDialogPanelScript.SetVisible(false);

        MainMenuDialogPanelScript.SetVisible(true);
    }

    public void CloseCreditsDialog()
    {
        CreditsDialogPanelScript.SetVisible(false);

        MainMenuDialogPanelScript.SetVisible(true);
    }

    public void CloseSeedErrorMessageAction()
    {
        MessageDialogPanelScript.SetVisible(false);

        SetGenerationSeed();
    }

    public void GenerateWorldWithCustomSeed()
    {
        int seed = 0;
        string seedStr = SetSeedDialogPanelScript.GetSeedString();

        if (!int.TryParse(seedStr, out seed))
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

    public void ProgressUpdate(float value, string message = null, bool reset = false)
    {
        if (reset || (value >= _progressValue))
        {
            if (message != null)
                _progressMessage = message;

            _progressValue = value;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
