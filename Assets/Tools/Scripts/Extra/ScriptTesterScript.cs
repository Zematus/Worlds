using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScriptTesterScript : MonoBehaviour
{
    // This script is meant to be very simple and to be used to test / develop other scripts. 
    // Kind of like a unit test, but not really...

    // Start is called before the first frame update
    void Start()
    {
        Manager.UpdateMainThreadReference();

        Debug.Log("loading biome mod file...");

        Biome.LoadBiomesFile(@"Mods\Base\biomes.json");

        foreach (Biome biome in Biome.Biomes.Values)
        {
            Debug.Log("generated biome: " + biome.Name);
        }

        Debug.Log("loading region attribute mod file...");

        RegionAttribute.LoadRegionAttributesFile(@"Mods\Base\region_attributes.json");

        foreach (RegionAttribute regionAttribute in RegionAttribute.Attributes.Values)
        {
            Debug.Log("generated region attribute: " + regionAttribute.Name);
        }

        Debug.Log("loading element mod file...");

        Element.LoadElementsFile(@"Mods\Base\elements.json");

        foreach (Element element in Element.Elements.Values)
        {
            Debug.Log("generated element: " + element.SingularName);
        }

        Debug.Log("finished");

        Debug.Break();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
