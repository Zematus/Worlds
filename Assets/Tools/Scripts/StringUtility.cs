using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class StringUtility {

	public static string ReplaceWithWhiteSpace (this string original) {

		string output = "";

		for (int i = 0; i < original.Length; i++) {
		
			output += " ";
		}

		return output;
	}
}
