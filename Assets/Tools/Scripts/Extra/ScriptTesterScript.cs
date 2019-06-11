using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class ScriptTesterScript : MonoBehaviour
{
    public const string InnerConditionRegex = @"^\s*(?:(?'Open'\()[^\(\)]*)+(?:(?'Inner-Open'\))[^\(\)]*)+(?(Open)(?!))\s*$";

    // This script is meant to be very simple and to be used to test / develop other scripts. 
    // Kind of like a unit test, but not really...

    // Start is called before the first frame update
    void Start()
    {
        string input = " (abc(xyz)def) ";

        Match m = Regex.Match(input, InnerConditionRegex);
        if (m.Success == true)
        {
            Debug.Log("Inner: " + m.Groups["Inner"].Value);

            int capCtr = 0;
            foreach (Capture cap in m.Groups["Inner"].Captures)
            {
                Debug.Log("   Inner Capture " + capCtr + ": " + cap.Value);
                capCtr++;
            }
        }
        else
        {
            Debug.Log("Match failed.");
        }

        Debug.Break();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
