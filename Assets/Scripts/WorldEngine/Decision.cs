using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

public abstract class Decision {

	public delegate void ExecuteDelegate ();

	public class Option {

		private ExecuteDelegate _executeMethod;

		private bool Preferred;

		private void Execute () {

			_executeMethod ();
		}

		public Option (ExecuteDelegate executeMethod, bool preferred) {

			_executeMethod = executeMethod;
			Preferred = preferred;
		}
	}

	public Decision () {

	}

	public abstract Option[] GetOptions ();

	public abstract void ExecutePreferredOption ();
}
