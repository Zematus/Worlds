using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class RegenerateWorldAction : EditorAction
{
    public System.Action<float> Action;

    public float PreviousValue;
    public float NewValue;

    public override void Do()
    {
        Action.Invoke(NewValue);
    }

    public override void Undo()
    {
        Action.Invoke(PreviousValue);
    }
}
