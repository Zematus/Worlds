using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class PolityEvent : WorldEvent {

	[XmlAttribute]
	public long PolityId;

	[XmlAttribute]
	public long EventTypeId;

	[XmlIgnore]
	public Polity Polity;

	public PolityEvent () {

	}

	public PolityEvent (Polity polity, long triggerDate, long eventTypeId) : base (polity.World, triggerDate, GenerateUniqueIdentifier (polity, triggerDate, eventTypeId)) {

		Polity = polity;
		PolityId = Polity.Id;

		EventTypeId = eventTypeId;

		//		#if DEBUG
		//		if (Manager.RegisterDebugEvent != null) {
		//			string polityId = "Id: " + polity.Id;
		//
		//			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("PolityEvent - Polity: " + polityId, "TriggerDate: " + TriggerDate);
		//
		//			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
		//		}
		//		#endif
	}

	public static long GenerateUniqueIdentifier (Polity polity, long triggerDate, long eventTypeId) {

		#if DEBUG
		if (triggerDate >= World.MaxSupportedDate) {
			Debug.LogWarning ("'triggerDate' shouldn't be greater than " + World.MaxSupportedDate + " (triggerDate = " + triggerDate + ")");
		}
		#endif

		return (triggerDate * 1000000000L) + ((polity.Id % 1000000L) * 1000L) + eventTypeId;
	}

	public override bool IsStillValid () {

		if (!base.IsStillValid ())
			return false;

		if (Polity == null)
			return false;

		return Polity.StillPresent;
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Polity = World.GetPolity (PolityId);

		if (Polity == null) {

			Debug.LogError ("PolityEvent: Polity with Id:" + PolityId + " not found");
		}
	}

	public virtual void Reset (long newTriggerDate) {

		Reset (newTriggerDate, GenerateUniqueIdentifier (Polity, newTriggerDate, EventTypeId));
	}
}
