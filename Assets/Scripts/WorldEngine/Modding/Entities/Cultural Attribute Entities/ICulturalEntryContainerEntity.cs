using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface ICulturalEntryContainerEntity : IResettableEntity
{
    public Culture Culture { get; }
}
