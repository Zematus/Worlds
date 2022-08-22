using System.Collections.Generic;

public class AppSettings
{
    public int StartSpeedIndex = 7;
    public float AltitudeScale = 0;
    public float SeaLevelOffset = 0;
    public float RiverStrength = 0;
    public float TemperatureOffset = 0;
    public float RainfallOffset = 0;
    public bool Fullscreen = true;
    public bool UIScaling = true;
    public string DeveloperMode = "None";
    public bool AnimationShaders = true;
    public string AutoSaveMode = "Deactivate";
    public float RealWorldAutoSaveInterval = 600f;
    public long AutoSaveInterval = 365000000;
    public float KeyboardXAxisSensitivity = 50.0f;
    public float KeyboardYAxisSensitivity = 50.0f;
    public bool KeyboardInvertXAxis = false;
    public bool KeyboardInvertYAxis = false;

    public List<string> ActiveModPaths = new List<string>();

    public List<LayerSettings> LayerSettings = new List<LayerSettings>();

    public AppSettings()
    {
    }

    public void Put()
    {
        StartSpeedIndex = Manager.StartSpeedIndex;
        AltitudeScale = Manager.AltitudeScale;
        SeaLevelOffset = Manager.SeaLevelOffset;
        RiverStrength = Manager.RiverStrength;
        TemperatureOffset = Manager.TemperatureOffset;
        RainfallOffset = Manager.RainfallOffset;
        Fullscreen = Manager.FullScreenEnabled;
        UIScaling = Manager.UIScalingEnabled;
        AnimationShaders = Manager.AnimationShadersEnabled;
        KeyboardXAxisSensitivity = Manager.KeyboardXAxisSensitivity;
        KeyboardYAxisSensitivity = Manager.KeyboardYAxisSensitivity;
        KeyboardInvertXAxis = Manager.KeyboardInvertXAxis;
        KeyboardInvertYAxis = Manager.KeyboardInvertYAxis;

        switch (Manager.CurrentDevMode)
        {
            case DevMode.None:
                DeveloperMode = "None";
                break;
            case DevMode.Basic:
                DeveloperMode = "Basic";
                break;
            case DevMode.Advanced:
                DeveloperMode = "Advanced";
                break;
            default:
                throw new System.Exception("Unhandled Dev Mode: " + Manager.CurrentDevMode);
        }

        RealWorldAutoSaveInterval = Manager.RealWorldAutoSaveInterval;
        AutoSaveInterval = Manager.AutoSaveInterval;

        switch (Manager.AutoSaveMode)
        {
            case global::AutoSaveMode.Deactivate:
                AutoSaveMode = "Deactivate";
                break;
            case global::AutoSaveMode.OnGameTime:
                AutoSaveMode = "OnGameTime";
                break;
            case global::AutoSaveMode.OnRealWorldAndGameTime:
                AutoSaveMode = "OnRealWorldAndGameTime";
                break;
            case global::AutoSaveMode.OnRealWorldOrGameTime:
                AutoSaveMode = "OnRealWorldOrGameTime";
                break;
            case global::AutoSaveMode.OnRealWorldTime:
                AutoSaveMode = "OnRealWorldTime";
                break;
            default:
                throw new System.Exception("Unhandled Dev Mode: " + Manager.CurrentDevMode);
        }

        ActiveModPaths.Clear();
        ActiveModPaths.AddRange(Manager.ActiveModPaths);

        LayerSettings.Clear();
        LayerSettings.AddRange(Manager.LayerSettings.Values);
    }

    public void Take()
    {
        Manager.StartSpeedIndex = StartSpeedIndex;
        Manager.AltitudeScale = AltitudeScale;
        Manager.SeaLevelOffset = SeaLevelOffset;
        Manager.RiverStrength = RiverStrength;
        Manager.TemperatureOffset = TemperatureOffset;
        Manager.RainfallOffset = RainfallOffset;
        Manager.FullScreenEnabled = Fullscreen;
        Manager.UIScalingEnabled = UIScaling;
        Manager.AnimationShadersEnabled = AnimationShaders;
        Manager.KeyboardXAxisSensitivity = KeyboardXAxisSensitivity;
        Manager.KeyboardYAxisSensitivity = KeyboardYAxisSensitivity;
        Manager.KeyboardInvertXAxis = KeyboardInvertXAxis;
        Manager.KeyboardInvertYAxis = KeyboardInvertYAxis;

        switch (DeveloperMode)
        {
            case "None":
                Manager.CurrentDevMode = DevMode.None;
                break;
            case "Basic":
                Manager.CurrentDevMode = DevMode.Basic;
                break;
            case "Advanced":
                Manager.CurrentDevMode = DevMode.Advanced;
                break;
            default:
                throw new System.Exception("Unhandled Developer Mode Setting: " + DeveloperMode);
        }

        Manager.RealWorldAutoSaveInterval = RealWorldAutoSaveInterval;
        Manager.AutoSaveInterval = AutoSaveInterval;

        switch (AutoSaveMode)
        {
            case "Deactivate":
                Manager.AutoSaveMode = global::AutoSaveMode.Deactivate;
                break;
            case "OnGameTime":
                Manager.AutoSaveMode = global::AutoSaveMode.OnGameTime;
                break;
            case "OnRealWorldAndGameTime":
                Manager.AutoSaveMode = global::AutoSaveMode.OnRealWorldAndGameTime;
                break;
            case "OnRealWorldOrGameTime":
                Manager.AutoSaveMode = global::AutoSaveMode.OnRealWorldOrGameTime;
                break;
            case "OnRealWorldTime":
                Manager.AutoSaveMode = global::AutoSaveMode.OnRealWorldTime;
                break;
            default:
                throw new System.Exception("Unhandled Dev Mode: " + Manager.CurrentDevMode);
        }

        Manager.SetActiveModPaths(ActiveModPaths);
        Manager.SetLayerSettings(LayerSettings);
    }
}
