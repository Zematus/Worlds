using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public struct Speed {

	public long Value;
	public string Text;

	public Speed (long value, string text) {

		Value = value;
		Text = text;
	}

	public override string ToString () {

		return Text;
	}

	public static implicit operator long (Speed s) {

		return s.Value;
	}

	public static implicit operator string (Speed s) {

		return s.Text;
	}

	public static bool operator == (Speed a, Speed b) {

		return a.Equals (b);
	}

	public static bool operator != (Speed a, Speed b) {

		return !a.Equals (b);
	}

	public override bool Equals (object obj)
	{
		if (obj is Speed) {

			Speed speed = (Speed)obj;

			return Value.Equals (speed.Value);
		}

		return false;
	}

	public override int GetHashCode ()
	{
		return Value.GetHashCode ();
	}

	public static Speed Zero = new Speed (0, "Paused");
	public static Speed One = new Speed (1, "Max 1 Day / Sec");
	public static Speed Two = new Speed (7, "Max 7 Days / Sec");
	public static Speed Three = new Speed (30, "Max 30 Days / Sec");
	public static Speed Four = new Speed (World.YearLength, "Max 1 Year / Sec");
	public static Speed Five = new Speed (10 * World.YearLength, "Max 10 Years / Sec");
	public static Speed Six = new Speed (100 * World.YearLength, "Max 100 Years / Sec");
	public static Speed Seven = new Speed (1000 * World.YearLength, "Max 1000 Years / Sec");
	public static Speed Max = new Speed (World.MaxPossibleTimeToSkip, "Unlimited");

	public static Speed[] Levels = new Speed[] {
		Speed.One, 
		Speed.Two, 
		Speed.Three, 
		Speed.Four, 
		Speed.Five, 
		Speed.Six, 
		Speed.Seven, 
		Speed.Max
	};
}
