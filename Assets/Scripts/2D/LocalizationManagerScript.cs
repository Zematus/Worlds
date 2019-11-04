using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationManagerScript : MonoBehaviour
{
    public static LocalizationManagerScript Instance { get; set; }

    private static readonly List<Dictionary<string, string>> _localizations = new List<Dictionary<string, string>>();

    private static List<LocalizationUITextScript> _localizationUITextScripts = new List<LocalizationUITextScript>();

    private static Dictionary<string, string> _currentLocalization;

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

    public static void SetCurrentLocalization(string language)
    {
        foreach (Dictionary<string, string> localization in _localizations)
        {
            if (localization["LANGUAGE"].Equals(language))
            {
                _currentLocalization = localization;

                foreach (LocalizationUITextScript localizationUITextScript in _localizationUITextScripts)
                {
                    localizationUITextScript.UpdateText();
                }

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

    public static void AddInstance(LocalizationUITextScript localizationUITextScript)
    {
        _localizationUITextScripts.Add(localizationUITextScript);
    }

    public void UpdateCurrentLocalization()
    {
        GameObject LanguageDropdown = GameObject.Find("LanguageDropdown");

        Dropdown Dropdown = LanguageDropdown.GetComponent<Dropdown>();

        SetCurrentLocalization(Dropdown.options[Dropdown.value].text);
    }
}
