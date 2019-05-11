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

    public List<string> ActiveMods = new List<string>();

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

        ActiveMods.Clear();
        ActiveMods.AddRange(Manager.ActiveMods);
    }

    public void Take()
    {
        Manager.AltitudeScale = AltitudeScale;
        Manager.SeaLevelOffset = SeaLevelOffset;
        Manager.TemperatureOffset = TemperatureOffset;
        Manager.RainfallOffset = RainfallOffset;
        Manager.FullScreenEnabled = Fullscreen;
        Manager.DebugModeEnabled = DebugMode;

        Manager.ActiveMods.Clear();
        Manager.ActiveMods.AddRange(ActiveMods);
    }
}
