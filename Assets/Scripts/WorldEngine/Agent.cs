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

public class Agent : Identifiable
{
    [System.Obsolete] //TODO: attributes now should be values between 0 and 1 instead of max 30
    public const int MaxAttributeValue = 30;
    [System.Obsolete] //TODO: attributes now should be values between 0 and 1 instead of max 30
    public const int MinAttributeValue = 3;

    public const int AttributeGenMax = 18;

    public const long MaxLifespan = 40151; // Prime number to hide birthdate cycle artifacts

    [System.Obsolete] //TODO: attributes now should be values between 0 and 1 instead of max 30
    public const int WisdomAgeOffset = 7;
    [System.Obsolete] //TODO: attributes now should be values between 0 and 1 instead of max 30
    public const int WisdomAgeFactor = 5 * World.YearLength;

    [XmlAttribute("Fem")]
    public bool IsFemale;

    [XmlAttribute("Cha")]
    public int BaseCharisma;

    [XmlAttribute("Wis")]
    public int BaseWisdom;

    [XmlAttribute("StilPres")]
    public bool StillPresent = true;

    [XmlAttribute("LanId")]
    public Identifier LanguageId;

    [XmlAttribute("RenId")]
    public Identifier BirthRegionInfoId;

    public WorldPosition BirthCellPosition;

    public long BirthDate => InitDate;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public Language Language;

    [XmlIgnore]
    public RegionInfo BirthRegionInfo;

    [XmlIgnore]
    public TerrainCell BirthCell;

    [XmlIgnore]
    public Name Name
    {
        get
        {
            if (_name == null)
            {
                GenerateName();
            }

            return _name;
        }
    }

    public long Age => World.CurrentDate - BirthDate;

    public int Charisma => BaseCharisma;

    [XmlIgnore]
    public int Wisdom
    {
        get
        {
            int wisdom = BaseWisdom + (int)(Age / WisdomAgeFactor) - WisdomAgeOffset;

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

        BirthCell = birthGroup.Cell;
        BirthCellPosition = BirthCell.Position;

        BirthRegionInfo = BirthCell.Region.Info;
        BirthRegionInfoId = BirthRegionInfo.Id;

        Language = birthGroup.Culture.Language;
        LanguageId = Language.Id;

        idOffset += birthGroup.GetHashCode();

        long initId = birthGroup.GenerateInitId(idOffset);

        Init(birthDate, initId);

        GenerateBio(birthGroup);
    }

    public void Destroy()
    {
        StillPresent = false;
    }

    private void GenerateBio(CellGroup birthGroup)
    {
        int rngOffset = RngOffsets.AGENT_GENERATE_BIO + unchecked(GetHashCode());

        IsFemale = birthGroup.GetLocalRandomFloat(BirthDate, rngOffset++) > 0.5f;
        BaseCharisma = MinAttributeValue + birthGroup.GetLocalRandomInt(BirthDate, rngOffset++, AttributeGenMax);
        BaseWisdom = MinAttributeValue + birthGroup.GetLocalRandomInt(BirthDate, rngOffset++, AttributeGenMax);
    }

    private void GenerateNameFromElement(Element.Instance element, GetRandomIntDelegate getRandomInt)
    {
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
            untranslatedName = "[Proper][NP](" + adjective + Language.CreateNounAdjunct(element.SingularName) + " " + subjectNoun + ")";
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

        _name = new Name(untranslatedName, Language, World);
    }

    private void GenerateNameFromRegionAttribute(RegionAttribute.Instance attribute, GetRandomIntDelegate getRandomInt)
    {
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

            untranslatedName = "[Proper][NP](" + adjective + Language.CreateNounAdjunct(variationNoun) + " " + subjectNoun + ")";
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

        _name = new Name(untranslatedName, Language, World);
    }

    private int GetRandomInt(int maxValue)
    {
        return BirthCell.GetLocalRandomInt(BirthDate, _rngOffset++, maxValue);
    }

    private void GenerateName()
    {
        _rngOffset = RngOffsets.AGENT_GENERATE_NAME + unchecked(GetHashCode());

        List<Element.Instance> elements = BirthRegionInfo.Elements;

        List<RegionAttribute.Instance> attributes = BirthRegionInfo.AttributeList;

        int optionCount = elements.Count + attributes.Count;

        if (optionCount <= 0)
        {
            throw new System.Exception("No elements nor attributes to choose name from");
        }

        if (elements.Count > GetRandomInt(optionCount))
        {
            Element.Instance element = elements.RandomSelect(GetRandomInt);

            GenerateNameFromElement(element, GetRandomInt);
        }
        else
        {
            RegionAttribute.Instance attribute = attributes.RandomSelect(GetRandomInt);

            GenerateNameFromRegionAttribute(attribute, GetRandomInt);
        }
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        BirthRegionInfo = World.GetRegionInfo(BirthRegionInfoId);

        if (BirthRegionInfo == null)
        {
            throw new System.Exception("Missing RegionInfo with Id " + BirthRegionInfoId);
        }

        Language = World.GetLanguage(LanguageId);

        if (Language == null)
        {
            throw new System.Exception("Missing Language with Id " + LanguageId);
        }

        BirthCell = World.GetCell(BirthCellPosition);

        if (BirthCell == null)
        {
            throw new System.Exception("Missing World Cell at Position " + BirthCellPosition);
        }
    }

    public override void Synchronize()
    {
    }

    public string PossessiveNoun
    {
        get
        {
            return (IsFemale) ? "her" : "his";
        }
    }
}
