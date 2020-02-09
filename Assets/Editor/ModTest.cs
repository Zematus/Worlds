using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;
using System;

public class ModTest
{
    public class TestContext : Context
    {
        public TestContext() : base("testContext")
        {
        }
    }

    public class TestBooleanEntityAttribute : BooleanEntityAttribute
    {
        private bool _value;

        public TestBooleanEntityAttribute(bool value)
        {
            _value = value;
        }

        public override bool GetValue()
        {
            return _value;
        }
    }

    public class TestNumericFunctionEntityAttribute : NumericEntityAttribute
    {
        private BooleanExpression _argument;

        public TestNumericFunctionEntityAttribute(Expression[] arguments)
        {
            if ((arguments == null) || (arguments.Length < 1))
            {
                throw new System.ArgumentException("Number of arguments less than 1");
            }

            _argument = BooleanExpression.ValidateExpression(arguments[0]);
        }

        public override float GetValue()
        {
            return (_argument.GetValue()) ? 10 : 2;
        }
    }

    public class TestEntity : Entity
    {
        private class InternalEntity : Entity
        {
            private TestBooleanEntityAttribute _boolAttribute = new TestBooleanEntityAttribute(true);

            public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
            {
                switch (attributeId)
                {
                    case "testBoolAttribute":
                        return _boolAttribute;
                }

                return null;
            }
        }

        private InternalEntity _internalEntity = new InternalEntity();

        private TestBooleanEntityAttribute _boolAttribute = new TestBooleanEntityAttribute(false);

        private FixedEntityEntityAttribute _entityAttribute;

        public TestEntity()
        {
            _entityAttribute = new FixedEntityEntityAttribute(_internalEntity);
        }

        public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
        {
            switch (attributeId)
            {
                case "testBoolAttribute":
                    return _boolAttribute;

                case "testEntityAttribute":
                    return _entityAttribute;

                case "testNumericFunctionAttribute":
                    return new TestNumericFunctionEntityAttribute(arguments);
            }

            return null;
        }
    }

