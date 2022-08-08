using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModifiableGroupPropertyContainerEntity : ModifiableEntryContainerEntity<CellGroup>
{
    public virtual CellGroup Group
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Group;

    public ModifiableGroupPropertyContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public ModifiableGroupPropertyContainerEntity(
        ValueGetterMethod<CellGroup> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    protected override void AddKey(string key)
    {
        Group.AddPropertyToAquire(key);
        Group.SetToUpdate(warnIfUnexpected: false);
    }

    protected override void RemoveKey(string key)
    {
        Group.AddPropertyToLose(key);
        Group.SetToUpdate(warnIfUnexpected: false);
    }

    protected override bool ContainsKey(string key) => Group.HasOrWillProperty(key);

    public override string GetDebugString() => "properties";

    public override string GetFormattedString() => "<i>properties</i>";
}
