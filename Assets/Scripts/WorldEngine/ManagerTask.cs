using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using UnityEngine.Profiling;

public delegate T ManagerTaskDelegate<T>();
public delegate void ManagerTaskDelegate();

public interface IManagerTask
{
    void Execute();
}

public class ManagerTask<T> : IManagerTask
{
    public const int SleepTime = 100;

    public bool IsRunning { get; private set; }

    private ManagerTaskDelegate<T> _taskDelegate;

    private T _result;

    public ManagerTask(ManagerTaskDelegate<T> taskDelegate)
    {
        IsRunning = true;

        _taskDelegate = taskDelegate;
    }

    public void Execute()
    {
        _result = _taskDelegate();

        IsRunning = false;
    }

    public void Wait()
    {
        while (IsRunning)
        {
            Thread.Sleep(SleepTime);
        }
    }

    public T Result
    {
        get
        {
            if (IsRunning) Wait();

            return _result;
        }
    }

    public static implicit operator T(ManagerTask<T> task)
    {
        return task.Result;
    }
}

public class ManagerTask : IManagerTask
{
    public const int SleepTime = 100;

    public bool IsRunning { get; private set; }

    private ManagerTaskDelegate _taskDelegate;

    public ManagerTask(ManagerTaskDelegate taskDelegate)
    {
        IsRunning = true;

        _taskDelegate = taskDelegate;
    }

    public void Execute()
    {
        _taskDelegate();

        IsRunning = false;
    }

    public void Wait()
    {
        while (IsRunning)
        {
            Thread.Sleep(SleepTime);
        }
    }
}
