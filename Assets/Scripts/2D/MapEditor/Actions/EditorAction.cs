using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class EditorAction
{
    public abstract void Undo();
    public abstract void Do();
}

public class RenegerateWorldAction : EditorAction
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
