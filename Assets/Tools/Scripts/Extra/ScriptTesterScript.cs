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
        Debug.Log("loading JSON...");

        foreach (Biome biome in BiomeLoader.Load(@"D:\Projects\GitHub\Worlds\Mods\Base\biomes.json"))
        {
            Debug.Log("generated biome: " + biome.Name);
        }

        Debug.Log("finished");

        Debug.Break();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
