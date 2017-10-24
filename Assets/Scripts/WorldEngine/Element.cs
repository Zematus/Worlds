using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Linq;
using System;

public class ElementConstraint {

	public string Type;
	public object Value;

	public static Regex ConstraintRegex = new Regex (@"^(?<type>[\w_]+):(?<value>.+)$");

	private ElementConstraint (string type, object value) {

		Type = type;
		Value = value;
	}

	public static ElementConstraint BuildConstraint (string constraint) {

		Match match = ConstraintRegex.Match (constraint);

		if (!match.Success)
			throw new System.Exception ("Unparseable constraint: " + constraint);

		string type = match.Groups ["type"].Value;
		string valueStr = match.Groups ["value"].Value;

		switch (type) {

		case "altitude_above":
			float altitude_above = float.Parse (valueStr);

			return new ElementConstraint (type, altitude_above);

		case "altitude_below":
			float altitude_below = float.Parse (valueStr);

			return new ElementConstraint (type, altitude_below);

		case "rainfall_above":
			float rainfall_above = float.Parse (valueStr);

			return new ElementConstraint (type, rainfall_above);

		case "rainfall_below":
			float rainfall_below = float.Parse (valueStr);

			return new ElementConstraint (type, rainfall_below);

		case "temperature_above":
			float temperature_above = float.Parse (valueStr);

			return new ElementConstraint (type, temperature_above);

		case "temperature_below":
			float temperature_below = float.Parse (valueStr);

			return new ElementConstraint (type, temperature_below);

		case "no_attribute":
			string[] attributeStrs = valueStr.Split (new char[] { ',' });

			RegionAttribute[] attributes = attributeStrs.Select (s=> {

				if (!RegionAttribute.Attributes.ContainsKey (s)) {

					throw new System.Exception ("Attribute not present: " + s);
				}

				return RegionAttribute.Attributes[s];
			}).ToArray ();

			return new ElementConstraint (type, attributes);

		case "any_attribute":
			attributeStrs = valueStr.Split (new char[] { ',' });

			attributes = attributeStrs.Select (s=> {

				if (!RegionAttribute.Attributes.ContainsKey (s)) {

					throw new System.Exception ("Attribute not present: " + s);
				}

				return RegionAttribute.Attributes[s];
			}).ToArray ();

			return new ElementConstraint (type, attributes);

		case "any_biome":
			string[] biomeStrs = valueStr.Split (new char[] { ',' });

			Biome[] biomes = biomeStrs.Select (s => {

				if (!Biome.Biomes.ContainsKey (s)) {

					throw new System.Exception ("Biome not present: " + s);
				}

				return Biome.Biomes[s];
			}).ToArray ();

			return new ElementConstraint (type, biomes);

		case "main_biome":
			biomeStrs = valueStr.Split (new char[] { ',' });

			biomes = biomeStrs.Select (s => {

				if (!Biome.Biomes.ContainsKey (s)) {

					throw new System.Exception ("Biome not present: " + s);
				}

				return Biome.Biomes[s];
			}).ToArray ();

			return new ElementConstraint (type, biomes);
		}

		throw new System.Exception ("Unhandled constraint type: " + type);
	}

	public bool Validate (Region region) {

		switch (Type) {

		case "altitude_above":
			return region.AverageAltitude >= (float)Value;

		case "altitude_below":
			return region.AverageAltitude < (float)Value;

		case "rainfall_above":
			return region.AverageRainfall >= (float)Value;

		case "rainfall_below":
			return region.AverageRainfall < (float)Value;

		case "temperature_above":
			return region.AverageTemperature >= (float)Value;

		case "temperature_below":
			return region.AverageTemperature < (float)Value;

		case "no_attribute":
			return !((RegionAttribute[])Value).Any (a => region.Attributes.Contains (a));

		case "any_attribute":
			return ((RegionAttribute[])Value).Any (a => region.Attributes.Contains (a));

		case "any_biome":
			return ((Biome[])Value).Any (a => region.PresentBiomeNames.Contains (a.Name));

		case "main_biome":
			return ((Biome[])Value).Any (a => region.BiomeWithMostPresence == a.Name);
		}

		throw new System.Exception ("Unhandled constraint type: " + Type);
	}
}

