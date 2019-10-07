using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

public class Morpheme : ISynchronizable
{
    [XmlAttribute("M")]
    public string Meaning;
    [XmlAttribute("V")]
    public string Value;

    [XmlAttribute("T")]
    public WordType Type;

    [XmlAttribute("P")]
    public int PropertiesInt;

    [XmlIgnore]
    public MorphemeProperties Properties;

    public Morpheme()
    {
    }

    public void Synchronize()
    {
        PropertiesInt = (int)Properties;
    }

    public void FinalizeLoad()
    {
        Properties = (MorphemeProperties)PropertiesInt;
    }
}
