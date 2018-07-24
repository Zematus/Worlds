using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestRunnerScript : MonoBehaviour {

	public List<AutomatedTest> tests = new List<AutomatedTest> ();

	private int _prevTestIndex = -1;
	private int _testIndex = 0;

	private int _successes = 0;

	// Use this for initialization
	void Start () {

        //		Manager.RecordingEnabled = true;

        ////		tests.Add (new SaveLoadTest (407252633, 80, 1, 2, 0, false, true));
        ////		tests.Add (new SaveLoadTest (407252633, 100000, 20000, 5));


#if DEBUG
        Manager.TracingData.GroupId = 141610233072;
        Manager.TracingData.PolityId = 37601724810000;
        Manager.TracingData.FactionId = 11266152902613603;
        Manager.TracingData.Longitude = 248;
        Manager.TracingData.Latitude = 100;
#endif

#if DEBUG
        SaveLoadTest.SaveConditionDelegate saveCondition = (World world) =>
        {
            return (world.PolityMergeCount > 10) && (world.PolityCount > 20);
        };
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 20000000, 10));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2000000, 10, 0));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 200000, 10, 0));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 20000, 10, 0));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2000, 10, 100000));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 200, 10, 102000));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 20, 10, 102800));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2, 10, 102880, true, true));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2, 10, 90110, true, true));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2, 10, 6510, true, true));
        tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2, 10, 5310, true, true));
#endif

        //tests.Add(new SaveLoadTest("after 5 polities", 783909167, (World world) =>
        //{
        //    return world.PolityCount > 5;
        //}, 200000, 5));
        //tests.Add(new SaveLoadTest("after 5 polities", 783909167, (World world) =>
        //{
        //    return world.PolityCount > 5;
        //}, 20000, 10, 0));
        //tests.Add(new SaveLoadTest("after 5 polities", 783909167, (World world) =>
        //{
        //    return world.PolityCount > 5;
        //}, 2000, 10, 0));
        //tests.Add(new SaveLoadTest("after 5 polities", 783909167, (World world) =>
        //{
        //    return world.PolityCount > 5;
        //}, 200, 10, 6000));
        //tests.Add(new SaveLoadTest("after 5 polities", 783909167, (World world) =>
        //{
        //    return world.PolityCount > 5;
        //}, 20, 10, 6600));
        //tests.Add(new SaveLoadTest("after 5 polities", 783909167, (World world) =>
        //{
        //    return world.PolityCount > 5;
        //}, 2, 10, 6620, trackGenRandomCallers: true, enhancedTracing: true));

        //tests.Add(new SaveLoadTest("after 5 polities", 783909167, (World world) =>
        //{
        //    return world.PolityCount > 5;
        //}, 2, 10, 6620, true, true));
        //		tests.Add (new SaveLoadTest ("after 5 polities", 783909167, (World world) => {
        //			return world.PolityCount > 5;
        //		}, 20, 10, 3400, true, true));
        //		tests.Add (new SaveLoadTest ("after 5 polities", 783909167, (World world) => {
        //			return world.PolityCount > 5;
        //		}, 200, 10, 0, true, true));

        //tests.Add (new LanguageGenerationTest());

        Debug.Log ("Running Tests...\n");
	}
	
	// Update is called once per frame
	void Update () {

		Manager.ExecuteTasks (100);

		if (_testIndex == tests.Count) {

			Debug.Log ("\nFinished Tests!");
			Debug.Log (_successes + " of " + tests.Count + " Succeded");
			Debug.Break ();

		} else {

			AutomatedTest test = tests [_testIndex];

			if (_prevTestIndex != _testIndex) {
				
				Debug.Log ("Executing test: " + _testIndex + " - " + test.Name);

				_prevTestIndex = _testIndex;
			}

			test.Run ();

			_successes += (test.State == TestState.Succeded) ? 1 : 0;

			if ((test.State == TestState.Succeded) || (test.State == TestState.Failed))
				_testIndex++;
		}
	}
}
