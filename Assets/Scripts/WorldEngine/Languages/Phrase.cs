using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

public class Phrase : ISynchronizable
{
    [XmlAttribute("O")]
    public string Original;
    [XmlAttribute("M")]
    public string Meaning;
    [XmlAttribute("T")]
    public string Text;

    [XmlAttribute("P")]
    public int PropertiesInt;

    [XmlIgnore]
    public PhraseProperties Properties;

    public void Synchronize()
    {
        PropertiesInt = (int)Properties;
    }

    public void FinalizeLoad()
    {
        Properties = (PhraseProperties)PropertiesInt;
    }

    public Phrase()
    {
    }

    public Phrase(Phrase phrase)
    {
        Original = phrase.Original;
        Meaning = phrase.Meaning;
        Text = phrase.Text;

        Properties = phrase.Properties;
    }
}
