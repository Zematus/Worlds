using UnityEngine;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

public class ModTest
{
    private World _testWorld;
    private TerrainCell _testCell1;
    private TerrainCell _testCell2;
    private TerrainCell _testCell3;
    private TerrainCell _testCell4;
    private CellGroup _testGroup1;
    private CellGroup _testGroup2;
    private CellGroup _testGroup3;
    private CellGroup _testGroup4;
    CellRegion _testRegion1;
    private TestFaction _testFaction1;
    private TestFaction _testFaction2;
    private TestFaction _testFaction3;
    TestPolity _testPolity1;

    [Test]
    public void ModTextParseTest()
    {
        int testCounter = 1;

        TestContext testContext = new TestContext();

        testContext.AddEntity(new TestEntity());

        Debug.Log("Test text " + (testCounter++));
        ModText text = new ModText(testContext, "normal string");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("normal string", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "1 + 1 equals <<1 + 1>>");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("1 + 1 equals 2", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "<<2 > 3>>, 2 is not greater than 3");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("False, 2 is not greater than 3", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "<<string>> and <<anotherString>>");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("string and anotherString", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "lerp(2,4,0.5) equals <<lerp(2,4,0.5)>>");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("lerp(2,4,0.5) equals 3", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "space between the numbers <<5 + 2>> <<7>>");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("space between the numbers 7 7", text.EvaluateString());
    }

