using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

public abstract class Decision {

	public string Description;

	public delegate void ExecuteDelegate ();

	public class Option {

		public string Text;

		private ExecuteDelegate _executeMethod;

		private bool Preferred;

		public void Execute () {

			_executeMethod ();
		}

		public Option (string text, ExecuteDelegate executeMethod, bool preferred) {

			Text = text;
			Preferred = preferred;

			_executeMethod = executeMethod;
		}
	}

	public Decision () {
		
	}

	public abstract Option[] GetOptions ();

	public abstract void ExecutePreferredOption ();
}
