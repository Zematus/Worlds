using System.Collections.Generic;
using System.Linq;

public class Element
{
    public string Id;

    public string SingularName;
    public string PluralName;

    public string[] Adjectives;

    public Association[] Associations;

    public ElementConstraint[] Constraints;

    public static Element Stone = new Element("Stone", "stone:s",
        new string[] { "white", "red", "blue", "green", "yellow", "black", "grey", "big", "light", "heavy" },
        new string[] { "altitude_above:0" },
        new string[] { "[ran]throw:er,of,ip|ns", "[ian(carry)]carrier,of,p", "[iv(bear,ts,past)]born,by|near,ds|dp" });
    public static Element Boulder = new Element("Boulder", "boulder:s",
        new string[] { "white", "red", "blue", "green", "yellow", "black", "grey", "great" },
        new string[] { "altitude_above:0" },
        new string[] { "[ran]break:er,of,ip|ns", "[ran]roll:er,of,p", "[ran]lift:er,of,ip|ns", "[ran]throw:er,of,ip|ns", "[iv(bear,ts,past)]born,by|near,ds|dp" });
    public static Element Rock = new Element("Rock", "rock:s",
        new string[] { "white", "red", "blue", "green", "yellow", "black", "grey", "great", "big", "light", "heavy" },
        new string[] { "altitude_above:0" },
        new string[] { "[ran]break:er,of,ip|ns", "[ran]throw:er,of,ip|ns", "[ran]jump:er,of,ip|ns", "[iv(bear,ts,past)]born,by|near,ds|dp" });
    public static Element Sand = new Element("Sand", "sand:s",
        new string[] { "white", "red", "yellow", "black", "grey", "fine", "coarse" },
        new string[] { "any_attribute:Desert,Delta,Peninsula,Island,Coast" },
        new string[] { "[ran]throw:er,of,u", "[ian(carry)]carrier,of,u", "[iv(bear,ts,past)]born,by,ds|dp" });
    public static Element Tree = new Element("Tree", "tree:s",
        new string[] { "white", "red", "green", "yellow", "black", "dark", "pale", "dead", "great", "short", "tall", "narrow", "wide" },
        new string[] { "main_biome:Forest,Taiga,Rainforest" },
        new string[] { "[ran]climb:er,of,ip|ns", "[ran]cut:ter,of,ip|ns", "[ran]fell:er,of,ip|ns", "[ran]fell:er,of,p", "[iv(bear,ts,past)]born,under,ip|is|ns", "[iv(bear,ts,past)]born,by|near,ds|dp" });
    public static Element Wood = new Element("Wood", "wood:s",
        new string[] { "white", "red", "green", "yellow", "black", "dark", "pale", "dead", "hard", "soft" },
        new string[] { "main_biome:Forest,Taiga,Rainforest" },
        new string[] { "[ran]cut:ter,of,u", "[ran]work:er", "[ian(carry)]carrier,of,u" });
    public static Element Grass = new Element("Grass", "grass:es",
        new string[] { "wild", "dead", "tall", "short", "soft", "wet", "dry" },
        new string[] { "main_biome:Grassland,Tundra" },
        new string[] { "[ran]cut:ter,of,u", "[ran]pull:er,of,u", "[ran]eat:er,of,u", "[iv(bear,ts,past)]born,in|by|near,ds" });
    public static Element Cloud = new Element("Cloud", "cloud:s",
        new string[] { "white", "red", "black", "grey", "dark", "great", "thin", "deep", "light", "bright", "heavy" },
        new string[] { "rainfall_above:675" },
        new string[] { "[ran]observe:r,of,ip", "[ran]watch:er,of,ip", "[ran]gaze:r,of,ip", "[iv(bear,ts,past)]born,under,ip" });
    public static Element Moss = new Element("Moss", "moss:es",
        new string[] { "white", "red", "green", "yellow", "black", "dark", "wet" },
        new string[] { "main_biome:Forest,Taiga,Tundra,Rainforest" },
        new string[] { "[ran]eat:er,of,u", "[ran]grow:er,of,u", "[iv(bear,ts,past)]born,in|by|near,ds" });
    public static Element Shrub = new Element("Shrub", "shrub:s",
        new string[] { "white", "red", "green", "yellow", "black", "dark", "dry" },
        new string[] { "main_biome:Grassland,Tundra,Desert" },
        new string[] { "[ran]burn:er,of,ip", "[iv(bear,ts,past)]born,between,ip", "[iv(bear,ts,past)]born,by|near,ds|dp", "[iv(hide,ts,past)]hidden,between,ip", "[iv(hide,ts,past)]hidden,in,is" });
    public static Element Bush = new Element("Bush", "bush:es",
        new string[] { "white", "red", "green", "yellow", "black", "dark", "dry" },
        new string[] { "main_biome:Grassland,Tundra,Desert" },
        new string[] { "[ran]burn:er,of,ip", "[iv(bear,ts,past)]born,between,ip", "[iv(bear,ts,past)]born,by|near,ds|dp", "[iv(hide,ts,past)]hidden,between,ip", "[iv(hide,ts,past)]hidden,in,is" });
    public static Element Fire = new Element("Fire", "fire:s",
        new string[] { "wild", "white", "red", "green", "blue", "yellow", "bright" },
        new string[] { "altitude_above:0", "temperature_above:0" },
        new string[] { "[ran]start:er,of,ip|ns", "[iv(bear,ts,past)]born,under,ns", "[iv(bear,ts,past)]born,by|near,ds|dp", "[rv(ts,past)]burn:ed,by,u|ds", "[ran]dance:r" });
    public static Element Flame = new Element("Flame", "flame:s",
        new string[] { "wild", "white", "red", "green", "blue", "yellow", "bright" },
        new string[] { "altitude_above:0", "temperature_above:0" },
        new string[] { "[ran]eat:er,of,ip", "[iv(bear,ts,past)]born,under,dp", "[iv(bear,ts,past)]born,by|near,ds|dp", "[rv(ts,past)]burn:ed,by,ip|dp", "[ran]dance:r" });
    public static Element Water = new Element("Water", "water:s",
        new string[] { "white", "green", "blue", "clear", "dark" },
        new string[] { "altitude_below:0" },
        new string[] { "[ran]swim:mer", "[ran]drink:er,of,u|ns", "[iv(bear,ts,past)]born,between,ip", "[iv(bear,ts,past)]born,by,ds", "[rv(ts,past)]soak:ed,by,u", "[rv(ts,past)]drench:ed,by,u" });
    public static Element Rain = new Element("Rain", "rain:s",
        new string[] { "heavy", "soft", "dark" },
        new string[] { "rainfall_above:675" },
        new string[] { "[iv(bear,ts,past)]born,under,u|ns", "[iv(bear,ts,past)]born,between,ip", "[rv(ts,past)]bath:ed,by,u", "[rv(ts,past)]soak:ed,by,u", "[rv(ts,past)]drench:ed,by,u", "[ran]dance:r" });
    public static Element Storm = new Element("Storm", "storm:s",
        new string[] { "heavy", "dark", "great" },
        new string[] { "rainfall_above:675" },
        new string[] { "[iv(bear,ts,past)]born,under,u|ns", "[iv(bear,ts,past)]born,between,ip|np", "[ran]dance:r" });
    public static Element Sun = new Element("Sun", "sun:s",
        new string[] { "white", "red", "yellow", "bright", "great" },
        new string[] { "rainfall_below:1775" },
        new string[] { "[iv(bear,ts,past)]born,under,ns", "[ran]gaze:r,of,ns", "[iv(hide,ts,past)]hidden,from,ns", "[ran]dance:r" });
    public static Element Moon = new Element("Moon", "moon:s",
        new string[] { "white", "red", "blue", "dark", "bright", "great" },
        new string[] { "rainfall_below:1775" },
        new string[] { "[iv(bear,ts,past)]born,under,ns", "[ran]gaze:r,of,ns", "[iv(hide,ts,past)]hidden,from,ns", "[ran]dance:r" });
    public static Element Day = new Element("Day", "day:s",
        new string[] { "white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great", "short", "long", "somber", "cheerful" },
        new string[] { },
        new string[] { "[iv(bear,ts,past)]born,in|during,ns", "[iv(bear,ts,past)]born,between,ip", "[ran]drink:er", "[ran]watch:er", "[ran]talk:er", "[ran]dream:er" });
    public static Element Night = new Element("Night", "night:s",
        new string[] { "white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great", "short", "long", "somber", "cheerful" },
        new string[] { },
        new string[] { "[iv(bear,ts,past)]born,in|during,ns", "[iv(bear,ts,past)]born,between,ip", "[ran]drink:er", "[ran]watch:er", "[ran]talk:er" });
    public static Element Air = new Element("Air", "air",
        new string[] { },
        new string[] { },
        new string[] { "[ran]breath:er,of,u" });
    public static Element Wind = new Element("Wind", "wind:s",
        new string[] { "strong", "soft" },
        new string[] { "no_attribute:Rainforest,Jungle,Taiga,Forest" },
        new string[] { "[ian(carry)]carrier,of,u|ns", "[iv(bear,ts,past)]born,under,ds|ns" });
    public static Element Sky = new Element("Sky", "[in(sky)]skies",
        new string[] { "white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great" },
        new string[] { "altitude_above:3000" },
        new string[] { "[iv(bear,ts,past)]born,under,ns", "[ran]gaze:r,of,ns" });
    public static Element Shadow = new Element("Shadow", "shadow:s",
        new string[] { "black", "dark" },
        new string[] { "main_biome:Forest,Taiga,Rainforest" },
        new string[] { "[iv(bear,ts,past)]born,under,ip|ns", "[ran]gaze:r,of,ip", "[ran]walk:er", "[ran]stalk:er", "[iv(hide,ts,past)]hidden,in,ip", "[ran]dance:r", "[rv(ts,past)]scare:d,by,u" });
    public static Element Ice = new Element("Ice", "ice:s",
        new string[] { "clear", "dark", "blue", "opaque" },
        new string[] { "temperature_below:0" },
        new string[] { "[ian(carry)]carrier,of,u", "[ran]eat:er,of,u", "[ran]walk:er", "[iv(bear,ts,past)]born,in|by|near,ds" });
    public static Element Snow = new Element("Snow", "snow:s",
        new string[] { "clear", "grey", "soft", "hard", "wet" },
        new string[] { "temperature_below:5" },
        new string[] { "[ran]throw:er,of,u", "[ian(carry)]carrier,of,u", "[ran]eat:er,of,u", "[iv(bear,ts,past)]born,in|by|near,ds", "[iv(bear,ts,past)]born,during,ns", "[iv(bear,ts,past)]born,between,ip", "[iv(hide,ts,past)]hidden,in,ip" });
    public static Element Peat = new Element("Peat", "peat:s",
        new string[] { "red", "green", "yellow", "black", "grey", "dark", "wet", "dry" },
        new string[] { "altitude_above:0", "temperature_above:0", "rainfall_above:675" },
        new string[] { "[ian(carry)]carrier,of,u", "[ran]use:r,of,u", "[iv(bear,ts,past)]born,in|by|near,ds" });
    public static Element Thunder = new Element("Thunder", "thunder:s",
        new string[] { "great", "loud" },
        new string[] { "rainfall_above:675" },
        new string[] { "[iv(bear,ts,past)]born,during,ip|np", "[rv(ts,past)]scare:d,by,u" });
    public static Element Lighting = new Element("Lighting", "lighting:s",
        new string[] { "white", "yellow", "green", "great" },
        new string[] { "rainfall_above:675" },
        new string[] { "[rv(ts,past)]scare:d,by,u", "[ran]chase:r,of,u" });
    public static Element Mud = new Element("Mud", "mud:s",
        new string[] { "red", "green", "yellow", "black", "grey", "dark", "wet", "dry" },
        new string[] { "rainfall_above:675" },
        new string[] { "[ian(carry)]carrier,of,u", "[ran]use:r,of,u", "[ran]eat:er,of,u", "[iv(bear,ts,past)]born,in|near,ds" });
    public static Element Dew = new Element("Dew", "dew:s",
        new string[] { },
        new string[] { "rainfall_above:675" },
        new string[] { "[rv(ts,past)]bath:ed,by,u", "[rv(ts,past)]soak:ed,by,u" });
    public static Element Haze = new Element("Haze", "haze:s",
        new string[] { "light", "white" },
        new string[] { "rainfall_above:675" },
        new string[] { "[iv(bear,ts,past)]born,during,sp|np", "[iv(hide,ts,past)]hidden,in,ds" });
    public static Element Mist = new Element("Mist", "mist:s",
        new string[] { "light", "grey", "white" },
        new string[] { "rainfall_above:675" },
        new string[] { "[iv(bear,ts,past)]born,during,sp|np", "[iv(hide,ts,past)]hidden,in,ds" });
    public static Element Fog = new Element("Fog", "fog:s",
        new string[] { "dense", "grey", "dark" },
        new string[] { "rainfall_above:1775" },
        new string[] { "[iv(bear,ts,past)]born,during,sp|np", "[iv(hide,ts,past)]hidden,in,ds" });
    public static Element Dust = new Element("Dust", "dust:s",
        new string[] { "white", "red", "yellow", "black", "grey", "fine", "coarse" },
        new string[] { "rainfall_below:675" },
        new string[] { "[ian(carry)]carrier,of,u", "[iv(bear,ts,past)]born,during,sp|np", "[iv(bear,ts,past)]born,in|near,ds" });

