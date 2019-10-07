using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LanguageGenerationTest : AutomatedTest
{
    public LanguageGenerationTest()
    {
        Name = "Languange Generation Test";
    }

    public override void Run()
    {
        State = TestState.Running;

        for (int i = 0; i < 10; i++)
        {
            Language language = new Language(Random.Range(0, int.MaxValue));

            language.GenerateArticleProperties();

            language.GenerateArticleAdjunctionProperties();
            language.GenerateArticleSyllables();
            language.GenerateAllArticles();

            string entry = "Test Language " + i;
            entry += "\n";

            entry += "\nArticle properties: " + Language.AdjunctionPropertiesToString(language.ArticleAdjunctionProperties);
            entry += "\nArticles:";
            foreach (Morpheme word in language.Articles)
            {
                entry += "\n    " + word.Meaning + " : " + word.Value;
            }
            entry += "\n";

            language.GenerateNounIndicativeProperties();

            language.GenerateNounIndicativeAdjunctionProperties();
            language.GenerateNounIndicativeSyllables();
            language.GenerateAllNounIndicatives();

            entry += "\nNoun indicative properties: " + Language.AdjunctionPropertiesToString(language.NounIndicativeAdjunctionProperties);
            entry += "\nNoun indicatives:";
            foreach (Morpheme word in language.NounIndicatives)
            {
                entry += "\n    " + word.Meaning + " : " + word.Value;
            }
            entry += "\n";

            language.GenerateVerbIndicativeProperties();

            language.GenerateVerbIndicativeAdjunctionProperties();
            language.GenerateVerbIndicativeSyllables();
            language.GenerateAllVerbIndicatives();

            entry += "\nVerb indicative properties: " + Language.AdjunctionPropertiesToString(language.VerbIndicativeAdjunctionProperties);
            entry += "\nVerb indicatives:";
            foreach (Morpheme word in language.VerbIndicatives)
            {
                entry += "\n    " + word.Meaning + " : " + word.Value;
            }
            entry += "\n";

            language.GenerateAdpositionAdjunctionProperties();
            language.GenerateAdpositionSyllables();

            language.GenerateAdjectiveAdjunctionProperties();
            language.GenerateAdjectiveSyllables();

            language.GenerateNounAdjunctionProperties();
            language.GenerateNounSyllables();

            language.GenerateVerbSyllables();

            ///

            entry += "\nExample adpositional phrases:";
            entry += "\n";

            string untranslatedPhrase = "[PpPP]([NP](the [in(woman)][fn]women) [PP](from [NP](a town)))";
            Phrase translatedPhrase = language.TranslatePhrase(untranslatedPhrase);
            language.LocalizePhrase(translatedPhrase);

            entry += "\n\t" + translatedPhrase.Text + " (" + translatedPhrase.Meaning + ")";

            untranslatedPhrase = "[PpPP]([NP](the child) [PP](above [NP](the cloud:s)))";
            translatedPhrase = language.TranslatePhrase(untranslatedPhrase);
            language.LocalizePhrase(translatedPhrase);

            entry += "\n\t" + translatedPhrase.Text + " (" + translatedPhrase.Meaning + ")";

            untranslatedPhrase = "[PpPP]([NP](a tree) [PP](within [NP](the forest)))";
            translatedPhrase = language.TranslatePhrase(untranslatedPhrase);
            language.LocalizePhrase(translatedPhrase);

            entry += "\n\t" + translatedPhrase.Text + " (" + translatedPhrase.Meaning + ")";

            untranslatedPhrase = "[PpPP]([NP]([un]water) [PP](with [NP]([un]oil)))";
            translatedPhrase = language.TranslatePhrase(untranslatedPhrase);
            language.LocalizePhrase(translatedPhrase);

            entry += "\n\t" + translatedPhrase.Text + " (" + translatedPhrase.Meaning + ")";

            untranslatedPhrase = "[PpPP]([NP]([name][mn][iv(bear,ts,past)]born) [PP](on [NP](a tree)))";
            translatedPhrase = language.TranslatePhrase(untranslatedPhrase);
            language.LocalizePhrase(translatedPhrase);

            entry += "\n\t" + translatedPhrase.Text + " (" + translatedPhrase.Meaning + ")";

            untranslatedPhrase = "[PpPP]([NP]([name][fn][rv(ts,past)]raise:d) [PP](in [NP](the city)))";
            translatedPhrase = language.TranslatePhrase(untranslatedPhrase);
            language.LocalizePhrase(translatedPhrase);

            entry += "\n\t" + translatedPhrase.Text + " (" + translatedPhrase.Meaning + ")";

            untranslatedPhrase = "[PpPP]([NP]([name][mn][ran]hunt:er) [PP](of [NP](cat:s)))";
            translatedPhrase = language.TranslatePhrase(untranslatedPhrase);
            language.LocalizePhrase(translatedPhrase);

            entry += "\n\t" + translatedPhrase.Text + " (" + translatedPhrase.Meaning + ")";

            untranslatedPhrase = "[PpPP]([NP]([name][fn][ian(carry)]carrier) [PP](of [NP]([un]water)))";
            translatedPhrase = language.TranslatePhrase(untranslatedPhrase);
            language.LocalizePhrase(translatedPhrase);

            entry += "\n\t" + translatedPhrase.Text + " (" + translatedPhrase.Meaning + ")";
            entry += "\n";

            ///

            entry += "\nGenerated adpositions:";
            foreach (Morpheme word in language.Adpositions)
            {
                entry += "\n    " + word.Meaning + " : " + word.Value;
            }
            entry += "\n";

            entry += "\nGenerated adjectives:";
            foreach (Morpheme word in language.Adjectives)
            {
                entry += "\n    " + word.Meaning + " : " + word.Value;
            }
            entry += "\n";

            entry += "\nGenerated verbs:";
            foreach (Morpheme word in language.Verbs)
            {
                entry += "\n    " + word.Meaning + " : " + word.Value;
            }
            entry += "\n";

            entry += "\nGenerated nouns:";
            foreach (Morpheme word in language.Nouns)
            {
                entry += "\n    " + word.Meaning + " : " + word.Value;
            }
            entry += "\n";

            ///

            Debug.Log(entry);
        }

        State = TestState.Succeded;
    }
}
