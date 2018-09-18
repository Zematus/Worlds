using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

// Agent Attributes
// -- Charisma
// -- Wisdom
// -- Cunning
// -- Strength
using UnityEngine.Profiling;

public class Agent : ISynchronizable
{

    public const int MaxAttributeValue = 30;
    public const int MinAttributeValue = 3;

    public const int AttributeGenMax = 18;

    public const long MaxLifespan = 40151; // Prime number to hide birthdate cycle artifacts

    public const int WisdomAgeOffset = 7;
    public const int WisdomAgeFactor = 5 * World.YearLength;

    [XmlAttribute]
    public long Id;

    [XmlAttribute("Birth")]
    public long BirthDate;

    [XmlAttribute("Fem")]
    public bool IsFemale;

    [XmlAttribute("Cha")]
    public int BaseCharisma;

    [XmlAttribute("Wis")]
    public int BaseWisdom;

    [XmlAttribute("GrpId")]
    public long GroupId;

    [XmlAttribute("StilPres")]
    public bool StillPresent = true;

    //[XmlAttribute("NameElem")]
    //public string NameElementId = null;

    //[XmlAttribute("NameAttrName")]
    //public string NameAttributeName = null;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public CellGroup Group;

    [XmlIgnore]
    public Name Name
    {
        get
        {
            //if (_name == null)
            //{
            //    GenerateName();
            //}

            return _name;
        }
    }

    [XmlIgnore]
    public long Age
    {
        get
        {
            return World.CurrentDate - BirthDate;
        }
    }

    [XmlIgnore]
    public int Charisma
    {
        get
        {
            return BaseCharisma;
        }
    }

    [XmlIgnore]
    public int Wisdom
    {
        get
        {
            int wisdom = BaseWisdom + (int)(Age / WisdomAgeFactor) - WisdomAgeOffset;
            //			wisdom = (wisdom > MaxAttributeValue) ? MaxAttributeValue : wisdom;

            return (wisdom > MinAttributeValue) ? wisdom : MinAttributeValue;
        }
    }

    private Name _name = null;

    private int _rngOffset;

    public Agent()
    {

    }

    public Agent(CellGroup birthGroup, long birthDate, long idOffset)
    {
        World = birthGroup.World;

        GroupId = birthGroup.Id;
        Group = birthGroup;

        BirthDate = birthDate;

        idOffset += birthGroup.Id;

        Profiler.BeginSample("new Agent - GenerateUniqueIdentifier");

        Id = GenerateUniqueIdentifier(birthDate, 1000L, idOffset);

        Profiler.EndSample();

        Profiler.BeginSample("new Agent - GenerateBio");

        GenerateBio();

        Profiler.EndSample();

        Profiler.BeginSample("new Agent - PregenerateName");

        PregenerateName();

        Profiler.EndSample();
    }

    public void Destroy()
    {
        StillPresent = false;
    }

    public long GenerateUniqueIdentifier(long date, long oom, long offset)
    {
        return Group.GenerateUniqueIdentifier(date, oom, offset);
    }

    private void GenerateBio()
    {
        int rngOffset = RngOffsets.AGENT_GENERATE_BIO + (int)Id;

        IsFemale = Group.GetLocalRandomFloat(BirthDate, rngOffset++) > 0.5f;
        BaseCharisma = MinAttributeValue + Group.GetLocalRandomInt(BirthDate, rngOffset++, AttributeGenMax);
        BaseWisdom = MinAttributeValue + Group.GetLocalRandomInt(BirthDate, rngOffset++, AttributeGenMax);
    }

    private void GenerateNameFromElement(Element element, GetRandomIntDelegate getRandomInt)
    {
        Language language = Group.Culture.Language;

        string untranslatedName;

        string adjective = element.Adjectives.RandomSelect(getRandomInt, 15);

        if (!string.IsNullOrEmpty(adjective))
        {
            adjective = "[adj]" + adjective + " ";
        }

        Association association = element.Associations.RandomSelect(getRandomInt);

        string nounGender = (IsFemale) ? "[fn]" : "[mn]";

        string subjectNoun = "[name]" + nounGender + association.Noun;

        if (association.IsAdjunction)
        {
            untranslatedName = "[Proper][NP](" + adjective + "[nad]" + element.SingularName + " " + subjectNoun + ")";
        }
        else
        {
            string article = 
                ((association.Form == AssociationForms.DefiniteSingular) ||
                (association.Form == AssociationForms.DefinitePlural) ||
                (association.Form == AssociationForms.NameSingular)) ? 
                "the " : "";

            string uncountableProp = (association.Form == AssociationForms.Uncountable) ? "[un]" : "";

            string elementNoun = element.SingularName;

            if ((association.Form == AssociationForms.DefinitePlural) ||
                (association.Form == AssociationForms.IndefinitePlural))
            {
                elementNoun = element.PluralName;
            }

            elementNoun = article + adjective + uncountableProp + elementNoun;

            untranslatedName = "[PpPP]([Proper][NP](" + subjectNoun + ") [PP](" + association.Relation + " [Proper][NP](" + elementNoun + ")))";
        }

        _name = new Name(untranslatedName, language, World);
    }

