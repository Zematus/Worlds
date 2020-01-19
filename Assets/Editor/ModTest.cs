using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;

public class ModTest
{
    public class TestContext : Context
    {
        public TestContext() : base("testContext")
        {
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

        Expression expression = Expression.BuildExpression(testContext, "-5");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "!false");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "1 + 1");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testContextNumericExpression");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());

        expression = Expression.BuildExpression(testContext, "testContextBooleanExpression");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
    }
}
