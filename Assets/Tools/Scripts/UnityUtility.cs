using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public static class UnityUtility {
	
	public static void WrapText (TextMesh textMesh, float maxWidth, string insertOnWrap = "") {

		string text = textMesh.text;
		textMesh.text = string.Empty;

		string[] words = text.Split(new char[] {' '});

		AddWrappedText(textMesh, words, maxWidth, insertOnWrap);
	}
	
	private static void AddWrappedText (TextMesh textMesh, string[] words, float maxWidth, string insertOnWrap) {
		
		float currentWidth = 0;
		
		int pointer = words.Length;
		
		bool addText = true;
		
		while (addText)
		{
			addText = false;
			
			StringBuilder newText = new StringBuilder();
			
			int count = 0;
			foreach (string word in words)
			{
				newText.Append(word + " ");
				
				count++;
				
				if (count >= pointer)
				{
					break;
				}
			}
			
			string oldText = textMesh.text;
			textMesh.text += newText.ToString();
			
			currentWidth = textMesh.transform.GetComponent<Renderer>().bounds.size.x;
			
			if (currentWidth > maxWidth)
			{
				addText = true;
				textMesh.text = oldText;
				pointer--;
			}
		}
		
		int wordsLeft = words.Length - pointer;
		
		if (wordsLeft > 0)
		{
			textMesh.text += "\n" + insertOnWrap;

			string[] restOfWords = new string[wordsLeft];
			
			Array.Copy(words, pointer, restOfWords, 0, wordsLeft);

			AddWrappedText(textMesh, restOfWords, maxWidth, insertOnWrap);
		}
	}
}
