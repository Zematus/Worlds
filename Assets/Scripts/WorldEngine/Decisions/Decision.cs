using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

public delegate void DecisionEffectDelegate ();

public abstract class Decision {

	public string Description;

	public delegate void ExecuteDelegate ();

	public class Option {

		public string Text;

		public string DescriptionText;

		private ExecuteDelegate _executeMethod;

		public void Execute () {

			_executeMethod ();
		}

		public Option (string text, string descriptionText, ExecuteDelegate executeMethod) {

			Text = text;

			DescriptionText = descriptionText;

			_executeMethod = executeMethod;
		}
	}

	public Decision () {
		
	}

	public abstract Option[] GetOptions ();

	public abstract void ExecutePreferredOption ();
}
