# Phrase Association Guide

Phrase associations are used along with the element and one or more adjectives to build translatable composite words or noun phrases.

An association can be used to form a noun phrase. I which case it must be defined as follows: `<association word>,<relations>,<form>`

  Here's a description of each part:

  - `<association word>`: an action noun or attributive verb. Refer to language_modding_guide.md for more details on how to define translatable nouns and verbs
  - `<relations>`: one or more prepositions (separated by pipes `|`) that can be used to link the association word with the element
  - `<forms>`: one or more grammatical forms (separated by pipes `|`) the element can take as part of the noun phrase. They can be one of the following:
    - `ns`: proper name singular. Examples: "The Stone", "Mount"
    - `ds`: definite singular. Examples: "the mount", "stone"
    - `is`: indefinite singular. Examples: "a stone", "an arch"
    - `dp`: definite plural. Examples: "the stones", "mounts"
    - `ip`: indefinite plural. Examples: "stones", "archs"
    - `u`: uncountable. Examples: "water, "air"

Example association strings that can be used to form noun phrases:

  - `[nrv]throw:er,of,ip|ns`             (Examples: "thrower of the stone", "thrower of boulders", or "rockthrower")
  - `[iv(bear,ts,past)]born,between,ip`  (Examples: "born between trees" or "waterborn")
  - `[nrv]break:er,of,ip|ns`             (Examples: "breaker of chains", "Breaker of The Stone Wall" or "wallbreaker")
  - `[iv(bear,ts,past)]born,under,ns`    (Examples: "born under the sky", "born under clouds" or "starborn")
  - `[nrv]cut:ter,of,u`                  (Examples: "cutter of grass" or "woodcutter")

Associations can also be used exclusively to form composite words. In such cases, only the `<association word>` part is needs to be defined.

Example association strings that can be used to only form composite words:

  - `[ran]work:er`   (Examples: "stoneworker", "woodworker")
  - `[ran]dance:r`   (Examples: "skydancer", "raindancer")
