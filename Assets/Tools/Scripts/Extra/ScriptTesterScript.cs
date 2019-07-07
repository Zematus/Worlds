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
        string input = "[ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3 [OR] ([NOT]cell_has_sea)";

        Condition condition = Condition.BuildCondition(input);

        Debug.Log(condition.ToString());

        input = "group_has_knowledge:neighbor,agriculture_knowledge,3 [OR] ([ANY_N_GROUP]cell_has_sea:0.10 [OR] cell_has_sea)";

        condition = Condition.BuildCondition(input);

        Debug.Log(condition.ToString());

        input = "[ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3 [OR] [ANY_N_GROUP]cell_has_sea:0.10 [OR] ([NOT]cell_has_sea)";

        condition = Condition.BuildCondition(input);

        Debug.Log(condition.ToString());

        input = "([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3 [OR] [ANY_N_GROUP]cell_has_sea:0.10 [OR] ([NOT]cell_has_sea) [OR] [ANY_N_GROUP]cell_has_sea:0.30)";

        condition = Condition.BuildCondition(input);

        Debug.Log(condition.ToString());

        input = "[NOT] ([ANY_N_GROUP]group_has_knowledge:agriculture_knowledge,3 [OR] ([NOT]cell_has_sea) [OR] [ANY_N_GROUP]cell_has_sea:0.30)";

        condition = Condition.BuildCondition(input);

        Debug.Log(condition.ToString());

        ///////////////

        Factor factor = Factor.BuildFactor("[INV]([SQ]neighborhood_sea_presence)");

        Debug.Log(factor.ToString());

        factor = Factor.BuildFactor("[SQ]([INV]neighborhood_sea_presence)");

        Debug.Log(factor.ToString());

        factor = Factor.BuildFactor("[SQ]([INV](neighborhood_sea_presence))");

        Debug.Log(factor.ToString());

        Debug.Break();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
