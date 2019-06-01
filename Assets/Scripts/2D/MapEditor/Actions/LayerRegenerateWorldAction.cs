using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class LayerRegenerateWorldAction : EditorAction
{
    public System.Action<string, float> Action;

    public string LayerId;

    public float PreviousValue;
    public float NewValue;

    public override void Do()
    {
        Action.Invoke(LayerId, NewValue);
    }

    public override void Undo()
    {
        Action.Invoke(LayerId, PreviousValue);
    }
}
