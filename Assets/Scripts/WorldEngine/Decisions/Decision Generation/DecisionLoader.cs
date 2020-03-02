using System;

/// <summary>
/// Class used to load decision mod entries from mod JSON files.
/// Class properties should match the root structure of the JSON file.
/// </summary>
[Serializable]
public class DecisionLoader
{
#pragma warning disable 0649 // Disable warning for unitialized properties...

    public LoadedDecision[] decisions;

    [Serializable]
    public class LoadedDecision
    {
        public string id;
        public string name;
        public string target;
        public Context.LoadedProperty[] properties;
        public string[] description;
        public string[] options;
    }

#pragma warning restore 0649

}
