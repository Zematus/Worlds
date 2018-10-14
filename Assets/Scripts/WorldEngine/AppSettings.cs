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
    public float TemperatureOffset = 0;
    public float RainfallOffset = 0;
    public float SeaLevelOffset = 0;
    public bool Fullscreen = true;
    public bool DebugMode = true;
    public bool FullGameplayInfo = true;

    public AppSettings()
    {
    }

    public void Put()
    {
        TemperatureOffset = Manager.TemperatureOffset;
        RainfallOffset = Manager.RainfallOffset;
        SeaLevelOffset = Manager.SeaLevelOffset;
        Fullscreen = Manager.IsFullscreen;
        DebugMode = Manager.IsDebugModeEnabled;
        FullGameplayInfo = Manager.ShowFullGameplayInfo;
    }

    public void Take()
    {
        Manager.TemperatureOffset = TemperatureOffset;
        Manager.RainfallOffset = RainfallOffset;
        Manager.SeaLevelOffset = SeaLevelOffset;
        Manager.IsFullscreen = Fullscreen;
        Manager.IsDebugModeEnabled = DebugMode;
        Manager.ShowFullGameplayInfo = FullGameplayInfo;
    }
}
