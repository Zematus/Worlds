using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class LocalizationManagerScript : MonoBehaviour
{
    public static LocalizationManagerScript Instance { get; set; }

    private static readonly List<Dictionary<string, string>> _localizations = new List<Dictionary<string, string>>();

    private Dictionary<string, string> _currentLocalization;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        foreach (string file in Directory.EnumerateFiles(@"Languages", "*.json"))
        {
            string json = File.ReadAllText(file);
            _localizations.Add(JsonConvert.DeserializeObject<Dictionary<string, string>>(json));
        }

        SetCurrentLocalization("English");
    }

    public string GetText(string key)
    {
        return _currentLocalization[key];
    }

    public void SetCurrentLocalization(string language)
    {
        foreach (Dictionary<string, string> localization in _localizations)
        {
            if (localization["LANGUAGE"].Equals(language))
            {
                _currentLocalization = localization;
                return;
            }
        }

        Debug.LogError("The specified language does not exist: " + language);
    }

    public static void LoadLanguagesFile(string filename)
    {
        string json = File.ReadAllText(filename);
        Dictionary<string, string> language = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        foreach (Dictionary<string, string> localization in _localizations)
        {
            if (localization["LANGUAGE"].Equals(language["LANGUAGE"]))
            {
                language.ToList().ForEach(x => localization[x.Key] = x.Value);
                return;
            }
        }

        Debug.LogError("The specified language does not exist: " + language["LANGUAGE"]);
    }
}
