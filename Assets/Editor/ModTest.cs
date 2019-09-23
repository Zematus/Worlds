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
    }

    [Test]
    public void LoadLayersModTest()
    {
        Manager.UpdateMainThreadReference();

        Debug.Log("loading layer mod file...");

        Layer.ResetLayers();
        Layer.LoadLayersFile(@"Mods\WeirdBiomesMod\Layers\weirdLayers.json");

        foreach (Layer layer in Layer.Layers.Values)
        {
            Debug.Log("generated layer: " + layer.Name);
        }
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
    }

    [Test]
    public void LoadDiscoveryModTest()
    {
        Manager.UpdateMainThreadReference();
        World.ResetStaticModData();

        Debug.Log("loading discovery mod file...");

        Discovery.ResetDiscoveries();
        Discovery.LoadDiscoveriesFile(@"Mods\Base\Discoveries\discoveries.json");

        foreach (Discovery discovery in Discovery.Discoveries.Values)
        {
            Debug.Log("generated discovery: " + discovery.Name);
        }
    }

    [Test]
    public void ConditionParseTest()
    {
        int condCounter = 1;

        string input = "[ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3";

        Condition condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3)";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "(([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3))";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3) [OR] ([NOT]cell_has_sea)";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "(([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3) [OR] ([NOT]cell_has_sea))";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3) [OR] (([ANY_N_CELL]cell_has_sea:0.10) [OR] cell_has_sea)";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3) [OR] ([ANY_N_CELL]cell_has_sea:0.10) [OR] ([NOT]cell_has_sea)";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3) [OR] ([ANY_N_CELL]cell_has_sea:0.10) [OR] ([NOT]cell_has_sea) [OR] ([ANY_N_CELL]cell_has_sea:0.30)";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "(([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3) [OR] ([ANY_N_CELL]cell_has_sea:0.10) [OR] ([NOT]cell_has_sea) [OR] ([ANY_N_CELL]cell_has_sea:0.30))";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

        input = "[NOT] (([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3) [OR] ([NOT]cell_has_sea) [OR] ([ANY_N_CELL]cell_has_sea:0.30))";

        condition = Condition.BuildCondition(input);

        Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());
    }

    [Test]
    public void FactorParseTest()
    {
        int factCounter = 1;

        Factor factor = Factor.BuildFactor("[INV]([SQ]neighborhood_sea_presence)");

        Debug.Log("Test factor " + (factCounter++) + ": " + factor.ToString());

        factor = Factor.BuildFactor("[SQ]([INV]neighborhood_sea_presence)");

        Debug.Log("Test factor " + (factCounter++) + ": " + factor.ToString());

        factor = Factor.BuildFactor("[SQ]([INV](neighborhood_sea_presence))");

        Debug.Log("Test factor " + (factCounter++) + ": " + factor.ToString());
    }

    // TODO: This test breaks the test runner for some reason. Investigate
    //[Test]
    //public void ConditionTypeTest()
    //{
    //    Biome.ResetBiomes();
    //    Biome.LoadBiomesFile(@"Mods\Base\Biomes\biomes.json");

    //    Layer.ResetLayers();
    //    Layer.LoadLayersFile(@"Mods\WeirdBiomesMod\Layers\weirdLayers.json");

    //    int condCounter = 1;

    //    string input = "cell_biome_presence:desert,0.3";

    //    Condition condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

    //    input = "cell_biome_most_present:grassland";

    //    condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

    //    input = "group_population:10000";

    //    condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

    //    input = "cell_layer_value:mycosystem,20";

    //    condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

    //    input = "cell_altitude:-1000";

    //    condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

    //    input = "cell_rainfall:100";

    //    condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

    //    input = "cell_temperature:-15";

    //    condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

    //    input = "cell_foraging_capacity:0.5";

    //    condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());

    //    input = "cell_survivability:0.6";

    //    condition = Condition.BuildCondition(input);

    //    Debug.Log("Test condition " + (condCounter++) + ": " + condition.ToString());
    //}
}
