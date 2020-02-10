using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;
using System;

public class TestContext : Context
{
    public TestContext() : base("testContext")
    {
    }
}

public class TestBooleanEntityAttribute : BooleanEntityAttribute
{
    private bool _value;

    public TestBooleanEntityAttribute(bool value)
    {
        _value = value;
    }

    public override bool GetValue()
    {
        return _value;
    }
}

public class TestNumericFunctionEntityAttribute : NumericEntityAttribute
{
    private BooleanExpression _argument;

    public TestNumericFunctionEntityAttribute(Expression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException("Number of arguments less than 1");
        }

        _argument = BooleanExpression.ValidateExpression(arguments[0]);
    }

    public override float GetValue()
    {
        return (_argument.GetValue()) ? 10 : 2;
    }
}

public class TestEntity : Entity
{
    private class InternalEntity : Entity
    {
        private TestBooleanEntityAttribute _boolAttribute = new TestBooleanEntityAttribute(true);

        public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
        {
            switch (attributeId)
            {
                case "testBoolAttribute":
                    return _boolAttribute;
            }

            return null;
        }
    }

    private InternalEntity _internalEntity = new InternalEntity();

    private TestBooleanEntityAttribute _boolAttribute = new TestBooleanEntityAttribute(false);

    private FixedEntityEntityAttribute _entityAttribute;

    public TestEntity()
    {
        _entityAttribute = new FixedEntityEntityAttribute(_internalEntity);
    }

    public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
    {
        switch (attributeId)
        {
            case "testBoolAttribute":
                return _boolAttribute;

            case "testEntityAttribute":
                return _entityAttribute;

            case "testNumericFunctionAttribute":
                return new TestNumericFunctionEntityAttribute(arguments);
        }

        return null;
    }
}

public class TestPolity : Polity
{
    public TestPolity(string type, CellGroup coreGroup) : base(type, coreGroup)
    {
    }

    public override float CalculateGroupProminenceExpansionValue(CellGroup sourceGroup, CellGroup targetGroup, float sourceValue)
    {
        throw new NotImplementedException();
    }

    public override void InitializeInternal()
    {
        throw new NotImplementedException();
    }

    protected override void GenerateEventsFromData()
    {
        throw new NotImplementedException();
    }

    protected override void GenerateName()
    {
        throw new NotImplementedException();
    }

    protected override void UpdateInternal()
    {
        throw new NotImplementedException();
    }
}

public class TestFaction : Faction
{
    private float _adminLoad;

    public TestFaction(
        string type, Polity polity, CellGroup coreGroup, float influence, float adminLoad)
        : base(type, polity, coreGroup, influence)
    {
        _adminLoad = adminLoad;
    }

    public override void Split()
    {
        throw new NotImplementedException();
    }

    protected override float CalculateAdministrativeLoad()
    {
        return _adminLoad;
    }

    protected override void GenerateEventsFromData()
    {
        throw new NotImplementedException();
    }

    protected override void GenerateName(Faction parentFaction)
    {
    }

    protected override Agent RequestCurrentLeader()
    {
        throw new NotImplementedException();
    }

    protected override Agent RequestNewLeader()
    {
        throw new NotImplementedException();
    }

    protected override void UpdateInternal()
    {
        throw new NotImplementedException();
    }
}
