using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class ScriptTesterScript : MonoBehaviour
{
    // This script is meant to be very simple and to be used to test / develop other scripts. 
    // Kind of like a unit test, but not really...

    // Start is called before the first frame update
    void Start()
    {
        string input = "[INV][SQ]neighborhood_sea_presence";

        Match match = Regex.Match(input, ModUtility.MixedStatementRegex);
        if (match.Success == true)
        {
            Debug.Log(match.Groups["unaryOp"].Value + "::" + match.Groups["ops"].Value + match.Groups["statement"].Value);
        }
        else
        {
            Debug.Log("Couldn't match: " + input);
        }

        input = "[INV][SQ](neighborhood_sea_presence)";

        match = Regex.Match(input, ModUtility.MixedStatementRegex);
        if (match.Success == true)
        {
            Debug.Log(match.Groups["unaryOp"].Value + "::" + match.Groups["ops"].Value + match.Groups["statement"].Value);
        }
        else
        {
            Debug.Log("Couldn't match: " + input);
        }

        input = "[INV]([SQ]neighborhood_sea_presence)";

        match = Regex.Match(input, ModUtility.MixedStatementRegex);
        if (match.Success == true)
        {
            Debug.Log(match.Groups["unaryOp"].Value + "::" + match.Groups["ops"].Value + match.Groups["statement"].Value);
        }
        else
        {
            Debug.Log("Couldn't match: " + input);
        }

        input = "([INV][SQ]neighborhood_sea_presence)";

        match = Regex.Match(input, ModUtility.MixedStatementRegex);
        if (match.Success == true)
        {
            Debug.Log(match.Groups["unaryOp"].Value + "::" + match.Groups["ops"].Value + match.Groups["statement"].Value);
        }
        else
        {
            Debug.Log("Couldn't match: " + input);
        }

        Debug.Break();

        input = "[NOT] (group_has_knowledge:neighbor,agriculture_knowledge,3 [OR] ([NOT]cell_has_sea:this) [OR] cell_has_sea:neighbor,0.30)";

        Condition condition = Condition.BuildCondition(input);

        Debug.Log(condition.ToString());
        
        Factor factor = Factor.BuildFactor("[INV][SQ]neighborhood_sea_presence");

        Debug.Log(factor.ToString());

        factor = Factor.BuildFactor("[INV]([SQ]neighborhood_sea_presence)");

        Debug.Log(factor.ToString());

        factor = Factor.BuildFactor("[SQ]([INV]neighborhood_sea_presence)");

        Debug.Log(factor.ToString());

        factor = Factor.BuildFactor("[SQ][INV](neighborhood_sea_presence)");

        Debug.Log(factor.ToString());

        Debug.Break();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
