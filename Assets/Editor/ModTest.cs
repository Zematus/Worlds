using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class ModTest
{
    [Test]
    public void LoadBiomeModTest()
    {
        Manager.UpdateMainThreadReference();
        
        Debug.Log("loading biome mod file...");

        Biome.ResetBiomes();
        Biome.LoadBiomesFile(@"Mods\Base\Biomes\biomes.json");
        
        foreach (Biome biome in Biome.Biomes.Values)
        {
            Debug.Log("generated biome: " + biome.Name);
        }
        
        Debug.Log("finished");
    }

    [Test]
    public void LoadRegionAttributeModTest()
    {
        Manager.UpdateMainThreadReference();
        
        Debug.Log("loading region attribute mod file...");

        Adjective.ResetAdjectives();
        RegionAttribute.ResetAttributes();
        RegionAttribute.LoadRegionAttributesFile(@"Mods\Base\RegionAttributes\region_attributes.json");

        foreach (RegionAttribute regionAttribute in RegionAttribute.Attributes.Values)
        {
            Debug.Log("generated region attribute: " + regionAttribute.Name);
        }

        Debug.Log("finished");
    }

    [Test]
    public void LoadElementModTest()
    {
        Manager.UpdateMainThreadReference();
        
        Debug.Log("loading element mod file...");

        Adjective.ResetAdjectives();
        Element.ResetElements();
        Element.LoadElementsFile(@"Mods\Base\Elements\elements.json");

        foreach (Element element in Element.Elements.Values)
        {
            Debug.Log("generated element: " + element.SingularName);
        }

        Debug.Log("finished");
    }

    [Test]
    public void LoadDiscoveryModTest()
    {
        Manager.UpdateMainThreadReference();

        Debug.Log("loading discovery mod file...");

        Discovery.ResetDiscoveries();
        Discovery.LoadDiscoveriesFile(@"Mods\Base\Discoveries\discoveries.json");

        foreach (Discovery discovery in Discovery.Discoveries.Values)
        {
            Debug.Log("generated discovery: " + discovery.Name);
        }

        Debug.Log("finished");
    }
}
