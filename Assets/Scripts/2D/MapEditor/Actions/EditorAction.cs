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
