using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class LocalizationManagerScript : MonoBehaviour
{
    public static LocalizationManagerScript Instance { get; set; }

    private readonly List<Dictionary<string, string>> localizations = new List<Dictionary<string, string>>();

    private Dictionary<string, string> currentLocalization;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        foreach (string file in Directory.EnumerateFiles(@"Languages", "*.json"))
        {
            string json = File.ReadAllText(file);
            localizations.Add(JsonConvert.DeserializeObject<Dictionary<string, string>>(json));
        }

        SetCurrentLocalization("English");
    }

    public string GetText(string key)
    {
        return currentLocalization[key];
    }

    public void SetCurrentLocalization(string language)
    {
        foreach (Dictionary<string, string> localization in localizations)
        {
            if (localization["LANGUAGE"].Equals(language))
            {
                currentLocalization = localization;
            }
        }
    }
}
