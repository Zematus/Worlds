using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using UnityEngine.Profiling;

public class AppSettings
{
    public float AltitudeScale = 0;
    public float SeaLevelOffset = 0;
    public float RiverStrength = 0;
    public float TemperatureOffset = 0;
    public float RainfallOffset = 0;
    public bool Fullscreen = true;
    public bool UIScaling = true;
    public bool DebugMode = true;
    public bool AnimationShaders = true;

    public List<string> ActiveModPaths = new List<string>();

    public List<LayerSettings> LayerSettings = new List<LayerSettings>();

    public AppSettings()
    {
    }

    public void Put()
    {
        AltitudeScale = Manager.AltitudeScale;
        SeaLevelOffset = Manager.SeaLevelOffset;
        RiverStrength = Manager.RiverStrength;
        TemperatureOffset = Manager.TemperatureOffset;
        RainfallOffset = Manager.RainfallOffset;
        Fullscreen = Manager.FullScreenEnabled;
        UIScaling = Manager.UIScalingEnabled;
        DebugMode = Manager.DebugModeEnabled;
        AnimationShaders = Manager.AnimationShadersEnabled;

        ActiveModPaths.Clear();
        ActiveModPaths.AddRange(Manager.ActiveModPaths);

        LayerSettings.Clear();
        LayerSettings.AddRange(Manager.LayerSettings.Values);
    }

    public void Take()
    {
        Manager.AltitudeScale = AltitudeScale;
        Manager.SeaLevelOffset = SeaLevelOffset;
        Manager.RiverStrength = RiverStrength;
        Manager.TemperatureOffset = TemperatureOffset;
        Manager.RainfallOffset = RainfallOffset;
        Manager.FullScreenEnabled = Fullscreen;
        Manager.UIScalingEnabled = UIScaling;
        Manager.DebugModeEnabled = DebugMode;
        Manager.AnimationShadersEnabled = AnimationShaders;

        Manager.SetActiveModPaths(ActiveModPaths);
        Manager.SetLayerSettings(LayerSettings);
    }
}
