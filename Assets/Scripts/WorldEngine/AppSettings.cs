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

        Manager.SetActiveModPaths(ActiveModPaths);
        Manager.SetLayerSettings(LayerSettings);
    }
}
