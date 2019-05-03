using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TestState
{
    NotStarted,
    Running,
    Failed,
    Succeded
}

public abstract class AutomatedTest
{
    public string Name { get; protected set; }

    public TestState State { get; protected set; }

    public abstract void Run();

    protected AutomatedTest()
    {
        State = TestState.NotStarted;
    }
}
