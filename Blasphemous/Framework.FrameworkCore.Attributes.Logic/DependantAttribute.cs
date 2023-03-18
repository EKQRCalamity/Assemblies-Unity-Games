using System.Collections.Generic;

namespace Framework.FrameworkCore.Attributes.Logic;

public class DependantAttribute : Attribute
{
	protected List<Attribute> _otherAttributes;

	public DependantAttribute(float baseValue, float upgradeValue, float baseMultiplier)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
		_otherAttributes = new List<Attribute>();
	}

	public void AddAttribute(Attribute attribute)
	{
		_otherAttributes.Add(attribute);
	}

	public void RemoveAttribute(Attribute attribute)
	{
		if (_otherAttributes.Contains(attribute))
		{
			_otherAttributes.Remove(attribute);
		}
	}

	public new virtual float CalculateValue()
	{
		_finalValue = base.Base;
		ApplyRawBonuses();
		ApplyFinalBonuses();
		return _finalValue;
	}
}
