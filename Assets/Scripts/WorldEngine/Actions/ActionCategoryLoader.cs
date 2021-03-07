using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// Class used to load action category mod entries from mod JSON files.
/// Class properties should match the root structure of the JSON file.
/// </summary>
[Serializable]
public class ActionCategoryLoader
{
#pragma warning disable 0649 // Disable warning for unitialized properties...

    /// <summary>
    /// list of loaded category entries.
    /// </summary>
    public LoadedActionCategory[] actionCategories;

    /// <summary>
    /// Object defining a category entry. Structure must match that of
    /// an category entry in the mod file
    /// </summary>
    [Serializable]
    public class LoadedActionCategory
    {
        public string id;
        public string name;
        public string image;
    }

#pragma warning restore 0649

    /// <summary>
    /// Produce a set of ActionCategory objects based on the contents an event mod file
    /// </summary>
    /// <param name="filename">Name of the JSON file with action categories to load</param>
    /// <returns>A set of ActionCategory objects, each one relating to a single category entry
    /// on the file</returns>
    public static IEnumerable<ActionCategory> Load(string filename)
    {
        string directoryPath = Path.GetDirectoryName(filename);

        string jsonStr = File.ReadAllText(filename);

        // Load json object from file into intermediary object
        ActionCategoryLoader loader = JsonUtility.FromJson<ActionCategoryLoader>(jsonStr);

        for (int i = 0; i < loader.actionCategories.Length; i++)
        {
            ActionCategory category;
            try
            {
                category = CreateActionCategory(loader.actionCategories[i], directoryPath);
            }
            catch (Exception e)
            {
                // If there's a failure while loading an action entry, report
                // the file from which the category came from and its index within
                // the file...
                throw new Exception(
                    "Failure loading action category #" + i + " in " + filename + ": "
                    + e.Message, e);
            }

            yield return category;
        }
    }

    /// <summary>
    /// Produces an action category object from a single category entry
    /// </summary>
    /// <param name="e">The category entry</param>
    /// <param name="modPath">The path to the location of the category mod file</param>
    /// <returns>The resulting action category</returns>
    private static ActionCategory CreateActionCategory(LoadedActionCategory e, string modPath)
    {
        if (string.IsNullOrEmpty(e.id))
        {
            throw new ArgumentException("action category 'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.name))
        {
            throw new ArgumentException("action category 'name' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.image))
        {
            throw new ArgumentException("action category 'image' can't be null or empty");
        }

        Texture2D texture = Manager.LoadTexture(Path.Combine(modPath, e.image));

        if (texture == null)
        {
            throw new ArgumentException("unable to load action category image: " + e.image);
        }

        // We need to perform this validation within a manager task
        if (Manager.EnqueueTask(() =>
            (texture.width != ActionCategory.ImageWidth) ||
            (texture.height != ActionCategory.ImageHeight)))
        {
            throw new ArgumentException("action category image size must be 60x50 pixels");
        }

        Sprite image = Manager.CreateSprite(texture);

        ActionCategory category = new ActionCategory
        {
            Id = e.id,
            Name = e.name,
            Image = image
        };

        return category;
    }
}
