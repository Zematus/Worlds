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
    public bool DebugMode = true;
    public bool FullGameplayInfo = true;

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
        DebugMode = Manager.DebugModeEnabled;
        FullGameplayInfo = Manager.ShowFullGameplayInfo;
    }

    public void Take()
    {
        Manager.TemperatureOffset = TemperatureOffset;
        Manager.RainfallOffset = RainfallOffset;
        Manager.SeaLevelOffset = SeaLevelOffset;
        Manager.FullScreenEnabled = Fullscreen;
        Manager.DebugModeEnabled = DebugMode;
        Manager.ShowFullGameplayInfo = FullGameplayInfo;
    }
}
