using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public struct Speed {

	public int Value;
	public string Text;

	public Speed (int value, string text) {

		Value = value;
		Text = text;
	}

	public override string ToString () {

		return Text;
	}

	public static implicit operator int (Speed s) {

		return s.Value;
	}

	public static implicit operator string (Speed s) {

		return s.Text;
	}

	public static Speed Stopped = new Speed (0, "Stopped");
	public static Speed Slowest = new Speed (1, "Slowest");
	public static Speed Slow = new Speed (10, "Slow");
	public static Speed Normal = new Speed (100, "Normal");
	public static Speed Fast = new Speed (1000, "Fast");
	public static Speed Fastest = new Speed (World.MaxPossibleTimeToSkip, "Fastest");
}

[System.Serializable]
public class MessageEvent : UnityEvent <string> {}

[System.Serializable]
public class SpeedChangeEvent : UnityEvent <Speed> {}

[System.Serializable]
public class ToggleEvent : UnityEvent <bool> {}
