using UnityEngine;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModTest
{
    private World _testWorld;
    private TerrainCell _testCell1;
    private TerrainCell _testCell2;
    private TerrainCell _testCell3;
    private TerrainCell _testCell4;
    private TerrainCell _testCell5;
    private TerrainCell _testCell6;
    private CellGroup _testGroup1;
    private CellGroup _testGroup2;
    private CellGroup _testGroup3;
    private CellGroup _testGroup4;
    private CellGroup _testGroup5;
    private CellGroup _testGroup6;
    CellRegion _testRegion1;
    CellRegion _testRegion2;
    private TestFaction _testFaction0;
    private TestFaction _testFaction1;
    private TestFaction _testFaction2;
    private TestFaction _testFaction3;
    private TestFaction _testFaction4;
    TestPolity _testPolity1;
    TestPolity _testPolity2;

    private void LoadBiomes()
    {
        Debug.Log("loading biome mod file...");

        Biome.ResetBiomes();
        Biome.LoadBiomesFile(Path.Combine("Mods", "Base", "Biomes", "biomes.json"));
    }

    [Test]
    public void LoadBiomeModTest()
    {
        Manager.UpdateMainThreadReference();

        LoadBiomes();

        foreach (Biome biome in Biome.Biomes.Values)
        {
            Debug.Log("loaded biome: " + biome.Name);
        }
    }

    private void LoadCulturalPreferences()
    {
        Debug.Log("loading cultural preferences mod file...");

        PreferenceGenerator.ResetPreferenceGenerators();
        PreferenceGenerator.LoadPreferencesFile(Path.Combine("Mods", "Base", "Preferences", "preferences.json"));
    }

    [Test]
    public void LoadCulturalPreferenceModTest()
    {
        Manager.UpdateMainThreadReference();

        LoadCulturalPreferences();

        foreach (PreferenceGenerator generator in PreferenceGenerator.Generators.Values)
        {
            Debug.Log("loaded preference generator: " + generator.Name);
        }
    }

    [Test]
    public void LoadLayersModTest()
    {
        Manager.UpdateMainThreadReference();

        Debug.Log("loading layer mod file...");

        Layer.ResetLayers();
        Layer.LoadLayersFile(Path.Combine("Mods", "WeirdBiomesMod", "Layers", "weirdLayers.json"));

        foreach (Layer layer in Layer.Layers.Values)
        {
            Debug.Log("loaded layer: " + layer.Name);
        }
    }

    private void LoadRegionAttributes()
    {
        Debug.Log("loading region attribute mod file...");

        Adjective.ResetAdjectives();
        RegionAttribute.ResetAttributes();
        RegionAttribute.LoadRegionAttributesFile(Path.Combine("Mods", "Base", "RegionAttributes", "region_attributes.json"));
    }

    [Test]
    public void LoadRegionAttributeModTest()
    {
        Manager.UpdateMainThreadReference();

        LoadRegionAttributes();

        foreach (RegionAttribute regionAttribute in RegionAttribute.Attributes.Values)
        {
            Debug.Log("loaded region attribute: " + regionAttribute.Name);
        }
    }

    private void LoadElements()
    {
        Debug.Log("loading element mod file...");

        Adjective.ResetAdjectives();
        Element.ResetElements();
        Element.LoadElementsFile(Path.Combine("Mods", "Base", "Elements", "elements.json"));
    }

    [Test]
    public void LoadElementModTest()
    {
        Manager.UpdateMainThreadReference();

        LoadElements();

        foreach (Element element in Element.Elements.Values)
        {
            Debug.Log("loaded element: " + element.SingularName);
        }
    }

    [Test]
    public void ModTextParseTest()
    {
        int testCounter = 1;

        TestContext testContext = new TestContext();

        testContext.AddEntity(new TestEntity(testContext));

        Debug.Log("Test text " + (testCounter++));
        ModText text = new ModText(testContext, "normal string");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("normal string", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "1 + 1 equals <<1 + 1>>");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("1 + 1 equals <b>2</b>", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "<<2 > 3>>, 2 is not greater than 3");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("<b>False</b>, 2 is not greater than 3", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "<<string>> and <<anotherString>>");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("<b>string</b> and <b>anotherString</b>", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "lerp(2,4,0.5) equals <<lerp(2,4,0.5)>>");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("lerp(2,4,0.5) equals <b>3</b>", text.EvaluateString());

        Debug.Log("Test text " + (testCounter++));
        text = new ModText(testContext, "space between the numbers <<5 + 2>> <<7>>");
        Debug.Log("evaluated text: " + text.EvaluateString());
        Assert.AreEqual("space between the numbers <b>7</b> <b>7</b>", text.EvaluateString());
    }

    [Test]
    public void ExpressionParseTest()
    {
        int expCounter = 1;

        TestContext testContext = new TestContext();

        testContext.AddEntity(new TestEntity(testContext));

        IExpression expression;

        expression = ExpressionBuilder.BuildExpression(testContext, "true && false");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(false, (expression as IValueExpression<bool>).Value);

        expression = ExpressionBuilder.BuildExpression(testContext, "true && true && true");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(true, (expression as IValueExpression<bool>).Value);

        expression = ExpressionBuilder.BuildExpression(testContext, "true && true && true && true");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(true, (expression as IValueExpression<bool>).Value);

        expression = ExpressionBuilder.BuildExpression(testContext, "false || false || true");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(true, (expression as IValueExpression<bool>).Value);

        expression = ExpressionBuilder.BuildExpression(testContext, "-5");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(-5, (expression as IValueExpression<float>).Value);

        expression = ExpressionBuilder.BuildExpression(testContext, "!false");
        Assert.AreEqual(true, (expression as IValueExpression<bool>).Value);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = ExpressionBuilder.BuildExpression(testContext, "1 + 1");
        Assert.AreEqual(2, (expression as IValueExpression<float>).Value);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = ExpressionBuilder.BuildExpression(testContext, "1 + 1 + 2");
        Assert.AreEqual(4, (expression as IValueExpression<float>).Value);

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
            testContext, "((2))");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(2, (expression as IValueExpression<float>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "((2) + ((3 + 1)))");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(6, (expression as IValueExpression<float>).Value);

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

        expression = ExpressionBuilder.BuildExpression(
            testContext, "testEntity.testNumericFunctionAttribute(false)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(2, (expression as IValueExpression<float>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "''test string''");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("test string", (expression as IValueExpression<string>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "(''inner test string'')");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("inner test string", (expression as IValueExpression<string>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "((''inner test string 2''))");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("inner test string 2", (expression as IValueExpression<string>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "(((''inner test string 3'')))");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("inner test string 3", (expression as IValueExpression<string>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "''test string <<1 + 1>> with a expression''");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("test string <b>2</b> with a expression", (expression as IValueExpression<string>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "(''test string <<6 / 2>>'')");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("test string <b>3</b>", (expression as IValueExpression<string>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "(''test string (<<2 * 2>>)'')");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("test string (<b>4</b>)", (expression as IValueExpression<string>).Value);

        expression = ExpressionBuilder.BuildExpression(
            testContext, "((''test string (<<(2 * 2) + 0.5>>)''))");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual("test string (<b>4.5</b>)", (expression as IValueExpression<string>).Value);

        /// NOTE: The following commented tests do not pass because the parser can't
        /// can't interpret paranthesis enclosed texts with unclosed parenthesis within

        //expression = ExpressionBuilder.BuildExpression(
        //    testContext, "(''test string 5)'')");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual("test string 5)", (expression as IValueExpression<string>).Value);

        //string miniTest = "''";
        //Match match = Regex.Match(miniTest, ModParseUtility.InnerTextStartRegexPart);
        //Assert.True(match.Success, "mini test: match " + miniTest);

        //expression = ExpressionBuilder.BuildExpression(
        //    testContext, "(''(5.5'')");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual("(5.5", (expression as IValueExpression<string>).Value);

        //expression = ExpressionBuilder.BuildExpression(
        //    testContext, "(''test string (6'')");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual("test string (6", (expression as IValueExpression<string>).Value);

        //expression = ExpressionBuilder.BuildExpression(
        //    testContext, "((''test string 7)''))");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual("test string 7)", (expression as IValueExpression<string>).Value);

        //expression = ExpressionBuilder.BuildExpression(
        //    testContext, "((''test string (8))''))");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual("test string (8))", (expression as IValueExpression<string>).Value);
    }

    private void InitializeModData()
    {
        World.ResetStaticModData();
        CellGroup.ResetEventGenerators();
        Faction.ResetEventGenerators();
        Polity.ResetEventGenerators();
    }

    private void InitializeTestWorld()
    {
        Manager.UpdateMainThreadReference();

        InitializeModData();

        LoadBiomes();
        LoadRegionAttributes();
        LoadElements();

        LoadCulturalPreferences();

        Knowledge.ResetKnowledges();
        Knowledge.InitializeKnowledges();

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

        _testCell3 = _testWorld.TerrainCells[70][70];
        _testCell3.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        _testCell3.AddBiomeRelPresence(Biome.Biomes["grassland"], 0.7f);

        _testGroup3 = new CellGroup(_testWorld, _testCell3, 12345);

        _testCell4 = _testWorld.TerrainCells[90][90];
        _testCell4.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        _testCell4.AddBiomeRelPresence(Biome.Biomes["grassland"], 0.7f);

        _testGroup4 = new CellGroup(_testWorld, _testCell4, 54321);

        _testCell5 = _testWorld.TerrainCells[110][110];
        _testCell5.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        _testCell5.AddBiomeRelPresence(Biome.Biomes["grassland"], 0.7f);

        _testGroup5 = new CellGroup(_testWorld, _testCell5, 54321);

        _testCell6 = _testWorld.TerrainCells[130][130];
        _testCell6.AddBiomeRelPresence(Biome.Biomes["forest"], 0.3f);
        _testCell6.AddBiomeRelPresence(Biome.Biomes["grassland"], 0.7f);

        _testGroup6 = new CellGroup(_testWorld, _testCell6, 54321);
    }

    [Test]
    public void GroupEntityEvalTest()
    {
        InitializeTestGroups();

        int expCounter = 1;

        TestContext testContext = new TestContext();

        GroupEntity testGroupEntity = new GroupEntity(testContext, "target");

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

        TestFaction._testCounter = 0;

        _testPolity1 = new TestPolity("tribe", _testGroup1);
        _testFaction0 = new TestFaction("clan", _testPolity1, _testGroup1, 0.5f, null, 200000f);
        _testFaction1 = new TestFaction("clan", _testPolity1, _testGroup2, 0.3f, null, 150000f);
        _testFaction2 = new TestFaction("clan", _testPolity1, _testGroup3, 0.2f, null, 200000f);

        _testPolity1.SetDominantFaction(_testFaction0);

        _testPolity2 = new TestPolity("tribe", _testGroup5);
        _testFaction3 = new TestFaction("clan", _testPolity2, _testGroup5, 0.55f, null, 200000f);
        _testFaction4 = new TestFaction("clan", _testPolity2, _testGroup6, 0.45f, null, 300000f);

        _testPolity2.SetDominantFaction(_testFaction3);

        _testFaction0.Initialize();
        _testFaction1.Initialize();
        _testFaction2.Initialize();

        _testFaction3.Initialize();
        _testFaction4.Initialize();

        _testWorld.AddPolityInfo(_testPolity1);
        _testWorld.AddPolityInfo(_testPolity2);

        _testGroup1.AddPolityProminenceValueDelta(_testPolity1, 1);
        _testGroup2.AddPolityProminenceValueDelta(_testPolity1, 1);
        _testGroup3.AddPolityProminenceValueDelta(_testPolity1, 1);
        _testGroup4.AddPolityProminenceValueDelta(_testPolity1, 1);
        _testGroup5.AddPolityProminenceValueDelta(_testPolity2, 1);
        _testGroup6.AddPolityProminenceValueDelta(_testPolity2, 1);

        _testGroup1.Culture.Language = _testPolity1.Culture.Language;
        _testGroup2.Culture.Language = _testPolity1.Culture.Language;
        _testGroup3.Culture.Language = _testPolity1.Culture.Language;
        _testGroup4.Culture.Language = _testPolity1.Culture.Language;
        _testGroup5.Culture.Language = _testPolity2.Culture.Language;
        _testGroup6.Culture.Language = _testPolity2.Culture.Language;

        _testGroup1.Culture.AddKnowledge(
            new SocialOrganizationKnowledge(_testGroup1, 600, 600));
        _testGroup2.Culture.AddKnowledge(
            new SocialOrganizationKnowledge(_testGroup2, 600, 600));
        _testGroup3.Culture.AddKnowledge(
            new SocialOrganizationKnowledge(_testGroup3, 600, 600));
        _testGroup4.Culture.AddKnowledge(
            new SocialOrganizationKnowledge(_testGroup4, 600, 600));
        _testGroup5.Culture.AddKnowledge(
            new SocialOrganizationKnowledge(_testGroup5, 600, 600));
        _testGroup6.Culture.AddKnowledge(
            new SocialOrganizationKnowledge(_testGroup6, 600, 600));

        _testRegion1 = new TestCellRegion(_testCell1, _testGroup1.Culture.Language);
        _testCell1.Region = _testRegion1;
        _testCell2.Region = _testRegion1;
        _testCell3.Region = _testRegion1;
        _testCell4.Region = _testRegion1;

        _testRegion2 = new TestCellRegion(_testCell5, _testGroup5.Culture.Language);
        _testCell5.Region = _testRegion2;
        _testCell6.Region = _testRegion2;

        _testFaction0.TestLeader = new Agent(_testFaction0.CoreGroup, 0, 0);
        _testFaction1.TestLeader = new Agent(_testFaction1.CoreGroup, 0, 0);
        _testFaction2.TestLeader = new Agent(_testFaction2.CoreGroup, 0, 0);
        _testFaction3.TestLeader = new Agent(_testFaction3.CoreGroup, 0, 0);
        _testFaction4.TestLeader = new Agent(_testFaction4.CoreGroup, 0, 0);

        _testFaction0.Culture.AddKnowledge(
            new CulturalKnowledge(
                SocialOrganizationKnowledge.KnowledgeId,
                SocialOrganizationKnowledge.KnowledgeName,
                600));

        _testFaction0.Culture.GetPreference("authority").Value = 0.4f;
        _testFaction0.Culture.GetPreference("cohesion").Value = 0.6f;

        _testFaction1.Culture.GetPreference("authority").Value = 0.6f;
        _testFaction1.Culture.GetPreference("cohesion").Value = 0.8f;

        _testFaction2.Culture.GetPreference("authority").Value = 0.6f;
        _testFaction2.Culture.GetPreference("cohesion").Value = 0.6f;

        _testFaction3.Culture.GetPreference("authority").Value = 0.6f;
        _testFaction3.Culture.GetPreference("cohesion").Value = 0.6f;

        _testFaction4.Culture.GetPreference("authority").Value = 0.6f;
        _testFaction4.Culture.GetPreference("cohesion").Value = 0.6f;

        _testGroup1.UpdatePolityProminences_test();
        _testGroup2.UpdatePolityProminences_test();
        _testGroup3.UpdatePolityProminences_test();
        _testGroup4.UpdatePolityProminences_test();
        _testGroup5.UpdatePolityProminences_test();
        _testGroup6.UpdatePolityProminences_test();

        _testGroup1.SetProminenceCoreDistances_test(_testPolity1, 1001, 1001);
        _testGroup2.SetProminenceCoreDistances_test(_testPolity1, 1001, 1001);
        _testGroup3.SetProminenceCoreDistances_test(_testPolity1, 1001, 1001);
        _testGroup4.SetProminenceCoreDistances_test(_testPolity1, 1001, 1001);
        _testGroup5.SetProminenceCoreDistances_test(_testPolity2, 1001, 1001);
        _testGroup6.SetProminenceCoreDistances_test(_testPolity2, 1001, 1001);

        _testPolity1.ClusterUpdate();
        _testPolity2.ClusterUpdate();

        Faction.SetRelationship(_testFaction0, _testFaction1, 0.6f);

        Polity.AddContact(_testPolity1, _testPolity2, 1);
    }

    [Test]
    public void FactionEntityEvalTest()
    {
        InitializeTestFactions();

        int expCounter = 1;

        TestContext testContext = new TestContext();

        FactionEntity testFactionEntity = new FactionEntity(testContext, "target");

        testContext.AddEntity(testFactionEntity);

        ////

        IExpression expression =
            ExpressionBuilder.BuildExpression(testContext, "target.type");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testFactionEntity.Set(_testFaction0);

        string type = (expression as IValueExpression<string>).Value;
        Debug.Log("Expression evaluation result - '_testFaction0': " + type);
        Assert.AreEqual("clan", type);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.type == clan");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        bool boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction0': " + boolResult);
        Assert.IsTrue(boolResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.administrative_load");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        float floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - '_testFaction0': " + floatResult);
        Assert.AreEqual(200000f, floatResult);

        testFactionEntity.Set(_testFaction1);

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - '_testFaction1': " + floatResult);
        Assert.AreEqual(150000f, floatResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.administrative_load > 170000");

        testFactionEntity.Set(_testFaction0);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction0': " + boolResult);
        Assert.IsTrue(boolResult);

        testFactionEntity.Set(_testFaction1);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction1': " + boolResult);
        Assert.IsFalse(boolResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.preferences.cohesion < 0.7");

        testFactionEntity.Set(_testFaction0);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction0': " + boolResult);
        Assert.IsTrue(boolResult);

        testFactionEntity.Set(_testFaction1);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction1': " + boolResult);
        Assert.IsFalse(boolResult);

        ////

        expression =
            ExpressionBuilder.BuildExpression(testContext, "target.preferences.authority > 0.5");

        testFactionEntity.Set(_testFaction0);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction0': " + boolResult);
        Assert.IsFalse(boolResult);

        testFactionEntity.Set(_testFaction1);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction1': " + boolResult);
        Assert.IsTrue(boolResult);

        ////

        expression = ExpressionBuilder.BuildExpression(
            testContext,
            "9125 + (91250 * (1 - saturation(target.administrative_load, 400000)) * target.preferences.cohesion)");

        testFactionEntity.Set(_testFaction0);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - '_testFaction0': " + floatResult);
        Assert.AreEqual(45625, floatResult);

        testFactionEntity.Set(_testFaction1);

        floatResult = (expression as IValueExpression<float>).Value;
        Debug.Log("Expression evaluation result - '_testFaction1': " + floatResult);
        Assert.AreEqual(62215.9141f, floatResult);

        ////

        expression = ExpressionBuilder.BuildExpression(
            testContext,
            "!(target.preferences.cohesion > 0.7)");

        testFactionEntity.Set(_testFaction0);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction0': " + boolResult);
        Assert.IsTrue(boolResult);

        testFactionEntity.Set(_testFaction1);

        boolResult = (expression as IValueExpression<bool>).Value;
        Debug.Log("Expression evaluation result - '_testFaction1': " + boolResult);
        Assert.IsFalse(boolResult);
    }

    private void LoadBaseEventsMod()
    {
        Debug.Log("loading event mod file...");

        EventGenerator.ResetGenerators();
        EventGenerator.LoadEventFile(Path.Combine("Mods", "Base", "Events", "events.json"));
    }

    [Test]
    public void LoadEventsModTest()
    {
        Manager.UpdateMainThreadReference();

        Knowledge.ResetKnowledges();
        Knowledge.InitializeKnowledges();

        LoadCulturalPreferences();

        LoadBaseEventsMod();

        foreach (EventGenerator generator in EventGenerator.Generators.Values)
        {
            Debug.Log("created event generator: " + generator.Name);
        }
    }

    private void LoadBaseActionCategoriesMod()
    {
        Debug.Log("loading action categories mod file...");

        ActionCategory.ResetActionCategories();
        ActionCategory.LoadActionCategoryFile(
            Path.Combine("Mods", "Base", "Actions", "Categories", "categories.json"));
    }

    private void LoadBaseActionsMod()
    {
        Debug.Log("loading action mod file...");

        ModAction.ResetActions();
        LoadBaseActionCategoriesMod();

        ModAction.LoadActionFile(Path.Combine("Mods", "Base", "Actions", "actions.json"));
    }

    [Test]
    public void LoadActionsModTest()
    {
        Manager.UpdateMainThreadReference();

        Knowledge.ResetKnowledges();
        Knowledge.InitializeKnowledges();

        LoadCulturalPreferences();

        LoadBaseActionsMod();

        foreach (ModAction action in ModAction.Actions.Values)
        {
            Debug.Log("created action: " + action.Name);
        }
    }

    private void LoadBaseDecisionsMod()
    {
        Debug.Log("loading decision mod files...");

        ModDecision.ResetDecisions();

        string path = Path.Combine("Mods", "Base", "Decisions");

        string[] files = Directory.GetFiles(path, "*.json");

        if (files.Length > 0)
        {
            foreach (string file in files)
            {
                ModDecision.LoadDecisionFile(file);
            }
        }
    }

    [Test]
    public void LoadDecisionsModTest()
    {
        Manager.UpdateMainThreadReference();

        LoadCulturalPreferences();

        LoadBaseDecisionsMod();

        foreach (ModDecision decision in ModDecision.Decisions.Values)
        {
            Debug.Log("created decision: " + decision.Name);
        }
    }

    /// <summary>
    /// Tests triggering a specific faction event
    /// </summary>
    /// <param name="testFaction">the test faction to use</param>
    /// <param name="eventId">the event generator id</param>
    private void TriggerFactionModEventTest(Faction testFaction, string eventId)
    {
        List<WorldEvent> eventsToHappen = _testWorld.GetEventsToHappen();

        Debug.Log("Number of events to happen: " + eventsToHappen.Count);

        FactionModEvent modEvent = null;

        foreach (FactionModEvent e in eventsToHappen)
        {
            if ((e.GeneratorId == eventId) &&
                (e.Generator.Target.Faction == testFaction))
            {
                modEvent = e;
                break;
            }
        }

        Debug.Log("Assert.IsNotNull(modEvent)");
        Assert.IsNotNull(modEvent);

        Debug.Log("Assert.IsTrue(modEvent.CanTrigger())");
        Assert.IsTrue(modEvent.CanTrigger());

        Debug.Log("modEvent.Trigger()");
        modEvent.Trigger();
    }

    // NOTE: mod script is changed frequently, whcih breaks the test. So keeping it
    // disabled for the time being...
    //[Test]
    //public void TriggerSplitClanDecision()
    //{
    //    Manager.CurrentDevMode = DevMode.Advanced;

    //    InitializeTestFactions();

    //    LoadBaseEventsMod();
    //    LoadBaseDecisionsMod();

    //    EventGenerator.InitializeGenerators();

    //    _testFaction2.InitializeDefaultEvents();

    //    TriggerFactionModEventTest(_testFaction2, "clan_decide_split");
    //}

    [Test]
    public void TriggerDemandInfluenceDecision()
    {
        Manager.CurrentDevMode = DevMode.Advanced;

        InitializeTestFactions();

        LoadBaseEventsMod();
        LoadBaseDecisionsMod();

        EventGenerator.InitializeGenerators();

        _testFaction1.InitializeDefaultEvents();

        TriggerFactionModEventTest(_testFaction1, "clan_decide_performing_influence_demand");
    }

    [Test]
    public void TriggerFosterRelationshipDecision()
    {
        Manager.CurrentDevMode = DevMode.Advanced;

        InitializeTestFactions();

        LoadBaseEventsMod();
        LoadBaseDecisionsMod();

        EventGenerator.InitializeGenerators();

        _testFaction0.InitializeDefaultEvents();

        TriggerFactionModEventTest(_testFaction0, "tribe_decide_fostering_relationship");
    }

    [Test]
    public void TriggerMergeTribeDecision()
    {
        Manager.CurrentDevMode = DevMode.Advanced;

        InitializeTestFactions();

        LoadBaseEventsMod();
        LoadBaseDecisionsMod();

        EventGenerator.InitializeGenerators();

        _testFaction0.InitializeDefaultEvents();

        TriggerFactionModEventTest(_testFaction0, "tribe_decide_merge");
    }

    [Test]
    public void TriggerFormNewTribeDecision()
    {
        Manager.CurrentDevMode = DevMode.Advanced;

        InitializeTestFactions();

        LoadBaseEventsMod();
        LoadBaseDecisionsMod();

        EventGenerator.InitializeGenerators();

        _testFaction1.InitializeDefaultEvents();

        TriggerFactionModEventTest(_testFaction1, "clan_decide_form_new_tribe");
    }
}
