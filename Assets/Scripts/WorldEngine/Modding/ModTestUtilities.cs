using System;
using System.Collections.Generic;

public class TestContext : Context
{
    public override float GetNextRandomFloat(int iterOffset)
    {
        throw new NotImplementedException();
    }

    public override float GetNextRandomInt(int iterOffset, int maxValue)
    {
        throw new NotImplementedException();
    }
}

public class TestBooleanEntityAttribute : ValueEntityAttribute<bool>
{
    public const string TestId = "testBoolAttribute";

    private readonly bool _value;

    public TestBooleanEntityAttribute(Entity entity, bool value)
        : base(TestId, entity, null)
    {
        _value = value;
    }

    public override bool Value => _value;
}

public class TestNumericFunctionEntityAttribute : ValueEntityAttribute<float>
{
    public const string TestId = "testNumericFunctionAttribute";

    private IValueExpression<bool> _argument;

    public TestNumericFunctionEntityAttribute(Entity entity, IExpression[] arguments)
        : base(TestId, entity, null)
    {
        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException("Number of arguments less than 1");
        }

        _argument = ValueExpressionBuilder.ValidateValueExpression<bool>(arguments[0]);
    }

    public override float Value => (_argument.Value) ? 10 : 2;
}

public class TestEntity : Entity
{
    public const string TestEntityAttributeId = "testEntityAttribute";

    private class InternalEntity : Entity
    {
        public const string TestId = "internalEntity";

        private TestBooleanEntityAttribute _boolAttribute;

        public InternalEntity() : base(TestId)
        {
            _boolAttribute = new TestBooleanEntityAttribute(this, true);
        }

        protected override object _reference => this;

        public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
        {
            switch (attributeId)
            {
                case TestBooleanEntityAttribute.TestId:
                    return _boolAttribute;
            }

            return null;
        }

        public override string GetDebugString()
        {
            throw new NotImplementedException();
        }

        public override string GetFormattedString()
        {
            throw new NotImplementedException();
        }

        public override void Set(object o)
        {
            throw new NotImplementedException();
        }
    }

    private readonly InternalEntity _internalEntity = new InternalEntity();

    private readonly TestBooleanEntityAttribute _boolAttribute;

    private readonly FixedValueEntityAttribute<Entity> _entityAttribute;

    protected override object _reference => this;

    public TestEntity() : base("testEntity")
    {
        _boolAttribute =
            new TestBooleanEntityAttribute(this, false);
        _entityAttribute =
            new FixedValueEntityAttribute<Entity>(_internalEntity, TestEntityAttributeId, this);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case TestBooleanEntityAttribute.TestId:
                return _boolAttribute;

            case TestEntityAttributeId:
                return _entityAttribute;

            case TestNumericFunctionEntityAttribute.TestId:
                return new TestNumericFunctionEntityAttribute(this, arguments);
        }

        return null;
    }

    public override string GetDebugString()
    {
        throw new NotImplementedException();
    }

    public override string GetFormattedString()
    {
        throw new NotImplementedException();
    }

    public override void Set(object o)
    {
        throw new NotImplementedException();
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
    public static int _testCounter = 0;

    private int _testId;

    private float _adminLoad;

    public Agent TestLeader;

    public TestFaction(
        string type,
        Polity polity,
        CellGroup coreGroup,
        float influence = 0,
        Faction parentFaction = null,
        float adminLoad = 0)
        : base(type, polity, coreGroup, influence, parentFaction)
    {
        _testId = _testCounter++;

        _adminLoad = adminLoad;

        Culture = new FactionCulture(this);

        Culture.AddPreference(new CulturalPreference(
            CulturalPreference.AuthorityPreferenceId,
            CulturalPreference.AuthorityPreferenceName,
            CulturalPreference.AuthorityPreferenceRngOffset,
            0));

        Culture.AddPreference(new CulturalPreference(
            CulturalPreference.CohesionPreferenceId,
            CulturalPreference.CohesionPreferenceName,
            CulturalPreference.CohesionPreferenceRngOffset,
            0));

        Culture.AddPreference(new CulturalPreference(
            CulturalPreference.IsolationPreferenceId,
            CulturalPreference.IsolationPreferenceName,
            CulturalPreference.IsolationPreferenceRngOffset,
            0));
    }

    public override string GetName()
    {
        return "test_faction_" + _testId;
    }

    public override string GetNameBold()
    {
        return "<b>test faction " + _testId + "</b>";
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
        return TestLeader;
    }

    protected override Agent RequestNewLeader()
    {
        throw new NotImplementedException();
    }

    protected override void UpdateInternal()
    {
        throw new NotImplementedException();
    }

    public override float GetGroupWeight(CellGroup group)
    {
        return 1;
    }
}

public class TestCellRegion : CellRegion
{
    public TestCellRegion(TerrainCell originCell, Language language) : base(originCell, language)
    {

    }
}