public static class AssociationForms {

	public const string NameSingular = "ns";
	public const string DefiniteSingular = "ds";
	public const string IndefiniteSingular = "is";
	public const string DefinitePlural = "dp";
	public const string IndefinitePlural = "ip";
	public const string Uncountable = "u";
}

public class Association {

	public static Regex AssocDefRegex = new Regex (@"^(?<noun>(?:\[[^\[\]]+\])*(?:\w+\:?)+)(?:,(?<relations>(?:\w+\|?)+),(?<forms>(?:\w+\|?)+))?$");

	public string Noun;
	public bool IsAdjunction;

	public string Relation;
	public string Form;

	public Association (string noun) {

		Noun = noun;
		IsAdjunction = true;

		Relation = null;
		Form = null;
	}

	public Association (string noun, string relation, string form) {

		Noun = noun;
		IsAdjunction = false;

		Relation = relation;
		Form = form;
	}
}

public class Element {

	public string SingularName;
	public string PluralName;

	public string[] Adjectives;

	public Association[] Associations;

	public ElementConstraint[] Constraints;

	public static Element Stone = new Element ("stone:s", 
		new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "big", "light", "heavy"}, 
		new string[] {"altitude_above:0"},
		new string[] {"[nrv]throw:er,of,ip|ns","[niv(carry)]carrier,of,p","[iv(bear,ts,past)]born,by|near,ds|dp"});
	public static Element Boulder = new Element ("boulder:s", 
		new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "great"}, 
		new string[] {"altitude_above:0"},
		new string[] {"[nrv]break:er,of,ip|ns","[nrv]roll:er,of,p","[nrv]lift:er,of,ip|ns","[nrv]throw:er,of,ip|ns","[iv(bear,ts,past)]born,by|near,ds|dp"});
	public static Element Rock = new Element ("rock:s", 
		new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "great", "big", "light", "heavy"}, 
		new string[] {"altitude_above:0"},
		new string[] {"[nrv]break:er,of,ip|ns","[nrv]throw:er,of,ip|ns","[nrv]jump:er,of,ip|ns","[iv(bear,ts,past)]born,by|near,ds|dp"});
	public static Element Sand = new Element ("sand:s", 
		new string[] {"white", "red", "yellow", "black", "grey", "fine", "coarse"}, 
		new string[] {"any_attribute:Desert,Delta,Peninsula,Island,Coast"},
		new string[] {"[nrv]throw:er,of,u","[niv(carry)]carrier,of,u","[iv(bear,ts,past)]born,by,ds|dp"});
	public static Element Tree = new Element ("tree:s", 
		new string[] {"white", "red", "green", "yellow", "black", "dark", "pale", "dead", "great", "short", "tall", "narrow", "wide"}, 
		new string[] {"main_biome:Forest,Taiga,Rainforest"},
		new string[] {"[nrv]climb:er,of,ip|ns","[nrv]cut:ter,of,ip|ns","[nrv]fell:er,of,ip|ns","[nrv]fell:er,of,p","[iv(bear,ts,past)]born,under,ip|is|ns","[iv(bear,ts,past)]born,by|near,ds|dp"});
	public static Element Wood = new Element ("wood:s", 
		new string[] {"white", "red", "green", "yellow", "black", "dark", "pale", "dead", "hard", "soft"}, 
		new string[] {"main_biome:Forest,Taiga,Rainforest"},
		new string[] {"[nrv]cut:ter,of,u","[nrv]work:er","[niv(carry)]carrier,of,u"});
	public static Element Grass = new Element ("grass:es", 
		new string[] {"wild", "dead", "tall", "short", "soft", "wet", "dry"}, 
		new string[] {"main_biome:Grassland,Tundra"},
		new string[] {"[nrv]cut:ter,of,u","[nrv]pull:er,of,u","[nrv]eat:er,of,u","[iv(bear,ts,past)]born,in|by|near,ds"});
	public static Element Cloud = new Element ("cloud:s", 
		new string[] {"white", "red", "black", "grey", "dark", "great", "thin", "deep", "light", "bright", "heavy"}, 
		new string[] {"rainfall_above:675"},
		new string[] {"[nrv]observe:r,of,ip","[nrv]watch:er,of,ip","[nrv]gaze:r,of,ip","[iv(bear,ts,past)]born,under,ip"});
	public static Element Moss = new Element ("moss:es", 
		new string[] {"white", "red", "green", "yellow", "black", "dark", "wet"}, 
		new string[] {"main_biome:Forest,Taiga,Tundra,Rainforest"},
		new string[] {"[nrv]eat:er,of,u","[nrv]grow:er,of,u","[iv(bear,ts,past)]born,in|by|near,ds"});
	public static Element Shrub = new Element ("shrub:s", 
		new string[] {"white", "red", "green", "yellow", "black", "dark", "dry"}, 
		new string[] {"main_biome:Grassland,Tundra,Desert"},
		new string[] {"[nrv]burn:er,of,ip","[iv(bear,ts,past)]born,between,ip","[iv(bear,ts,past)]born,by|near,ds|dp","[iv(hide,ts,past)]hidden,between,ip","[iv(hide,ts,past)]hidden,in,is"});
	public static Element Bush = new Element ("bush:es", 
		new string[] {"white", "red", "green", "yellow", "black", "dark", "dry"}, 
		new string[] {"main_biome:Grassland,Tundra,Desert"},
		new string[] {"[nrv]burn:er,of,ip","[iv(bear,ts,past)]born,between,ip","[iv(bear,ts,past)]born,by|near,ds|dp","[iv(hide,ts,past)]hidden,between,ip","[iv(hide,ts,past)]hidden,in,is"});
	public static Element Fire = new Element ("fire:s", 
		new string[] {"wild", "white", "red", "green", "blue", "yellow", "bright"}, 
		new string[] {"altitude_above:0","temperature_above:0"},
		new string[] {"[nrv]start:er,of,ip|ns","[iv(bear,ts,past)]born,under,ns","[iv(bear,ts,past)]born,by|near,ds|dp","[rv(ts,past)]burn:ed,by,u|ds","[nrv]dance:r"});
	public static Element Flame = new Element ("flame:s", 
		new string[] {"wild", "white", "red", "green", "blue", "yellow", "bright"}, 
		new string[] {"altitude_above:0","temperature_above:0"},
		new string[] {"[nrv]eat:er,of,ip","[iv(bear,ts,past)]born,under,dp","[iv(bear,ts,past)]born,by|near,ds|dp","[rv(ts,past)]burn:ed,by,ip|dp","[nrv]dance:r"});
	public static Element Water = new Element ("water:s", 
		new string[] {"white", "green", "blue", "clear", "dark"}, 
		new string[] {"altitude_below:0"},
		new string[] {"[nrv]swim:mer","[nrv]drink:er,of,u|ns","[iv(bear,ts,past)]born,between,ip","[iv(bear,ts,past)]born,by,ds","[rv(ts,past)]soak:ed,by,u","[rv(ts,past)]drench:ed,by,u"});
	public static Element Rain = new Element ("rain:s", 
		new string[] {"heavy", "soft", "dark"}, 
		new string[] {"rainfall_above:675"},
		new string[] {"[iv(bear,ts,past)]born,under,u|ns","[iv(bear,ts,past)]born,between,ip","[rv(ts,past)]soak:ed,by,u","[rv(ts,past)]drench:ed,by,u","[nrv]dance:r"});
	public static Element Storm = new Element ("storm:s", 
		new string[] {"heavy", "dark", "great"}, 
		new string[] {"rainfall_above:675"},
		new string[] {"[iv(bear,ts,past)]born,under,u|ns","[iv(bear,ts,past)]born,between,ip|np","[nrv]dance:r"});
	public static Element Sun = new Element ("sun:s", 
		new string[] {"white", "red", "yellow", "bright", "great"}, 
		new string[] {"rainfall_below:1775"},
		new string[] {"[iv(bear,ts,past)]born,under,ns","[nrv]gaze:r,of,ns","[iv(hide,ts,past)]hidden,from,ns","[nrv]dance:r"});
	public static Element Moon = new Element ("moon:s", 
		new string[] {"white", "red", "blue", "dark", "bright", "great"}, 
		new string[] {"rainfall_below:1775"},
		new string[] {"[iv(bear,ts,past)]born,under,ns","[nrv]gaze:r,of,ns","[iv(hide,ts,past)]hidden,from,ns","[nrv]dance:r"});
	public static Element Day = new Element ("day:s", 
		new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great", "short", "long", "somber", "cheerful"}, 
		new string[] {},
		new string[] {"[iv(bear,ts,past)]born,in|during,ns","[iv(bear,ts,past)]born,between,ip","[nrv]drink:er","[nrv]watch:er","[nrv]talk:er","[nrv]dream:er"});
	public static Element Night = new Element ("night:s", 
		new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great", "short", "long", "somber", "cheerful"}, 
		new string[] {},
		new string[] {"[iv(bear,ts,past)]born,in|during,ns","[iv(bear,ts,past)]born,between,ip","[nrv]drink:er","[nrv]watch:er","[nrv]talk:er"});
	public static Element Air = new Element ("air", 
		new string[] {}, 
		new string[] {},
		new string[] {"[nrv]breath:er,of,u"});
	public static Element Wind = new Element ("wind:s", 
		new string[] {"strong", "soft"}, 
		new string[] {"no_attribute:Rainforest,Jungle,Taiga,Forest"},
		new string[] {"[niv(carry)]carrier,of,u|ns","[iv(bear,ts,past)]born,under,ds|ns"});
	public static Element Sky = new Element ("[ipn(sky)]skies", 
		new string[] {"white", "red", "blue", "green", "yellow", "black", "grey", "dark", "bright", "great"}, 
		new string[] {"altitude_above:3000"},
		new string[] {"[iv(bear,ts,past)]born,under,ns","[nrv]gaze:r,of,ns"});
	public static Element Shadow = new Element ("shadow:s", 
		new string[] {"black", "dark"}, 
		new string[] {"main_biome:Forest,Taiga,Rainforest"},
		new string[] {"[iv(bear,ts,past)]born,under,ip|ns","[nrv]gaze:r,of,ip","[nrv]walk:er","[nrv]stalk:er","[iv(hide,ts,past)]hidden,in,ip","[nrv]dance:r","[rv(ts,past)]scare:d,by,u"});
	public static Element Ice = new Element ("ice:s", 
		new string[] {"clear", "dark", "blue", "opaque"}, 
		new string[] {"temperature_below:0"},
		new string[] {"[niv(carry)]carrier,of,u","[nrv]eat:er,of,u","[nrv]walk:er","[iv(bear,ts,past)]born,in|by|near,ds"});
	public static Element Snow = new Element ("snow:s", 
		new string[] {"clear", "grey", "soft", "hard", "wet"}, 
		new string[] {"temperature_below:5"},
		new string[] {"[nrv]throw:er,of,u","[niv(carry)]carrier,of,u","[nrv]eat:er,of,u","[iv(bear,ts,past)]born,in|by|near,ds","[iv(bear,ts,past)]born,during,ns","[iv(bear,ts,past)]born,between,ip","[iv(hide,ts,past)]hidden,in,ip"});
	public static Element Peat = new Element ("peat:s", 
		new string[] {"red", "green", "yellow", "black", "grey", "dark", "wet", "dry"}, 
		new string[] {"altitude_above:0","temperature_above:0","rainfall_above:675"},
		new string[] {"[niv(carry)]carrier,of,u","[nrv]use:r,of,u","[iv(bear,ts,past)]born,in|by|near,ds"});
	public static Element Thunder = new Element ("thunder:s", 
		new string[] {"great", "loud"}, 
		new string[] {"rainfall_above:675"},
		new string[] {"[iv(bear,ts,past)]born,during,ip|np","[rv(ts,past)]scare:d,by,u"});
	public static Element Lighting = new Element ("lighting:s", 
		new string[] {"white", "yellow", "green", "great"}, 
		new string[] {"rainfall_above:675"},
		new string[] {"[rv(ts,past)]scare:d,by,u","[nrv]chase:r,of,u"});
	public static Element Mud = new Element ("mud:s", 
		new string[] {"red", "green", "yellow", "black", "grey", "dark", "wet", "dry"}, 
		new string[] {"rainfall_above:675"},
		new string[] {"[niv(carry)]carrier,of,u","[nrv]use:r,of,u","[nrv]eat:er,of,u","[iv(bear,ts,past)]born,in|near,ds"});
	public static Element Dew = new Element ("dew:s", 
		new string[] {}, 
		new string[] {"rainfall_above:675"},
		new string[] {});
	public static Element Haze = new Element ("haze:s", 
		new string[] {"light", "white"}, 
		new string[] {"rainfall_above:675"},
		new string[] {"[iv(bear,ts,past)]born,during,sp|np","[iv(hide,ts,past)]hidden,in,ds"});
	public static Element Mist = new Element ("mist:s", 
		new string[] {"light", "grey", "white"}, 
		new string[] {"rainfall_above:675"},
		new string[] {"[iv(bear,ts,past)]born,during,sp|np","[iv(hide,ts,past)]hidden,in,ds"});
	public static Element Fog = new Element ("fog:s", 
		new string[] {"dense", "grey", "dark"}, 
		new string[] {"rainfall_above:1775"},
		new string[] {"[iv(bear,ts,past)]born,during,sp|np","[iv(hide,ts,past)]hidden,in,ds"});
	public static Element Dust = new Element ("dust:s", 
		new string[] {"white", "red", "yellow", "black", "grey", "fine", "coarse"}, 
		new string[] {"rainfall_below:675"},
		new string[] {"[niv(carry)]carrier,of,u","[iv(bear,ts,past)]born,during,sp|np","[iv(bear,ts,past)]born,in|near,ds"});

	public static Dictionary<string, Element> Elements = new Dictionary<string, Element> () {
		{"Stone", Stone},
		{"Boulder", Boulder},
		{"Rock", Rock},
		{"Sand", Sand},
		{"Tree", Tree},
		{"Wood", Wood},
		{"Grass", Grass},
		{"Cloud", Cloud},
		{"Moss", Moss},
		{"Shrub", Shrub},
		{"Fire", Fire},
		{"Flame", Flame},
		{"Water", Water},
		{"Rain", Rain},
		{"Storm", Storm},
		{"Sun", Sun},
		{"Moon", Moon},
		{"Day", Day},
		{"Night", Night},
		{"Air", Air},
		{"Wind", Wind},
		{"Sky", Sky},
		{"Shadow", Shadow},
		{"Ice", Ice},
		{"Snow", Snow},
		{"Peat", Peat},
		{"Thunder", Thunder},
		{"Lighting", Lighting},
		{"Mud", Mud},
		{"Dew", Dew},
		{"Haze", Haze},
		{"Mist", Mist},
		{"Fog", Fog},
		{"Dust", Dust}
	};

	private Association[] ParseAssociations (string associationStr) {

		Match match = Association.AssocDefRegex.Match (associationStr);

		if (!match.Success) {
			throw new System.Exception ("Association string not valid: " + associationStr);
		}

		string noun = match.Groups ["noun"].Value;

		bool isAdjunction = string.IsNullOrEmpty (match.Groups ["relations"].Value);

		if (isAdjunction) {
			return new Association[] { new Association (noun) };
		}

		string[] relations = match.Groups ["relations"].Value.Split('|');
		string[] forms = match.Groups ["forms"].Value.Split('|');

		Association[] associations = new Association[1 + (relations.Length * forms.Length)];

		int index = 0;
		associations [index++] = new Association (noun);

		foreach (string relation in relations) {
			foreach (string form in forms) {
				associations [index++] = new Association (noun, relation, form);
			}
		}

		return associations;
	}

	private Element (string pluralName, string[] adjectives, string[] constraints, string[] associationStrs) {

		SingularName = Language.GetSingularForm (pluralName);
		PluralName = pluralName;

		Adjectives = adjectives;

		Constraints = new ElementConstraint[constraints.Length];

		int index = 0;
		foreach (string constraint in constraints) {
		
			Constraints [index] = ElementConstraint.BuildConstraint (constraint);
			index++;
		}

		List<Association> associations = new List<Association> ();

		foreach (string assocStr in associationStrs) {

			associations.AddRange (ParseAssociations (assocStr));
		}

		Associations = associations.ToArray ();
	}

	public bool Assignable (Region region) {
	
		return Constraints.All (c => c.Validate (region));
	}
}
