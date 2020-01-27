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

    public class TestEntityEntityAttribute : EntityEntityAttribute
    {
        private Entity _entity;

        public TestEntityEntityAttribute(Entity entity)
        {
            _entity = entity;
        }

        public override Entity GetEntity()
        {
            return _entity;
        }
    }

    public class TestEntity : Entity
    {
        private class InternalEntity : Entity
        {
            private TestBooleanEntityAttribute _boolAttribute = new TestBooleanEntityAttribute(true);

            public override EntityAttribute GetAttribute(string attributeId)
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

        private TestEntityEntityAttribute _entityAttribute;

        public TestEntity()
        {
            _entityAttribute = new TestEntityEntityAttribute(_internalEntity);
        }

        public override EntityAttribute GetAttribute(string attributeId)
        {
            switch (attributeId)
            {
                case "testBoolAttribute":
                    return _boolAttribute;

                case "testEntityAttribute":
                    return _entityAttribute;
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
        Assert.AreEqual((expression as NumericExpression).GetValue(), -5);

        expression = Expression.BuildExpression(testContext, "!false");
        Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "1 + 1");
        Assert.AreEqual((expression as NumericExpression).GetValue(), 2);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "1 + -1 + 2");
        Assert.AreEqual((expression as NumericExpression).GetValue(), 2);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "-1 + 2 + 2");
        Assert.AreEqual((expression as NumericExpression).GetValue(), 3);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "2 +2+3");
        Assert.AreEqual((expression as NumericExpression).GetValue(), 7);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testContextNumericExpression");
        Assert.AreEqual((expression as NumericExpression).GetValue(), -15);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testContextBooleanExpression");
        Assert.AreEqual((expression as BooleanExpression).GetValue(), false);

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testEntity.testBoolAttribute");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as BooleanExpression).GetValue(), false);

        expression = Expression.BuildExpression(
            testContext, "testEntity.testEntityAttribute.testBoolAttribute");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        expression = Expression.BuildExpression(
            testContext, "lerp(3, -1, 0.5)");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        Assert.AreEqual((expression as NumericExpression).GetValue(), 1);

        expression = Expression.BuildExpression(
            testContext, "testFunction1()");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        expression = Expression.BuildExpression(
            testContext, "testFunction2(true)");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);

        expression = Expression.BuildExpression(
            testContext, "testFunction3(false ,3 +3, -5)");

        //Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
        //Assert.AreEqual((expression as BooleanExpression).GetValue(), true);
    }
}
