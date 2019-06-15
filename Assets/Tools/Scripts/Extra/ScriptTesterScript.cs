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
        //string input = "(abc(xyz)def)";
        string input = "[NOT] (group_has_knowledge:neighbor,agriculture_knowledge [OR] ([NOT]cell_is_sea:this) [OR] cell_is_sea:neighbor)";

        Condition condition = Condition.BuildCondition(input);

        Debug.Log(condition.ToString());

        Debug.Break();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
