using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public delegate string ReplaceDelegate ();

public static class StringMachine {

	private const string placeholderPattern = @"<([\w_]+?)>";

	private static Dictionary<string, ReplaceDelegate> _replaceDelegate = new Dictionary<string, ReplaceDelegate>();

	public static string Process (string input) {
		
		return Regex.Replace(input, placeholderPattern, MatchEvaluator);
	}

	private static string MatchEvaluator (Match match) {
		
		string placeholder = match.Groups[1].Value;
		ReplaceDelegate replaceDelegate = null;
		
		if (!_replaceDelegate.TryGetValue(placeholder, out replaceDelegate))
		{
			return match.Groups[0].Value;
		}
		
		return replaceDelegate();
	}

	public static void AddReplaceDelegate (string placeholder, ReplaceDelegate replaceDelegate)
	{
		_replaceDelegate.Add(placeholder, replaceDelegate);
	}
}
