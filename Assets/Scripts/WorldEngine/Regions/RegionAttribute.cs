using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class RegionAttribute
{
    public const string RelationTag = "relation";

    public string Name;

    public string[] Adjectives;

    public Variation[] Variations;

    public Association[] Associations;

    public static RegionAttribute Glacier = new RegionAttribute("Glacier",
        new string[] { "clear", "white", "blue", "grey" },
        new string[] { "glacier{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute IceCap = new RegionAttribute("IceCap",
        new string[] { "clear", "white", "blue", "grey" },
        new string[] { "[nad]ice cap{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Ocean = new RegionAttribute("Ocean",
        new string[] { "clear", "dark", "blue", "red", "green", "grey" },
        new string[] { "ocean{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Grassland = new RegionAttribute("Grassland",
        new string[] { "dark", "pale", "red", "green", "grey", "yellow" },
        new string[] { "grass:land{:s}", "steppe{:s}", "savanna{:s}", "shrub:land{:s}", "prairie{:s}", "range{:s}", "field{:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Forest = new RegionAttribute("Forest",
        new string[] { "black", "dark", "pale", "red", "blue", "grey" },
        new string[] { "forest{<relation>:s}", "wood:s", "wood:land{:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Taiga = new RegionAttribute("Taiga",
        new string[] { "white", "black", "dark", "pale", "red", "blue", "grey" },
        new string[] { "taiga{<relation>:s}", "hinter{:land}{:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Tundra = new RegionAttribute("Tundra",
        new string[] { "white", "black", "dark", "pale", "red", "blue", "grey" },
        new string[] { "tundra{<relation>:s}", "waste{:land}{:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Desert = new RegionAttribute("Desert",
        new string[] { "white", "red", "yellow", "black", "grey" },
        new string[] { "desert{<relation>:s}", "sand{:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Rainforest = new RegionAttribute("Rainforest",
        new string[] { "black", "dark", "red", "blue", "grey" },
        new string[] { "{rain:}forest{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Jungle = new RegionAttribute("Jungle",
        new string[] { "black", "dark", "red", "blue", "grey" },
        new string[] { "jungle{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Valley = new RegionAttribute("Valley",
        new string[] { },
        new string[] { "valley{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Highland = new RegionAttribute("Highland",
        new string[] { },
        new string[] { "high:land{:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    //	public static RegionAttribute MountainRange = new RegionAttribute ("MountainRange", new string[] {"[nad]mountain range", "mountain:s", "mount:s"});
    //	public static RegionAttribute Hill = new RegionAttribute ("Hill", new string[] {"hill{:s}"});
    //	public static RegionAttribute Mountain = new RegionAttribute ("Mountain", new string[] {"mountain", "mount"});
    public static RegionAttribute Basin = new RegionAttribute("Basin",
        new string[] { },
        new string[] { "basin{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    //	public static RegionAttribute Plain = new RegionAttribute ("Plain", new string[] {"plain{:s}"});
    public static RegionAttribute Delta = new RegionAttribute("Delta",
        new string[] { },
        new string[] { "delta{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Peninsula = new RegionAttribute("Peninsula",
        new string[] { },
        new string[] { "cape{<relation>:s}", "horn{<relation>:s}", "peninsula{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Island = new RegionAttribute("Island",
        new string[] { },
        new string[] { "isle{<relation>:s}", "island{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    //	public static RegionAttribute Archipelago = new RegionAttribute ("Archipelago", new string[] {"archipelago", "isle:s", "island:s"});
    //	public static RegionAttribute Channel = new RegionAttribute ("Channel", new string[] {"channel"});
    //	public static RegionAttribute Gulf = new RegionAttribute ("Gulf", new string[] {"gulf"});
    //	public static RegionAttribute Sound = new RegionAttribute ("Sound", new string[] {"sound"});
    //	public static RegionAttribute Lake = new RegionAttribute ("Lake", new string[] {"lake"});
    //	public static RegionAttribute Sea = new RegionAttribute ("Sea", new string[] {"sea"});
    //	public static RegionAttribute Continent = new RegionAttribute ("Continent", new string[] {"continent"});
    //	public static RegionAttribute Strait = new RegionAttribute ("Strait", new string[] {"strait", "pass"});
    public static RegionAttribute Coast = new RegionAttribute("Coast",
        new string[] { },
        new string[] { "strand{<relation>:s}", "coast{<relation>:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    public static RegionAttribute Region = new RegionAttribute("Region",
        new string[] { "dark", "bleak", "open" },
        new string[] { "region{<relation>:s}", "land{:s}" },
        new string[] { "[iv(bear,ts,past)]born,by|near|in,ds|dp", "[rv(ts,past)]raise:d,near|in,ds|dp", "[ran]walk:er,of,ip|ns", "[ran]stride:r,of,ip|ns", "[rv(ts,past)]arrive:d,from,ds" });
    //	public static RegionAttribute Expanse = new RegionAttribute ("Expanse", new string[] {"expanse"});

    public static Dictionary<string, RegionAttribute> Attributes = new Dictionary<string, RegionAttribute>() {
        {Glacier.Name, Glacier},
        {IceCap.Name, IceCap},
        {Ocean.Name, Ocean},
        {Grassland.Name, Grassland},
        {Forest.Name, Forest},
        {Taiga.Name, Taiga},
        {Tundra.Name, Tundra},
        {Desert.Name, Desert},
        {Rainforest.Name, Rainforest},
        {Jungle.Name, Jungle},
        {Valley.Name, Valley},
        {Highland.Name, Highland},
//		{"MountainRange", MountainRange},
//		{"Hill", Hill},
//		{"Mountain", Mountain},
		{Basin.Name, Basin},
//		{"Plain", Plain},
		{Delta.Name, Delta},
        {Peninsula.Name, Peninsula},
        {Island.Name, Island},
//		{"Archipelago", Archipelago},
//		{"Chanel", Channel},
//		{"Gulf", Gulf},
//		{"Sound", Sound},
//		{"Lake", Lake},
//		{"Sea", Sea},
//		{"Continent", Continent},
//		{"Strait", Strait},
		{Coast.Name, Coast},
        {Region.Name, Region}
//		{"Expanse", Expanse}
	};

    private RegionAttribute(string name, string[] adjectives, string[] variants, string[] associationStrs)
    {
        Name = name;

        Adjectives = adjectives;

        Variations = NameTools.GenerateNounVariations(variants);

        List<Association> associations = new List<Association>();

        foreach (string assocStr in associationStrs)
        {
            associations.AddRange(Association.Parse(assocStr));
        }

        Associations = associations.ToArray();
    }

    public string GetRandomVariation(GetRandomIntDelegate getRandomInt, Element filterElement = null, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations;

        if (filterElement != null)
        {
            filteredVariations = Variations.Where(v => !v.Text.Contains(filterElement.SingularName));
        }

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomVariation(GetRandomIntDelegate getRandomInt, string filterStr, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations;

        filterStr = filterStr.ToLower();

        if (filterStr != null)
        {
            filteredVariations = Variations.Where(v => !v.Text.Contains(filterStr));
        }

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomSingularVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations.Where(v => !Language.IsPluralForm(v.Text));

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomPluralVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged = true)
    {
        IEnumerable<Variation> filteredVariations = Variations.Where(v => Language.IsPluralForm(v.Text));

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }

    public string GetRandomVariation(GetRandomIntDelegate getRandomInt, bool filterRelationTagged)
    {
        IEnumerable<Variation> filteredVariations = Variations;

        if (filterRelationTagged)
        {
            filteredVariations = Variations.Where(v => !v.Tags.Contains(RegionAttribute.RelationTag));
        }

        return filteredVariations.RandomSelect(getRandomInt).Text;
    }
}
