using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DelayedSetKnowledgeEntity : KnowledgeEntity
{
    private Culture _culture;
    private string _attributeId;

    private CulturalKnowledge _knowledge = null;
    private bool _isSet = false;

    public DelayedSetKnowledgeEntity(
        Culture culture, string attributeId, Context c, string id)
        : base(c, id)
    {
        _culture = culture;
        _attributeId = attributeId;
    }

    public void Reset()
    {
        _knowledge = null;
        _isSet = false;
    }

    public override CulturalKnowledge Knowledge
    {
        get
        {
            if (!_isSet)
            {
                _knowledge = _culture.GetKnowledge(_attributeId);

                Set(_knowledge);
                _isSet = true;
            }

            return _knowledge;
        }
    }
}
