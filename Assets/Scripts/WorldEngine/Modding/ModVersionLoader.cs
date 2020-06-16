using System;
using System.IO;
using UnityEngine;

/// <summary>
/// class used to identify a mod's version
/// </summary>
[Serializable]
public class ModVersionReader
{
    public const string LoaderVersion033 = "0.3.3";
    public const string LoaderVersion034 = "0.3.4";

    private const string _versionFileName = "version.json";

#pragma warning disable 0649 // Disable warning for unitialized properties...

    public string version;
    public string loader_version;

#pragma warning restore 0649

    public static string GetLoaderVersion(string modPath)
    {
        string filename = Path.Combine(modPath, _versionFileName);

        if (!File.Exists(filename))
            return LoaderVersion033;

        string jsonStr = File.ReadAllText(filename);

        // Load json object from file into intermediary object
        ModVersionReader reader = JsonUtility.FromJson<ModVersionReader>(jsonStr);

        if (string.IsNullOrWhiteSpace(reader.loader_version))
        {
            throw new Exception("Mod's loader version can't be null or empty...");
        }

        return reader.loader_version.Trim();
    }
}
