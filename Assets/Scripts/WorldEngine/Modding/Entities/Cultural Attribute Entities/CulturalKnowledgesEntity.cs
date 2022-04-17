using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalKnowledgesEntity : CulturalEntityAttributeContainerEntity<CulturalKnowledge>
{
    public CulturalKnowledgesEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CulturalKnowledgesEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override string GetDebugString() => "cultural_knowledges";

    public override string GetFormattedString() => "<i>cultural knowledges</i>";

    protected override bool ContainsKey(string key) => Culture.HasKnowledge(key);

    protected override DelayedSetEntity<CulturalKnowledge> CreateEntity(string attributeId) =>
        new KnowledgeEntity(
            () => Culture.GetKnowledge(attributeId),
            Context,
            BuildAttributeId(attributeId),
            this);

    protected override bool ValidateKey(string attributeId) => Knowledge.Knowledges.ContainsKey(attributeId);
}
