using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRecorder {

	void Record (string key, string data);
}

public class TestRecorder : IRecorder {

	public Dictionary<string, string> RecordedData = new Dictionary<string, string> ();

	public void Record (string key, string data) {
	
		RecordedData.Add (key, data);
	}

	public string Recover (string key) {
	
		string value = null;

		if (!RecordedData.TryGetValue (key, out value)) {
		
			return null;
		}

		return value;
	}

	public int GetEntryCount () {

		return RecordedData.Count;
	}
}

public class DefaultRecorder : IRecorder {

	public static DefaultRecorder Default = new DefaultRecorder ();

	public void Record (string key, string data) {}
}