    public static Dictionary<string, Element> Elements = new Dictionary<string, Element>() {
        {Stone.Id, Stone},
        {Boulder.Id, Boulder},
        {Rock.Id, Rock},
        {Sand.Id, Sand},
        {Tree.Id, Tree},
        {Wood.Id, Wood},
        {Grass.Id, Grass},
        {Cloud.Id, Cloud},
        {Moss.Id, Moss},
        {Shrub.Id, Shrub},
        {Fire.Id, Fire},
        {Flame.Id, Flame},
        {Water.Id, Water},
        {Rain.Id, Rain},
        {Storm.Id, Storm},
        {Sun.Id, Sun},
        {Moon.Id, Moon},
        {Day.Id, Day},
        {Night.Id, Night},
        {Air.Id, Air},
        {Wind.Id, Wind},
        {Sky.Id, Sky},
        {Shadow.Id, Shadow},
        {Ice.Id, Ice},
        {Snow.Id, Snow},
        {Peat.Id, Peat},
        {Thunder.Id, Thunder},
        {Lighting.Id, Lighting},
        {Mud.Id, Mud},
        {Dew.Id, Dew},
        {Haze.Id, Haze},
        {Mist.Id, Mist},
        {Fog.Id, Fog},
        {Dust.Id, Dust}
    };

    public Element(string id, string pluralName, string[] adjectives, string[] constraints, string[] associationStrs)
    {
        Id = id;

        SingularName = Language.GetSingularForm(pluralName);
        PluralName = pluralName;

        Adjectives = adjectives;

        Constraints = new ElementConstraint[constraints.Length];

        int index = 0;
        foreach (string constraint in constraints)
        {
            Constraints[index] = ElementConstraint.BuildConstraint(constraint);
            index++;
        }

        List<Association> associations = new List<Association>();

        foreach (string assocStr in associationStrs)
        {
            associations.AddRange(Association.Parse(assocStr));
        }

        Associations = associations.ToArray();
    }

    public bool Assignable(Region region)
    {
        foreach (ElementConstraint c in Constraints)
        {
            if (!c.Validate(region)) return false;
        }

        return true;
    }
}
