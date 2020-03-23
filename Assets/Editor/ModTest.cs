using UnityEngine;
using NUnit.Framework;
using System.IO;

public class ModTest
{
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

        expression =
            ExpressionBuilder.BuildExpression(testContext, "testEntity.testNumericFunctionAttribute(true)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(10, (expression as IValueExpression<float>).Value);

        expression =
            ExpressionBuilder.BuildExpression(testContext, "testEntity.testNumericFunctionAttribute(false)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(2, (expression as IValueExpression<float>).Value);
    }

    [Test]
    public void GroupEntityEvalTest()
    {
        Manager.UpdateMainThreadReference();

        int expCounter = 1;

        TestContext testContext = new TestContext();

        GroupEntity testGroupEntity = new GroupEntity("target");

        Biome.ResetBiomes();
        Biome.LoadBiomesFile(Path.Combine("Mods", "Base", "Biomes", "biomes.json"));

        CellGroup.ResetEventGenerators();
        Knowledge.ResetKnowledges();
        Knowledge.InitializeKnowledges();

        World testWorld = new World(400, 200, 1);
        testWorld.TerrainInitialization();

        TerrainCell testCell1 = testWorld.TerrainCells[0][0];
        testCell1.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        testCell1.AddBiomeRelPresence(Biome.Biomes["grassland"], 0.7f);

        CellGroup testGroup1 = new CellGroup(testWorld, testCell1, 1234);

        TerrainCell testCell2 = testWorld.TerrainCells[50][50];
        testCell2.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        testCell2.AddBiomeRelPresence(Biome.Biomes["taiga"], 0.15f);
        testCell2.AddBiomeRelPresence(Biome.Biomes["desert"], 0.55f);

        CellGroup testGroup2 = new CellGroup(testWorld, testCell2, 35000);

        testContext.AddEntity(testGroupEntity);

        IExpression expression =
            ExpressionBuilder.BuildExpression(testContext, "target.cell.biome_trait_presence(wood)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testGroupEntity.Set(testGroup1);

        float numResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testGroup1': " + numResult);
        Assert.IsTrue(numResult.IsInsideRange(0.29f, 0.31f));

        testGroupEntity.Set(testGroup2);

        numResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testGroup2': " + numResult);
        Assert.IsTrue(numResult.IsInsideRange(0.44f, 0.46f));

        /////

        expression =
             ExpressionBuilder.BuildExpression(testContext, "target.cell.biome_trait_presence(wood) > 0.4");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testGroupEntity.Set(testGroup1);

        bool boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testGroup1': " + boolResult);
        Assert.AreEqual(false, boolResult);

        testGroupEntity.Set(testGroup2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testGroup2': " + boolResult);
        Assert.AreEqual(true, boolResult);
    }

    [Test]
    public void FactionEntityEvalTest()
    {
        Manager.UpdateMainThreadReference();

        int expCounter = 1;

        TestContext testContext = new TestContext();

        FactionEntity testFactionEntity = new FactionEntity("target");

        testContext.AddEntity(testFactionEntity);

        CellGroup.ResetEventGenerators();
        Knowledge.ResetKnowledges();
        Knowledge.InitializeKnowledges();
        CulturalPreference.InitializePreferences();

        World testWorld = new World(400, 200, 1);
        testWorld.TerrainInitialization();

        TerrainCell testCell1 = testWorld.TerrainCells[0][0];
        TerrainCell testCell2 = testWorld.TerrainCells[10][10];
        CellGroup testGroup1 = new CellGroup(testWorld, testCell1, 1234);
        CellGroup testGroup2 = new CellGroup(testWorld, testCell2, 1234);

        TestPolity testPolity1 = new TestPolity("tribe", testGroup1);
        TestFaction testFaction1 = new TestFaction("clan", testPolity1, testGroup1, 0, 0.3f);
        TestFaction testFaction2 = new TestFaction("clan", testPolity1, testGroup2, 0, 0.7f);

        testGroup1.Culture.Language = testPolity1.Culture.Language;
        testGroup2.Culture.Language = testPolity1.Culture.Language;

        CellRegion testRegion = new CellRegion(testCell1, testGroup1.Culture.Language);
        testCell1.Region = testRegion;
        testCell2.Region = testRegion;

        testFaction1.TestLeader = new Agent(testFaction1.CoreGroup, 0, 0);
        testFaction2.TestLeader = new Agent(testFaction2.CoreGroup, 0, 0);

        testFaction1.Culture.GetPreference("authority").Value = 0.4f;
        testFaction1.Culture.GetPreference("cohesion").Value = 0.6f;

        testFaction2.Culture.GetPreference("authority").Value = 0.6f;
        testFaction2.Culture.GetPreference("cohesion").Value = 0.8f;

        ////

        IExpression expression =
            ExpressionBuilder.BuildExpression(testContext, "target.type");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testFactionEntity.Set(testFaction1);

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

        testFactionEntity.Set(testFaction2);

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + floatResult);
        Assert.AreEqual(0.7f, floatResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.administrative_load > 0.5");

        testFactionEntity.Set(testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsFalse(boolResult);

        testFactionEntity.Set(testFaction2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + boolResult);
        Assert.IsTrue(boolResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.preferences.cohesion < 0.7");

        testFactionEntity.Set(testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsTrue(boolResult);

        testFactionEntity.Set(testFaction2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + boolResult);
        Assert.IsFalse(boolResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.preferences.authority > 0.5");

        testFactionEntity.Set(testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsFalse(boolResult);

        testFactionEntity.Set(testFaction2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + boolResult);
        Assert.IsTrue(boolResult);

        ////

        expression = ExpressionBuilder.BuildExpression(
            testContext,
            "91250 * (1 - target.administrative_load) * target.preferences.cohesion");

        testFactionEntity.Set(testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + floatResult);
        Assert.AreEqual(38325, floatResult);

        testFactionEntity.Set(testFaction2);

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + floatResult);
        Assert.AreEqual(21900, floatResult);

        ////

        expression = ExpressionBuilder.BuildExpression(
            testContext,
            "!(target.preferences.cohesion > 0.7)");

        testFactionEntity.Set(testFaction1);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction1': " + boolResult);
        Assert.IsTrue(boolResult);

        testFactionEntity.Set(testFaction2);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - 'testFaction2': " + boolResult);
        Assert.IsFalse(boolResult);
    }

    [Test]
    public void LoadEventsModTest()
    {
        Manager.UpdateMainThreadReference();

        Debug.Log("loading event mod file...");

        CulturalPreference.InitializePreferences();

        EventGenerator.ResetGenerators();
        EventGenerator.LoadEventFile(Path.Combine("Mods", "Base", "Events", "events.json"));

        foreach (EventGenerator generator in EventGenerator.Generators.Values)
        {
            Debug.Log("created event generator: " + generator.Name);
        }
    }

    [Test]
    public void LoadDecisionsModTest()
    {
        Manager.UpdateMainThreadReference();

        Debug.Log("loading clans split decision mod file...");

        CulturalPreference.InitializePreferences();

        ModDecision.ResetDecisions();
        ModDecision.LoadDecisionFile(Path.Combine("Mods", "Base", "Decisions", "clan_split.json"));

        foreach (ModDecision decision in ModDecision.Decisions.Values)
        {
            Debug.Log("created decision: " + decision.Name);
        }
    }
}
