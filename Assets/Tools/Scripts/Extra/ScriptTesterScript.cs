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

        Debug.Log("loading JSON...");

        Biome.LoadModFile(@"Mods\Base\biomes.json");

        foreach (Biome biome in Biome.Biomes.Values)
        {
            Debug.Log("generated biome: " + biome.Name);
        }

        foreach (Element element in ElementLoader.Load(@"Mods\Base\elements.json"))
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
