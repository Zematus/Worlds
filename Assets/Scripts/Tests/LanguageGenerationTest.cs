using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LanguageGenerationTest : AutomatedTest {

	public LanguageGenerationTest () {
	
		Name = "Languange Generation Test";
	}

	public float GetRandomFloat () {
	
		return Random.Range (0, int.MaxValue) / (float)int.MaxValue;
	}

	public override void Run ()
	{
		State = TestState.Running;

		for (int i = 0; i < 10; i++) {
			Language language = new Language (0);

			language.GenerateArticleProperties (GetRandomFloat);

			language.GenerateArticleAdjunctionProperties (GetRandomFloat);
			language.GenerateArticleSyllables (GetRandomFloat);
			language.GenerateAllArticles (GetRandomFloat);

			string entry = "Test Language " + i;
			entry += "\n";

			entry += "\nArticle properties: " + Language.AdjunctionPropertiesToString (language.ArticleAdjunctionProperties);
			entry += "\nArticles:";
			foreach (Language.Morpheme word in language.Articles) {

				entry += "\n    " + word.Meaning + " : " + word.Value;
			}
			entry += "\n";

			language.GenerateNounIndicativeProperties (GetRandomFloat);

			language.GenerateNounIndicativeAdjunctionProperties (GetRandomFloat);
			language.GenerateNounIndicativeSyllables (GetRandomFloat);
			language.GenerateAllNounIndicatives (GetRandomFloat);

			entry += "\nNoun indicative properties: " + Language.AdjunctionPropertiesToString (language.NounIndicativeAdjunctionProperties);
			entry += "\nNoun indicatives:";
			foreach (Language.Morpheme word in language.NounIndicatives) {

				entry += "\n    " + word.Meaning + " : " + word.Value;
			}
			entry += "\n";

			language.GenerateVerbIndicativeProperties (GetRandomFloat);

			language.GenerateVerbIndicativeAdjunctionProperties (GetRandomFloat);
			language.GenerateVerbIndicativeSyllables (GetRandomFloat);
			language.GenerateAllVerbIndicatives (GetRandomFloat);

			entry += "\nVerb indicative properties: " + Language.AdjunctionPropertiesToString (language.VerbIndicativeAdjunctionProperties);
			entry += "\nVerb indicatives:";
			foreach (Language.Morpheme word in language.VerbIndicatives) {

				entry += "\n    " + word.Meaning + " : " + word.Value;
			}
			entry += "\n";

			language.GenerateAdpositionAdjunctionProperties (GetRandomFloat);
			language.GenerateAdpositionSyllables (GetRandomFloat);

//			language.GenerateAdposition ("from", GetRandomFloat);
//			language.GenerateAdposition ("to", GetRandomFloat);
//			language.GenerateAdposition ("within", GetRandomFloat);
//			language.GenerateAdposition ("with", GetRandomFloat);
//			language.GenerateAdposition ("above", GetRandomFloat);
//			language.GenerateAdposition ("below", GetRandomFloat);
//			language.GenerateAdposition ("beyond", GetRandomFloat);
//			language.GenerateAdposition ("on", GetRandomFloat);
//
//			entry += "\nGenerated adpositions:";
//			foreach (Language.Morpheme word in language.Adpositions) {
//
//				entry += "\n    " + word.Meaning + " : " + word.Value;
//			}
//			entry += "\n";

			language.GenerateAdjectiveAdjunctionProperties (GetRandomFloat);
			language.GenerateAdjectiveSyllables (GetRandomFloat);

			language.GenerateNounAdjunctionProperties (GetRandomFloat);
			language.GenerateNounSyllables (GetRandomFloat);

			language.GenerateVerbSyllables (GetRandomFloat);

//			List<Language.Phrase> phrases = new List<Language.Phrase> ();
//
//			phrases.Add (language.TranslateNounPhrase ("the tree", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("a rain:forest", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("the river", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("a town", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("the [adj]white stone:s", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("a mountain", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("the [adj]great desert", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("cloud:s", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("[un]water", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("[un]oil", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("the [mn]man", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("a [fn]woman", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("the [nad]forest person", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("[mn]boy:s", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("the [fn]girl:s", GetRandomFloat));
//			phrases.Add (language.TranslateNounPhrase ("[ipn(child)]children", GetRandomFloat));
//
//			entry += "\nGenerated adjectives:";
//			entry += "\n";
//			foreach (Language.Morpheme word in language.Adjectives) {
//
//				entry += "\n\t" + word.Meaning + " : " + word.Value + " (Properties: " + Language.WordPropertiesToString (word.Properties) + ")";
//				entry += "\n";
//			}
//			entry += "\n";
//
//			entry += "\nGenerated nouns:";
//			entry += "\n";
//			foreach (Language.Morpheme word in language.Nouns) {
//
//				entry += "\n\t" + word.Meaning + " : " + word.Value + " (Properties: " + Language.WordPropertiesToString (word.Properties) + ")";
//				entry += "\n";
//			}
//			entry += "\n";
//
//			entry += "\nExample nouns phrases:";
//			entry += "\n";
//			foreach (Language.NounPhrase phrase in phrases) {
//
//				if ((phrase.Properties & Language.PhraseProperties.Uncountable) == Language.PhraseProperties.Uncountable) {
//					entry += "\n\tSample uncountable noun phrase: " + phrase.Text + " (Meaning: " + phrase.Meaning + ")";
//				} else if ((phrase.Properties & Language.PhraseProperties.Indefinite) == Language.PhraseProperties.Indefinite) {
//					entry += "\n\tSample indefinite noun phrase: " + phrase.Text + " (Meaning: " + phrase.Meaning + ")";
//				} else {
//					entry += "\n\tSample definite noun phrase: " + phrase.Text + " (Meaning: " + phrase.Meaning + ")";
//				}
//
//				entry += "\n";
//			}
//			entry += "\n";

			entry += "\nExample adpositional phrases:";
			entry += "\n";

			string untranslatedPhrase = "[NP](the [ipn(woman)][fn]women) [PP](from [NP](a town))";
			Language.Phrase translatedPhrase = language.TranslatePhrase (untranslatedPhrase, GetRandomFloat);

			Language.Phrase prePhrase = language.TranslateNounPhrase ("the [ipn(woman)][fn]women", GetRandomFloat);
			Language.Phrase complementPhrase = language.TranslateNounPhrase ("a town", GetRandomFloat);
			Language.Phrase postPhrase = language.BuildAdpositionalPhrase ("from", complementPhrase, GetRandomFloat);
			Language.Phrase mergedPhrase = language.MergePhrases (prePhrase, postPhrase);
			language.LocalizePhrase (mergedPhrase);

			entry += "\n\t" + mergedPhrase.Text + " (" + mergedPhrase.Meaning + ")";

//			string untranslatedPhrase = "[NP](the child) [PP](above [NP](the cloud:s))";
//			Language.Phrase translatedPhrase = language.TranslatePhrase (untranslatedPhrase, GetRandomFloat);

			prePhrase = language.TranslateNounPhrase ("the child", GetRandomFloat);
			complementPhrase = language.TranslateNounPhrase ("the cloud:s", GetRandomFloat);
			postPhrase = language.BuildAdpositionalPhrase ("above", complementPhrase, GetRandomFloat);
			mergedPhrase = language.MergePhrases (prePhrase, postPhrase);
			language.LocalizePhrase (mergedPhrase);

			entry += "\n\t" + mergedPhrase.Text + " (" + mergedPhrase.Meaning + ")";

//			string untranslatedPhrase = "[NP](a tree) [PP](within [NP](the forest))";
//			Language.Phrase translatedPhrase = language.TranslatePhrase (untranslatedPhrase, GetRandomFloat);

			prePhrase = language.TranslateNounPhrase ("a tree", GetRandomFloat);
			complementPhrase = language.TranslateNounPhrase ("the forest", GetRandomFloat);
			postPhrase = language.BuildAdpositionalPhrase ("within", complementPhrase, GetRandomFloat);
			mergedPhrase = language.MergePhrases (prePhrase, postPhrase);
			language.LocalizePhrase (mergedPhrase);

			entry += "\n\t" + mergedPhrase.Text + " (" + mergedPhrase.Meaning + ")";

//			string untranslatedPhrase = "[NP]([un]water) [PP](with [NP]([un]oil))";
//			Language.Phrase translatedPhrase = language.TranslatePhrase (untranslatedPhrase, GetRandomFloat);

			prePhrase = language.TranslateNounPhrase ("[un]water", GetRandomFloat);
			complementPhrase = language.TranslateNounPhrase ("[un]oil", GetRandomFloat);
			postPhrase = language.BuildAdpositionalPhrase ("with", complementPhrase, GetRandomFloat);
			mergedPhrase = language.MergePhrases (prePhrase, postPhrase);
			language.LocalizePhrase (mergedPhrase);

			entry += "\n\t" + mergedPhrase.Text + " (" + mergedPhrase.Meaning + ")";

//			string untranslatedPhrase = "[NP]([name][mn][iv(bear,ts,past)]born) [PP](on [NP](a tree))";
//			Language.Phrase translatedPhrase = language.TranslatePhrase (untranslatedPhrase, GetRandomFloat);

			prePhrase = language.TranslateNounPhrase ("[name][mn][iv(bear,ts,past)]born", GetRandomFloat);
			complementPhrase = language.TranslateNounPhrase ("a tree", GetRandomFloat);
			postPhrase = language.BuildAdpositionalPhrase ("on", complementPhrase, GetRandomFloat);
			mergedPhrase = language.MergePhrases (prePhrase, postPhrase);
			language.LocalizePhrase (mergedPhrase);

			entry += "\n\t" + mergedPhrase.Text + " (" + mergedPhrase.Meaning + ")";

//			string untranslatedPhrase = "[NP]([name][fn][rv(ts,past)]raise:d) [PP](in [NP](the city))";
//			Language.Phrase translatedPhrase = language.TranslatePhrase (untranslatedPhrase, GetRandomFloat);

			prePhrase = language.TranslateNounPhrase ("[name][fn][rv(ts,past)]raise:d", GetRandomFloat);
			complementPhrase = language.TranslateNounPhrase ("the city", GetRandomFloat);
			postPhrase = language.BuildAdpositionalPhrase ("in", complementPhrase, GetRandomFloat);
			mergedPhrase = language.MergePhrases (prePhrase, postPhrase);
			language.LocalizePhrase (mergedPhrase);

			entry += "\n\t" + mergedPhrase.Text + " (" + mergedPhrase.Meaning + ")";

//			string untranslatedPhrase = "[NP]([name][mn][nrv]hunt:er) [PP](of [NP](cat:s))";
//			Language.Phrase translatedPhrase = language.TranslatePhrase (untranslatedPhrase, GetRandomFloat);

			prePhrase = language.TranslateNounPhrase ("[name][mn][nrv]hunt:er", GetRandomFloat);
			complementPhrase = language.TranslateNounPhrase ("cat:s", GetRandomFloat);
			postPhrase = language.BuildAdpositionalPhrase ("of", complementPhrase, GetRandomFloat);
			mergedPhrase = language.MergePhrases (prePhrase, postPhrase);
			language.LocalizePhrase (mergedPhrase);

			entry += "\n\t" + mergedPhrase.Text + " (" + mergedPhrase.Meaning + ")";

//			string untranslatedPhrase = "[NP]([name][fn][niv(carry)]carrier) [PP](of [NP]([un]water))";
//			Language.Phrase translatedPhrase = language.TranslatePhrase (untranslatedPhrase, GetRandomFloat);

			prePhrase = language.TranslateNounPhrase ("[name][fn][niv(carry)]carrier", GetRandomFloat);
			complementPhrase = language.TranslateNounPhrase ("[un]water", GetRandomFloat);
			postPhrase = language.BuildAdpositionalPhrase ("of", complementPhrase, GetRandomFloat);
			mergedPhrase = language.MergePhrases (prePhrase, postPhrase);
			language.LocalizePhrase (mergedPhrase);

			entry += "\n\t" + mergedPhrase.Text + " (" + mergedPhrase.Meaning + ")";
			entry += "\n";

			///
			
			entry += "\nGenerated adpositions:";
			foreach (Language.Morpheme word in language.Adpositions) {

				entry += "\n    " + word.Meaning + " : " + word.Value;
			}
			entry += "\n";

			entry += "\nGenerated adjectives:";
			foreach (Language.Morpheme word in language.Adjectives) {

				entry += "\n    " + word.Meaning + " : " + word.Value;
			}
			entry += "\n";

			entry += "\nGenerated verbs:";
			foreach (Language.Morpheme word in language.Verbs) {

				entry += "\n    " + word.Meaning + " : " + word.Value;
			}
			entry += "\n";

			entry += "\nGenerated nouns:";
			foreach (Language.Morpheme word in language.Nouns) {

				entry += "\n    " + word.Meaning + " : " + word.Value;
			}
			entry += "\n";

			///

			Debug.Log (entry);
		}

		State = TestState.Succeded;
	}
}
