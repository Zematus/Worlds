using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;

public class ModTest
{
    [Test]
    public void ExpressionParseTest()
    {
        int expCounter = 1;

        Expression expression = Expression.BuildExpression("1 + 1");

        Debug.Log("Test expression " + (expCounter++) + ": " + expression.ToString());
    }
}
