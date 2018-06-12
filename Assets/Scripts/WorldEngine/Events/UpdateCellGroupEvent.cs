using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class UpdateCellGroupEvent : CellGroupEvent {

	public UpdateCellGroupEvent () {

		DoNotSerialize = true;
	}

	public UpdateCellGroupEvent (CellGroup group, long triggerDate, long? id = null) : base (group, triggerDate, UpdateCellGroupEventId, id) {

		DoNotSerialize = true;
	}

	public override void Trigger () {

		World.AddGroupToUpdate (Group);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Group.UpdateEvent = this;
	}
}
