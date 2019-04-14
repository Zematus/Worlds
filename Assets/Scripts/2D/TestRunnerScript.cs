using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestRunnerScript : MonoBehaviour
{
    public List<AutomatedTest> tests = new List<AutomatedTest>();

    private int _prevTestIndex = -1;
    private int _testIndex = 0;

    private int _successes = 0;

    // Use this for initialization
    void Start()
    {
        //		Manager.RecordingEnabled = true;
        
#if DEBUG
        Manager.TracingData.GroupId = 7102101242056;
        Manager.TracingData.PolityId = 16785053513207705;
        Manager.TracingData.FactionId = 10817008823906100;
        Manager.TracingData.ClusterId = 56906352244149;
        Manager.TracingData.RegionId = 214831393248116;
        Manager.TracingData.Longitude = 248;
        Manager.TracingData.Latitude = 100;
        Manager.TracingData.LastSaveDate = 0; // This value should be overwritten by the test when a save occurs
#endif

#if DEBUG
        SaveLoadTest.SaveConditionDelegate saveCondition = (World world) =>
        {
            return (world.PolityMergeCount > 10) && (world.PolityCount > 20);
        };

        //SaveLoadTestSettings testSettings = new SaveLoadTestSettings {
        //    Seed = 1142453343,
        //    SaveCondition = saveCondition,
        //    HeightmapFilename = @"TestData\Earth_3600x1800.png",
        //    AppSettingsFilename = @"TestData\Earth_3600x1800.settings"
        //};

        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 20000000, 10));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 2000000, 10));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 200000, 10, 6000000));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 20000, 10, 7400000, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 2000, 10, 7460000, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 200, 10, 7476000, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 20, 10, 7476400, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 2, 10, 7476440, true, true, tracingPriority: 0));

        SaveLoadTestSettings testSettings = new SaveLoadTestSettings
        {
            Seed = 783909167,
            SaveCondition = saveCondition,
            AppSettingsFilename = @"TestData\Default.settings"
        };

        tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", testSettings, 20000000, 10));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2000000, 10, 60000000));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 200000, 10, 0, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 20000, 10, 1600000, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2000, 10, 1620000, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2000, 10, 17809, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 200, 10, 1338000, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 200, 10, 1330000, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 20, 10, 1339200, true, true, tracingPriority: 1));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2, 10, 1622100, true, true, tracingPriority: 0));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2, 10, 1613768, true, true, tracingPriority: 0));
        //tests.Add(new SaveLoadTest("after 20 polities and 10 polity merges", 783909167, saveCondition, 2, 10, 13209639, true, true, tracingPriority: 0));
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

        Debug.Log("Running Tests...\n");
    }

    // Update is called once per frame
    void Update()
    {
        Manager.ExecuteTasks(100);

        if (_testIndex == tests.Count)
        {
            Debug.Log("\nFinished Tests!");
            Debug.Log(_successes + " of " + tests.Count + " Succeded");
            Debug.Break();
        }
        else
        {
            AutomatedTest test = tests[_testIndex];

            if (_prevTestIndex != _testIndex)
            {
                Debug.Log("Executing test: " + _testIndex + " - " + test.Name);

                _prevTestIndex = _testIndex;
            }

            test.Run();

            _successes += (test.State == TestState.Succeded) ? 1 : 0;

            if ((test.State == TestState.Succeded) || (test.State == TestState.Failed))
                _testIndex++;
        }
    }

    void OnEnable()
    {
        Application.logMessageReceivedThreaded += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        //Manager.HandleLog(logString, stackTrace, type);

        if (type == LogType.Exception)
        {
            Debug.Break();
        }
    }
}
