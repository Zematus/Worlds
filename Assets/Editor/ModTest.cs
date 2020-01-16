using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;

public class ModTest
{
    [Test]
    public void ExpressionParseTest()
    {
        int factCounter = 1;

        Factor factor = Factor.BuildFactor("[INV]([SQ]cell_biome_type_presence:water)");

        Debug.Log("Test factor " + (factCounter++) + ": " + factor.ToString());

        factor = Factor.BuildFactor("[SQ]([INV]cell_biome_type_presence:water)");

        Debug.Log("Test factor " + (factCounter++) + ": " + factor.ToString());

        factor = Factor.BuildFactor("[SQ]([INV](cell_biome_type_presence:water))");

        Debug.Log("Test factor " + (factCounter++) + ": " + factor.ToString());
    }
}
