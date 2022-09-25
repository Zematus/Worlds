# Language Structures Modding Guide

Procedurally generated languages in *Worlds* are built by "translating" English words and sentences defined in mod files like *elements.json*, *region_attributes.json* and sometimes within the game itself. In order for this to work, the input strings given in those documents must follow a format that instructs the word parser on how to understand each word or phrase. This document attempts to describe the structure and keywords used to facilitate the parsing process.

### NOUNS

Nouns are single words that are used to identify people, groups of people, locations or other types of objects within the game. These can be used in singular or plural form and be merged together with other nouns or adjectives. They can be built from  verbs or other type of words through processes like verb nominalization.

In most cases, nouns must be defined in 'plural' form. If done correctly, the parser will be able to recognize the singular part of the word. To do so, the noun definition must separate the plural suffix element, `s` or `es`, using a colon `:` like in the following examples:

  `stone:s`, `sun:s`, `moon:s`, `bush:es`, `box:es`, `potato:es`

For irregular nouns, the `in` annotation, encased within brackets, is prefixed. The annotation must include as a parameter the noun's singular spelling. Here are some examples:

  `[in(foot)]feet`, `[in(tooth)]teeth`, `[in(sky)]skies`, `[in(knife)]knives`, `[in(man)]men`, `[in(child)]children`

Certain collective nouns, like `people`, can also be assigned a singular spelling `[in(person)]people`. This should be rarely done as it can lead to translation ambiguities.

#### Agent Nouns

Agent nouns are nouns constructed from words (in this case specifically, verbs) denoting actions. Examples are `runner`, `builder`, `carrier`. *Worlds*' language generator can identify the verb part of agent nouns if properly annotated. In this way, the language builder can make both the verb and the noun derived from it share a root on generated languages. There are two ways to annotate agent nouns:
  - Regular agent nouns (nouns terminating with `er` or `r`) are prefixed within brackets with the annotation `ran` and separate the suffix element with `:`. Examples:

    `[ran]breath:er`, `[ran]throw:er`, `[ran]burn:er`, `[ran]dance:r`, `[ran]swim:mer`, `[ran]cut:ter`

  - Irregular agent nouns (slight noun spelling variation) are prefixed within brackets with the annotation 'ian' with the original verb spelling as a parameter. Examples:

    `[ian(carry)]carrier`, `[ian(translate)]translator`

#### Noun Adjuncts

Noun Adjuncts are single-word nouns that are associated with other nouns much in the same way as adjectives. These must be annotated with the keyword `nad`. Here are some examples:

  `[nad]ice cap`, `[nad]ice sheet`, `[nad]wood table`, `[nad]stone tower`

### VERBS

Though not fully implemented yet, the language generator can produce translations for a limited set of verbal conjugations to construct noun phrases.

To translate a regular verb, the conjugation must be prefixed with the `rv` annotation and two parameters, the grammatical person, and the grammatical tense. The suffix must be separated of the verb using `:`. Here are some examples:

  `[rv(ts,past)]scare:d`, `[rv(ts,past)]soak:ed`

To translate a irregular verb, the conjugation must be prefixed with the `iv` annotation and three parameters, the uninflected verb, the grammatical person, and the grammatical tense. Here are some examples:

  `[iv(bear,ts,past)]born`, `[iv(pin,ts,past)]pinned`, `[iv(cut,ts,past)]cut`

## Notes:
1. There are some cases of regular action nouns or verbs that can't be parsed using the annotations `ran` or `rv`. In those cases it is Ok to use the irregular form annotations `ian` or `iv` respectively.
2. The language generation process is still very limited. Please report any issues to the author (**drtardigrage -at- gmail.com**). Please provide examples of the particular issues found when reporting.
