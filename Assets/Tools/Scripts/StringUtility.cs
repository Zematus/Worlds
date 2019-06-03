using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class StringUtility
{
    public static string ReplaceWithWhiteSpace(this string original)
    {
        string output = "";

        for (int i = 0; i < original.Length; i++)
        {
            output += " ";
        }

        return output;
    }

    public static string ToBoldFormat(this string text)
    {
        return "<b>" + text + "</b>";
    }

    public static string FirstLetterToUpper(this string text)
    {
        if (text.Length <= 1)
            return text.ToUpper();

        return text[0].ToString().ToUpper() + text.Substring(1);
    }

    public static string FirstLetterToLower(this string text)
    {
        if (text.Length <= 1)
            return text.ToLower();

        return text[0].ToString().ToLower() + text.Substring(1);
    }

    public static string AllFirstLettersToUpper(this string text)
    {
        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = words[i].FirstLetterToUpper();
        }
        
        return string.Join(" ", words);
    }

    public static string AllFirstLettersToLower(this string text)
    {
        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = words[i].FirstLetterToLower();
        }

        return string.Join(" ", words);
    }

    public static string AddPossApos(this string text)
    {
        char lastChar = text.ToLower()[text.Length - 1];

        if ((lastChar == 's') || (lastChar == 'z'))
            return text + "'";

        return text + "'s";
    }
}