    [Test]
    public void ExpressionParseTest()
    {
        int expCounter = 1;

        TestContext testContext = new TestContext();

        testContext.Expressions.Add(
            "testContextNumericExpression",
            Expression.BuildExpression(testContext, "-15"));
        testContext.Expressions.Add(
            "testContextBooleanExpression",
            Expression.BuildExpression(testContext, "!true"));

        testContext.Entities.Add("testEntity", new TestEntity());

        Expression expression = Expression.BuildExpression(testContext, "-5");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(-5, (expression as NumericExpression).GetValue());

        expression = Expression.BuildExpression(testContext, "!false");
        Assert.AreEqual(true, (expression as BooleanExpression).GetValue());

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "1 + 1");
        Assert.AreEqual(2, (expression as NumericExpression).GetValue());

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "1 + -1 + 2");
        Assert.AreEqual(2, (expression as NumericExpression).GetValue());

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "-1 + 2 + 2");
        Assert.AreEqual(3, (expression as NumericExpression).GetValue());

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "2 +2+3");
        Assert.AreEqual(7, (expression as NumericExpression).GetValue());

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testContextNumericExpression");
        Assert.AreEqual(-15, (expression as NumericExpression).GetValue());

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testContextBooleanExpression");
        Assert.AreEqual(false, (expression as BooleanExpression).GetValue());

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testEntity.testBoolAttribute");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(false, (expression as BooleanExpression).GetValue());

        expression = Expression.BuildExpression(
            testContext, "testEntity.testEntityAttribute.testBoolAttribute");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(true, (expression as BooleanExpression).GetValue());

        expression = Expression.BuildExpression(
            testContext, "lerp(3, -1, 0.5)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(1, (expression as NumericExpression).GetValue());

        expression = Expression.BuildExpression(
            testContext, "lerp(4, (1 - 2), 0.1)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(3.5f, (expression as NumericExpression).GetValue());

        expression = Expression.BuildExpression(
            testContext, "2 + (1 + lerp(3, -1, 0.5))");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(4, (expression as NumericExpression).GetValue());

        expression = Expression.BuildExpression(
            testContext, "2 + lerp(0.5 + 0.5 + 2, -1, 0.5) + 1");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(4, (expression as NumericExpression).GetValue());

        expression =
            Expression.BuildExpression(testContext, "testEntity.testNumericFunctionAttribute(true)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(10, (expression as NumericExpression).GetValue());

        expression =
            Expression.BuildExpression(testContext, "testEntity.testNumericFunctionAttribute(false)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual(2, (expression as NumericExpression).GetValue());

        //expression = Expression.BuildExpression(
        //    testContext, "testFunction1()");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        //expression = Expression.BuildExpression(
        //    testContext, "testFunction2(true)");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        //expression = Expression.BuildExpression(
        //    testContext, "testFunction3(false ,3 +3, -5)");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);
    }

    [Test]
    public void GroupEntityEvalTest()
    {
        Manager.UpdateMainThreadReference();

        int expCounter = 1;

        TestContext testContext = new TestContext();

        GroupEntity testGroupEntity = new GroupEntity();

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

        testContext.Entities.Add("group", testGroupEntity);

        Expression expression = 
            Expression.BuildExpression(testContext, "group.cell.biome_trait_presence(wood)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testGroupEntity.Set(testGroup1);

        float numResult = (expression as NumericExpression).GetValue();
        Debug.Log("Expression evaluation result - 'testGroup1': " + numResult);
        Assert.IsTrue(numResult.IsInsideRange(0.29f, 0.31f));

        testGroupEntity.Set(testGroup2);
        expression.Reset();

        numResult = (expression as NumericExpression).GetValue();
        Debug.Log("Expression evaluation result - 'testGroup2': " + numResult);
        Assert.IsTrue(numResult.IsInsideRange(0.44f, 0.46f));

        /////

        expression =
             Expression.BuildExpression(testContext, "group.cell.biome_trait_presence(wood) > 0.4");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testGroupEntity.Set(testGroup1);

        bool boolResult = (expression as BooleanExpression).GetValue();
        Debug.Log("Expression evaluation result - 'testGroup1': " + boolResult);
        Assert.AreEqual(false, boolResult);

        testGroupEntity.Set(testGroup2);
        expression.Reset();

        boolResult = (expression as BooleanExpression).GetValue();
        Debug.Log("Expression evaluation result - 'testGroup2': " + boolResult);
        Assert.AreEqual(true, boolResult);
    }

    [Test]
    public void FactionEntityEvalTest()
    {
        Manager.UpdateMainThreadReference();

        int expCounter = 1;

        TestContext testContext = new TestContext();

        FactionEntity testFactionEntity = new FactionEntity();

        testContext.Entities.Add("faction", testFactionEntity);

        CellGroup.ResetEventGenerators();
        Knowledge.ResetKnowledges();
        Knowledge.InitializeKnowledges();

        World testWorld = new World(400, 200, 1);
        testWorld.TerrainInitialization();

        TerrainCell testCell1 = testWorld.TerrainCells[0][0];
        CellGroup testGroup1 = new CellGroup(testWorld, testCell1, 1234);

        TerrainCell testCell2 = testWorld.TerrainCells[10][10];
        CellGroup testGroup2 = new CellGroup(testWorld, testCell2, 2345);

        Tribe testTribe1 = new Tribe(testGroup1);
        Clan testClan1 = new Clan(testTribe1, testGroup1, 0);
        Clan testClan2 = new Clan(testTribe1, testGroup2, 0);

        Expression expression =
            Expression.BuildExpression(testContext, "faction.type");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        testFactionEntity.Set(testClan1);

        string type = (expression as StringExpression).GetValue();
        Debug.Log("Expression evaluation result - 'testClan1': " + type);
        Assert.AreEqual("clan", type);

        testFactionEntity.Set(testClan2);
        expression.Reset();

        type = (expression as StringExpression).GetValue();
        Debug.Log("Expression evaluation result - 'testClan2': " + type);
        Assert.AreEqual("clan", type);
    }
}