    private void GenerateNameFromRegionAttribute(RegionAttribute attribute, GetRandomIntDelegate getRandomInt)
    {
        Language language = Group.Culture.Language;

        string untranslatedName;

        string adjective = attribute.Adjectives.RandomSelect(getRandomInt, 15);

        if (!string.IsNullOrEmpty(adjective))
        {
            adjective = "[adj]" + adjective + " ";
        }

        Association association = attribute.Associations.RandomSelect(getRandomInt);

        string nounGender = (IsFemale) ? "[fn]" : "[mn]";

        string subjectNoun = "[name]" + nounGender + association.Noun;

        if (association.IsAdjunction)
        {
            string variationNoun = attribute.GetRandomSingularVariation(getRandomInt, false);

            untranslatedName = "[Proper][NP](" + adjective + "[nad]" + variationNoun + " " + subjectNoun + ")";
        }
        else
        {
            string article = 
                ((association.Form == AssociationForms.DefiniteSingular) ||
                (association.Form == AssociationForms.DefinitePlural) ||
                (association.Form == AssociationForms.NameSingular)) ? 
                "the " : "";

            string uncountableProp = (association.Form == AssociationForms.Uncountable) ? "[un]" : "";

            string variationNoun;

            if ((association.Form == AssociationForms.DefinitePlural) ||
                (association.Form == AssociationForms.IndefinitePlural))
            {
                variationNoun = attribute.GetRandomPluralVariation(getRandomInt, false);
            }
            else
            {
                variationNoun = attribute.GetRandomSingularVariation(getRandomInt, false);
            }

            variationNoun = article + adjective + uncountableProp + variationNoun;

            untranslatedName = "[PpPP]([Proper][NP](" + subjectNoun + ") [PP](" + association.Relation + " [Proper][NP](" + variationNoun + ")))";
        }

        _name = new Name(untranslatedName, language, World);
    }

    private int GetRandomInt(int maxValue)
    {
        return Group.GetLocalRandomInt(BirthDate, _rngOffset++, maxValue);
    }

    private void PregenerateName()
    {
        _rngOffset = RngOffsets.AGENT_PREGENERATE_NAME + (int)Id;

        Region region = Group.Cell.Region;

        Profiler.BeginSample("region.Elements.Where");

        List<Element> elements = region.Elements;

        Profiler.EndSample();

        Profiler.BeginSample("region.Attributes.Where");

        List<RegionAttribute> attributes = region.Attributes;

        Profiler.EndSample();

        int optionCount = elements.Count + attributes.Count;

        if (optionCount <= 0)
        {
            throw new System.Exception("No elements nor attributes to choose name from");
        }

        Profiler.BeginSample("remainingElements.Count > getRandomInt");

        if (elements.Count > GetRandomInt(optionCount))
        {
            Profiler.BeginSample("RandomSelectAndRemove");

            Element element = elements.RandomSelect(GetRandomInt);

            Profiler.EndSample();

            Profiler.BeginSample("GenerateName - GenerateNameFromElement");

            GenerateNameFromElement(element, GetRandomInt);

            Profiler.EndSample();
        }
        else
        {
            Profiler.BeginSample("RandomSelectAndRemove");

            RegionAttribute attribute = attributes.RandomSelect(GetRandomInt);

            Profiler.EndSample();

            Profiler.BeginSample("GenerateName - GenerateNameFromRegionAttribute");

            GenerateNameFromRegionAttribute(attribute, GetRandomInt);

            Profiler.EndSample();
        }

        Profiler.EndSample();
    }

    //private void GenerateName()
    //{
    //    _rngOffset = RngOffsets.AGENT_GENERATE_NAME + (int)Id;

    //    if (NameElementId != null)
    //    {
    //        Element element = Element.Elements[NameElementId];

    //        Profiler.BeginSample("GenerateName - GenerateNameFromElement");

    //        GenerateNameFromElement(element, GetRandomInt);

    //        Profiler.EndSample();

    //    }
    //    else
    //    {
    //        RegionAttribute attribute = RegionAttribute.Attributes[NameAttributeName];

    //        Profiler.BeginSample("GenerateName - GenerateNameFromRegionAttribute");

    //        GenerateNameFromRegionAttribute(attribute, GetRandomInt);

    //        Profiler.EndSample();
    //    }

    //    //		#if DEBUG
    //    //		Debug.Log ("Leader #" + Id + " name: " + Name);
    //    //		#endif
    //}

    public virtual void Synchronize()
    {
        //Name.Synchronize ();
    }

    public virtual void FinalizeLoad()
    {
        Group = World.GetGroup(GroupId);

        if (Group == null)
        {
            throw new System.Exception("Missing Group with Id " + GroupId);
        }
    }

    public string PossessiveNoun
    {
        get
        {
            return (IsFemale) ? "her" : "his";
        }
    }
}
