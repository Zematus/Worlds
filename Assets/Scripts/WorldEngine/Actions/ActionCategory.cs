using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Player accessible scriptable action category
/// </summary>
public class ActionCategory
{
    public const int ImageWidth = 60;
    public const int ImageHeight = 50;

    public static Dictionary<string, ActionCategory> Categories;

    // This list will ensure keys appear in the order they were loaded
    public static List<string> CategoryKeys;

    /// <summary>
    /// Unique identifier for this category
    /// </summary>
    public string Id;

    /// <summary>
    /// Name to use in the UI for this category
    /// </summary>
    public string Name;

    /// <summary>
    /// Image file to use for this category
    /// </summary>
    public Sprite Image;

    public static void ResetActionCategories()
    {
        Categories = new Dictionary<string, ActionCategory>();
        CategoryKeys = new List<string>();
    }

    public static void LoadActionCategoryFile(string filename)
    {
        foreach (ActionCategory category in ActionCategoryLoader.Load(filename))
        {
            if (Categories.ContainsKey(category.Id))
            {
                Categories[category.Id] = category;
            }
            else
            {
                Categories.Add(category.Id, category);
                CategoryKeys.Add(category.Id);
            }

            Action.CategoryIds.Add(category.Id);
        }
    }

    public static ActionCategory GetCategory(string id)
    {
        return !Categories.TryGetValue(id, out ActionCategory c) ? null : c;
    }
}
