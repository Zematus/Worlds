public enum WordType
{
    Article,
    Indicative,
    Adposition,
    Adjective,
    Noun,
    Verb
}

public enum GeneralArticleProperties
{
    HasDefiniteSingularArticles = 0x001,
    HasDefinitePluralArticles = 0x002,
    HasIndefiniteSingularArticles = 0x004,
    HasIndefinitePluralArticles = 0x008,
    HasUncountableArticles = 0x010
}

public enum GeneralNounIndicativeProperties
{
    HasMasculineIndicative = 0x001,
    HasFemenineIndicative = 0x002,
    HasNeutralIndicative = 0x004,
    HasSingularIndicative = 0x008,
    HasPluralIndicative = 0x010,
    HasDefiniteIndicative = 0x020,
    HasIndefiniteIndicative = 0x040,
    HasUncountableIndicative = 0x080
}

public enum GeneralVerbIndicativeProperties
{
    HasFirstPersonIndicative = 0x0001,
    HasSecondPersonIndicative = 0x0002,
    HasThirdPersonIndicative = 0x0004,

    HasSingularIndicative = 0x0008,
    HasPluralIndicative = 0x0010,

    HasActiveNominalizationIndicative = 0x0020,
    HasPassiveNominalizationIndicative = 0x0040,

    HasPresentTenseIndicative = 0x0080,
    HasPastTenseIndicative = 0x0100,
    HasFutureTenseIndicative = 0x0200,
    HasInfinitiveTenseIndicative = 0x0400
}

public enum AdjunctionProperties
{
    None = 0x00,
    IsAffixed = 0x01,
    GoesAfter = 0x02,
    IsLinkedWithDash = 0x04,

    IsSuffixed = 0x03,
    GoesAfterNounAndLinkedWithDash = 0x06
}

public enum MorphemeProperties
{
    None = 0x0000,

    Plural = 0x0001,
    Uncountable = 0x0002,

    Indefinite = 0x0004,

    Femenine = 0x0008,
    Neutral = 0x0010,

    FirstPerson = 0x0020,
    SecondPerson = 0x0040,
    ThirdPerson = 0x0080,

    Passive = 0x0100
}

public enum PhraseProperties
{
    None = 0x0000,

    Plural = 0x0001,
    Uncountable = 0x0002,

    Indefinite = 0x0004,

    Femenine = 0x0008,
    Neutral = 0x0010,

    FirstPerson = 0x0020,
    SecondPerson = 0x0040,
    ThirdPerson = 0x0080,

    Passive = 0x0100
}