    [Test]
    public void ExpressionParseTest()
    {
        int expCounter = 1;

        TestContext testContext = new TestContext();

        testContext.AddEntity(new TestEntity());

        IExpression expression = ExpressionBuilder.BuildExpression(testContext, "-5");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(-5, (expression as IValueExpression<float>).Value);

        expression = ExpressionBuilder.BuildExpression(testContext, "!false");
        Assert.AreEqual(true, (expression as IValueExpression<bool>).Value);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = ExpressionBuilder.BuildExpression(testContext, "1 + 1");
        Assert.AreEqual(2, (expression as IValueExpression<float>).Value);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = ExpressionBuilder.BuildExpression(testContext, "1 + -1 + 2");
        Assert.AreEqual(2, (expression as IValueExpression<float>).Value);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = ExpressionBuilder.BuildExpression(testContext, "-1 + 2 + 2");
        Assert.AreEqual(3, (expression as IValueExpression<float>).Value);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = ExpressionBuilder.BuildExpression(testContext, "2 +2+3");
        Assert.AreEqual(7, (expression as IValueExpression<float>).Value);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = ExpressionBuilder.BuildExpression(testContext, "testEntity.testBoolAttribute");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(false, (expression as IValueExpression<bool>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "testEntity.testEntityAttribute.testBoolAttribute");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(true, (expression as IValueExpression<bool>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "lerp(3, -1, 0.5)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(1, (expression as IValueExpression<float>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "lerp(4, (1 - 2), 0.1)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(3.5f, (expression as IValueExpression<float>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "2 + (1 + lerp(3, -1, 0.5))");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(4, (expression as IValueExpression<float>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "2 + lerp(0.5 + 0.5 + 2, -1, 0.5) + 1");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(4, (expression as IValueExpression<float>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "percent(0.58756)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("58.76 %", (expression as IValueExpression<string>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "testEntity.testNumericFunctionAttribute(true)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(10, (expression as IValueExpression<float>).Value);

        expression =
            ExpressionBuilder.BuildExpression(testContext, "testEntity.testNumericFunctionAttribute(false)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(2, (expression as IValueExpression<float>).Value);
    }

    private void InitializeModData()
    {
        World.ResetStaticModData();
        CellGroup.ResetEventGenerators();
        Faction.ResetEventGenerators();
    }

    private void InitializeTestWorld()
    {
        Manager.UpdateMainThreadReference();

        InitializeModData();

        Biome.ResetBiomes();
        Biome.LoadBiomesFile(Path.Combine("Mods", "Base", "Biomes", "biomes.json"));

        Knowledge.ResetKnowledges();
        Knowledge.InitializeKnowledges();
        CulturalPreference.InitializePreferences();

        _testWorld = new World(400, 200, 1);
        _testWorld.TerrainInitialization();
    }

    private void InitializeTestGroups()
    {
        InitializeTestWorld();

        _testCell1 = _testWorld.TerrainCells[0][0];
        _testCell1.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        _testCell1.AddBiomeRelPresence(Biome.Biomes["grassland"], 0.7f);

        _testGroup1 = new CellGroup(_testWorld, _testCell1, 1234);

        _testCell2 = _testWorld.TerrainCells[50][50];
        _testCell2.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        _testCell2.AddBiomeRelPresence(Biome.Biomes["taiga"], 0.15f);
        _testCell2.AddBiomeRelPresence(Biome.Biomes["desert"], 0.55f);

        _testGroup2 = new CellGroup(_testWorld, _testCell2, 35000);

        _testCell3 = _testWorld.TerrainCells[100][100];
        _testCell3.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        _testCell3.AddBiomeRelPresence(Biome.Biomes["grassland"], 0.7f);

        _testGroup3 = new CellGroup(_testWorld, _testCell3, 12345);

        _testCell4 = _testWorld.TerrainCells[150][150];
        _testCell4.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        _testCell4.AddBiomeRelPresence(Biome.Biomes["grassland"], 0.7f);

        _testGroup4 = new CellGroup(_testWorld, _testCell4, 54321);
    }

    [Test]
    public void GroupEntityEvalTest()
    {
        InitializeTestGroups();

        int expCounter = 1;

        TestContext testContext = new TestContext();

        GroupEntity testGroupEntity = new GroupEntity("target");

        testContext.AddEntity(testGroupEntity);

        /////

        IExpression expression =
            ExpressionBuilder.BuildExpression(testContext, "target.cell.biome_trait_presence(wood)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testGroupEntity.Set(_testGroup1);

        float numResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testGroup1': " + numResult);
        Assert.IsTrue(numResult.IsInsideRange(0.29f, 0.31f));

        testGroupEntity.Set(_testGroup2);

        numResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testGroup2': " + numResult);
        Assert.IsTrue(numResult.IsInsideRange(0.44f, 0.46f));

        /////

        expression =
             ExpressionBuilder.BuildExpression(testContext, "target.cell.biome_trait_presence(wood) > 0.4");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testGroupEntity.Set(_testGroup1);

        bool boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testGroup1': " + boolResult);
        Assert.AreEqual(false, boolResult);

        testGroupEntity.Set(_testGroup2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testGroup2': " + boolResult);
        Assert.AreEqual(true, boolResult);
    }

    private void InitializeTestFactions()
    {
        InitializeTestGroups();

        _testPolity1 = new TestPolity("tribe", _testGroup1);
        _testFaction1 = new TestFaction("clan", _testPolity1, _testGroup1, 0, null, 0.3f);
        _testFaction2 = new TestFaction("clan", _testPolity1, _testGroup2, 0, null, 0.7f);
        _testFaction3 = new TestFaction("clan", _testPolity1, _testGroup3, 0, null, 0.7f);

        _testFaction1.Initialize();
        _testFaction2.Initialize();
        _testFaction3.Initialize();

        _testWorld.AddPolityInfo(_testPolity1);

        _testGroup1.SetPolityProminence(_testPolity1, 1);
        _testGroup2.SetPolityProminence(_testPolity1, 1);
        _testGroup3.SetPolityProminence(_testPolity1, 1);
        _testGroup4.SetPolityProminence(_testPolity1, 1);

        _testGroup1.Culture.Language = _testPolity1.Culture.Language;
        _testGroup2.Culture.Language = _testPolity1.Culture.Language;
        _testGroup3.Culture.Language = _testPolity1.Culture.Language;
        _testGroup4.Culture.Language = _testPolity1.Culture.Language;

        _testRegion1 = new TestCellRegion(_testCell1, _testGroup1.Culture.Language);
        _testCell1.Region = _testRegion1;
        _testCell2.Region = _testRegion1;
        _testCell3.Region = _testRegion1;
        _testCell4.Region = _testRegion1;

        _testFaction1.TestLeader = new Agent(_testFaction1.CoreGroup, 0, 0);
        _testFaction2.TestLeader = new Agent(_testFaction2.CoreGroup, 0, 0);
        _testFaction3.TestLeader = new Agent(_testFaction3.CoreGroup, 0, 0);

        _testFaction1.Culture.GetPreference("authority").Value = 0.4f;
        _testFaction1.Culture.GetPreference("cohesion").Value = 0.6f;

        _testFaction2.Culture.GetPreference("authority").Value = 0.6f;
        _testFaction2.Culture.GetPreference("cohesion").Value = 0.8f;

        _testFaction3.Culture.GetPreference("authority").Value = 0.6f;
        _testFaction3.Culture.GetPreference("cohesion").Value = 0.6f;

        _testGroup1.PostUpdatePolityProminences_BeforePolityUpdates();
        _testGroup2.PostUpdatePolityProminences_BeforePolityUpdates();
        _testGroup3.PostUpdatePolityProminences_BeforePolityUpdates();
        _testGroup4.PostUpdatePolityProminences_BeforePolityUpdates();

        _testPolity1.ClusterUpdate();
    }

    [Test]
    public void FactionEntityEvalTest()
    {
        InitializeTestFactions();

        int expCounter = 1;

        TestContext testContext = new TestContext();

        FactionEntity testFactionEntity = new FactionEntity("target");

        testContext.AddEntity(testFactionEntity);

        ////

        IExpression expression =
            ExpressionBuilder.BuildExpression(testContext, "target.type");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testFactionEntity.Set(_testFaction1);

        string type = (expression as IValueExpression<string>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + type);
        Assert.AreEqual("clan", type);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.type == clan");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        bool boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsTrue(boolResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.administrative_load");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        float floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + floatResult);
        Assert.AreEqual(0.3f, floatResult);

        testFactionEntity.Set(_testFaction2);

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + floatResult);
        Assert.AreEqual(0.7f, floatResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.administrative_load > 0.5");

        testFactionEntity.Set(_testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsFalse(boolResult);

        testFactionEntity.Set(_testFaction2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + boolResult);
        Assert.IsTrue(boolResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.preferences.cohesion < 0.7");

        testFactionEntity.Set(_testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsTrue(boolResult);

        testFactionEntity.Set(_testFaction2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + boolResult);
        Assert.IsFalse(boolResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.preferences.authority > 0.5");

        testFactionEntity.Set(_testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsFalse(boolResult);

        testFactionEntity.Set(_testFaction2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + boolResult);
        Assert.IsTrue(boolResult);

        ////

        expression = ExpressionBuilder.BuildExpression(
            testContext,
            "91250 * (1 - target.administrative_load) * target.preferences.cohesion");

        testFactionEntity.Set(_testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + floatResult);
        Assert.AreEqual(38325, floatResult);

        testFactionEntity.Set(_testFaction2);

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + floatResult);
        Assert.AreEqual(21900, floatResult);

        ////

        expression = ExpressionBuilder.BuildExpression(
            testContext,
            "!(target.preferences.cohesion > 0.7)");

        testFactionEntity.Set(_testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsTrue(boolResult);

        testFactionEntity.Set(_testFaction2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + boolResult);
        Assert.IsFalse(boolResult);
    }

    private void LoadBaseEventsMod()
    {
        Debug.Log("loading event mod file...");

        CulturalPreference.InitializePreferences();

        EventGenerator.ResetGenerators();
        EventGenerator.LoadEventFile(Path.Combine("Mods", "Base", "Events", "events.json"));
    }

    [Test]
    public void LoadEventsModTest()
    {
        Manager.UpdateMainThreadReference();

        LoadBaseEventsMod();

        foreach (EventGenerator generator in EventGenerator.Generators.Values)
        {
            Debug.Log("created event generator: " + generator.Name);
        }
    }

    private void LoadBaseDecisionsMod()
    {
        Debug.Log("loading clans split decision mod file...");

        CulturalPreference.InitializePreferences();

        ModDecision.ResetDecisions();
        ModDecision.LoadDecisionFile(Path.Combine("Mods", "Base", "Decisions", "clan_split.json"));
    }

    [Test]
    public void LoadDecisionsModTest()
    {
        Manager.UpdateMainThreadReference();

        LoadBaseDecisionsMod();

        foreach (ModDecision decision in ModDecision.Decisions.Values)
        {
            Debug.Log("created decision: " + decision.Name);
        }
    }

    [Test]
    public void TriggerSplitClanDecision()
    {
        InitializeTestFactions();

        LoadBaseEventsMod();
        LoadBaseDecisionsMod();

        EventGenerator.InitializeGenerators();

        _testFaction3.InitializeDefaultEvents();

        List<WorldEvent> eventsToHappen = _testWorld.GetEventsToHappen();

        Debug.Log("Number of events to happen: " + eventsToHappen.Count);

        FactionModEvent splitEvent = null;

        foreach (FactionModEvent e in eventsToHappen)
        {
            if (e.GeneratorId == "clan_split")
            {
                splitEvent = e;
            }
        }

        Debug.Log("Assert.IsNotNull(splitEvent)");
        Assert.IsNotNull(splitEvent);

        Debug.Log("Assert.IsTrue(splitEvent.CanTrigger())");
        Assert.IsTrue(splitEvent.CanTrigger());

        Debug.Log("splitEvent.Trigger()");
        splitEvent.Trigger();
    }
}
