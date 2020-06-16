using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class InvFactor : Factor
{
    public Factor Factor;

    public InvFactor(string factorStr)
    {
        Factor = BuildFactor(factorStr);
    }

    public override float Calculate(CellGroup group)
    {
        return 1 - Factor.Calculate(group);
    }

    public override float Calculate(TerrainCell cell)
    {
        return 1 - Factor.Calculate(cell);
    }

    public override string ToString()
    {
        return "INVERSE (" + Factor.ToString() + ")";
    }
}
