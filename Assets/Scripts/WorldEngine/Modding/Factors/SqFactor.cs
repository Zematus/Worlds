using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SqFactor : Factor
{
    public Factor Factor;

    public SqFactor(string factorStr)
    {
        Factor = BuildFactor(factorStr);
    }

    public override float Calculate(CellGroup group)
    {
        float f = Factor.Calculate(group);

        return f * f;
    }

    public override float Calculate(TerrainCell cell)
    {
        float f = Factor.Calculate(cell);

        return f * f;
    }

    public override string ToString()
    {
        return "SQUARE (" + Factor.ToString() + ")";
    }
}
