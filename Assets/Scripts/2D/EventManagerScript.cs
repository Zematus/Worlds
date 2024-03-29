using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Profiling;

public class EventManagerScript : MonoBehaviour
{
    public UnityEvent GuidedFactionStatusChange;
    public ToggleEvent GuidedFactionSet;

    public void Start()
    {
        Manager.EventManager = this;
    }
}
