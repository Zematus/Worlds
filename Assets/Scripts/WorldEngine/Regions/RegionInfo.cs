using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class RegionInfo : ISynchronizable, IKeyedValue<long>
{
    [XmlAttribute]
    public long Id;

    [XmlAttribute("LId")]
    public long LanguageId;

    [XmlAttribute("FD")]
    public long EstablishmentDate;

    public Region Region;

    public List<string> AttributeNames = new List<string>();
    public List<string> ElementIds = new List<string>();

    public WorldPosition OriginCellPosition;

    [XmlIgnore]
    public List<RegionAttribute> Attributes = new List<RegionAttribute>();

    [XmlIgnore]
    public List<Element> Elements = new List<Element>();

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public Language Language;

    [XmlIgnore]
    public TerrainCell OriginCell;
    
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

    private Name _name = null;

    private int _rngOffset;

    public RegionInfo()
    {

    }

    public RegionInfo(Region region, TerrainCell originCell, Language language)
    {
        World = originCell.World;

        EstablishmentDate = World.CurrentDate;

        Id = originCell.GenerateUniqueIdentifier(EstablishmentDate);
        Region = region;

        OriginCell = originCell;
        OriginCellPosition = originCell.Position;

        Language = language;
        LanguageId = language.Id;
    }

    public void AddAttribute(RegionAttribute attribute)
    {
        AttributeNames.Add(attribute.Name);
        Attributes.Add(attribute);
    }

    public void AddElement(Element element)
    {
        ElementIds.Add(element.Id);
        Elements.Add(element);
    }

    public virtual void Synchronize()
    {
        if (Region != null)
            Region.Synchronize();
    }

    public virtual void FinalizeLoad()
    {
        OriginCell = World.GetCell(OriginCellPosition);

        Language = World.GetLanguage(LanguageId);

        foreach (string attrName in AttributeNames)
        {
            Attributes.Add(RegionAttribute.Attributes[attrName]);
        }

        foreach (string elemName in ElementIds)
        {
            Elements.Add(Element.Elements[elemName]);
        }

        if (Region != null)
        {
            Region.Info = this;
            Region.FinalizeLoad();
        }
    }

    public string GetRandomAttributeVariation(GetRandomIntDelegate getRandomInt)
    {
        if (Attributes.Count <= 0)
        {
            return string.Empty;
        }

        int index = getRandomInt(Attributes.Count);

        return Attributes[index].GetRandomVariation(getRandomInt);
    }

    protected void AddElements(IEnumerable<Element> elem)
    {
        Elements.AddRange(elem);
    }

    public string GetRandomUnstranslatedAreaName(GetRandomIntDelegate getRandomInt, bool isNounAdjunct)
    {
        string untranslatedName;

        Element element = Elements.RandomSelect(getRandomInt, isNounAdjunct ? 5 : 20);

        List<RegionAttribute> remainingAttributes = new List<RegionAttribute>(Attributes);

        RegionAttribute attribute = remainingAttributes.RandomSelectAndRemove(getRandomInt);

        string[] possibleAdjectives = attribute.Adjectives;

        bool addAttributeNoun = true;

        int wordCount = 0;

        if (element != null)
        {
            possibleAdjectives = element.Adjectives;

            wordCount++;

            if (isNounAdjunct && (getRandomInt(10) > 4))
            {

                addAttributeNoun = false;
            }
        }

        string attributeNoun = string.Empty;

        if (addAttributeNoun)
        {
            attributeNoun = attribute.GetRandomVariation(getRandomInt, element);

            wordCount++;
        }

        int nullAdjectives = 4 * wordCount * (isNounAdjunct ? 4 : 1);

        string adjective = possibleAdjectives.RandomSelect(getRandomInt, nullAdjectives);
        if (!string.IsNullOrEmpty(adjective))
            adjective = "[adj]" + adjective + " ";

        string elementNoun = string.Empty;
        if (element != null)
            elementNoun = "[nad]" + element.SingularName + ((addAttributeNoun) ? " " : string.Empty);

        untranslatedName = adjective + elementNoun;

        if (isNounAdjunct)
        {
            untranslatedName += (addAttributeNoun) ? ("[nad]" + attributeNoun) : string.Empty;
        }
        else
        {
            untranslatedName += attributeNoun;
        }

        return untranslatedName;
    }

    private int GetRandomInt(int maxValue)
    {
        return OriginCell.GetLocalRandomInt(EstablishmentDate, _rngOffset++, maxValue);
    }

    private float GetRandomFloat()
    {
        return OriginCell.GetLocalRandomFloat(EstablishmentDate, _rngOffset++);
    }

    private void GenerateName()
    {
        _rngOffset = RngOffsets.REGION_GENERATE_NAME + unchecked((int)Language.Id);

        string untranslatedName;

        int wordCount = 1;

        List<RegionAttribute> remainingAttributes = new List<RegionAttribute>(Attributes);

        RegionAttribute primaryAttribute = remainingAttributes.RandomSelectAndRemove(GetRandomInt);

        List<Element> remainingElements = new List<Element>(Elements);

        Element firstElement = remainingElements.RandomSelect(GetRandomInt, 5, true);

        IEnumerable<string> possibleAdjectives = primaryAttribute.Adjectives;

        if (firstElement != null)
        {
            possibleAdjectives = firstElement.Adjectives;

            wordCount++;
        }

        string primaryAttributeNoun = primaryAttribute.GetRandomVariation(GetRandomInt, firstElement);

        string secondaryAttributeNoun = string.Empty;

        int elementFactor = (firstElement != null) ? 8 : 4;

        float secondaryAttributeChance = 4f / (elementFactor + possibleAdjectives.Count());

        if ((remainingAttributes.Count > 0) && (GetRandomFloat() < secondaryAttributeChance))
        {
            RegionAttribute secondaryAttribute = remainingAttributes.RandomSelectAndRemove(GetRandomInt);

            if (firstElement == null)
            {
                possibleAdjectives = possibleAdjectives.Union(secondaryAttribute.Adjectives);
            }

            secondaryAttributeNoun = "[nad]" + secondaryAttribute.GetRandomVariation(GetRandomInt, firstElement) + " ";

            wordCount++;
        }

        string adjective = possibleAdjectives.RandomSelect(GetRandomInt, (int)Mathf.Pow(2, wordCount));

        if (!string.IsNullOrEmpty(adjective))
            adjective = "[adj]" + adjective + " ";

        string elementNoun = string.Empty;
        if (firstElement != null)
        {
            elementNoun = "[nad]" + firstElement.SingularName + " ";
        }

        untranslatedName = "[Proper][NP](" + adjective + elementNoun + secondaryAttributeNoun + primaryAttributeNoun + ")";

        _name = new Name(untranslatedName, Language, World);
    }

    public long GetKey()
    {
        return Id;
    }
}
