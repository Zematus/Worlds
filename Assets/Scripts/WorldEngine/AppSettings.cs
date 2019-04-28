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
    public float TemperatureOffset = 0;
    public float RainfallOffset = 0;
    public bool Fullscreen = true;
    public bool UIScaling = true;
    public bool DebugMode = true;

    public AppSettings()
    {
    }

    public void Put()
    {
        AltitudeScale = Manager.AltitudeScale;
        SeaLevelOffset = Manager.SeaLevelOffset;
        TemperatureOffset = Manager.TemperatureOffset;
        RainfallOffset = Manager.RainfallOffset;
        Fullscreen = Manager.FullScreenEnabled;
        UIScaling = Manager.UIScalingEnabled;
        DebugMode = Manager.DebugModeEnabled;
    }

    public void Take()
    {
        Manager.AltitudeScale = AltitudeScale;
        Manager.SeaLevelOffset = SeaLevelOffset;
        Manager.TemperatureOffset = TemperatureOffset;
        Manager.RainfallOffset = RainfallOffset;
        Manager.FullScreenEnabled = Fullscreen;
        Manager.UIScalingEnabled = UIScaling;
        Manager.DebugModeEnabled = DebugMode;
    }
}
