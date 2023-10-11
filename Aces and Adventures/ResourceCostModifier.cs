using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(Alter))]
[ProtoInclude(11, typeof(Reduce))]
[ProtoInclude(12, typeof(Override))]
[ProtoInclude(13, typeof(Add))]
public abstract class ResourceCostModifier : IEquatable<ResourceCostModifier>, IComparable<ResourceCostModifier>
{
	[ProtoContract]
	[UIField]
	public class Alter : ResourceCostModifier
	{
		[ProtoContract(EnumPassthru = true)]
		public enum Function
		{
			[UITooltip("Both old cost and altered cost will be valid.")]
			Add,
			[UITooltip("Old cost will no longer be valid and only the altered cost will be considered valid")]
			Set
		}

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Determines the altered cost for the cards which pass the <i>affected cost</i> filter set below.")]
		[UIDeepValueChange]
		protected PlayingCard.Filter _alteredCost;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open, tooltip = "Determines which cards will be affected by <i>altered cost</i> set above.")]
		[UIDeepValueChange]
		protected PlayingCard.Filter _affectedCost;

		[ProtoMember(3)]
		[UIField(tooltip = "Determines how the old cost and the altered cost are combined to form the new cost.")]
		protected Function _function;

		public override int sortOrder => (int)(-100 - _function);

		public override bool Equals(ResourceCostModifier other)
		{
			if (other is Alter alter && _affectedCost == alter._affectedCost && _alteredCost == alter._alteredCost)
			{
				return _function == alter._function;
			}
			return false;
		}

		public override AResourceCosts ProcessCost(AResourceCosts resourceCosts)
		{
			List<PlayingCard.Filter> list = (resourceCosts as ResourceCosts)?.cost;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (_affectedCost.AreValid(list[i]))
					{
						list[i] = ((_function == Function.Add) ? (list[i] | _alteredCost) : _alteredCost);
					}
				}
			}
			else if (resourceCosts is FlexibleResourceCosts flexibleResourceCosts && _affectedCost.AreValid(flexibleResourceCosts.filter))
			{
				flexibleResourceCosts.filter = ((_function == Function.Add) ? (flexibleResourceCosts.filter | _alteredCost) : _alteredCost);
			}
			return resourceCosts;
		}

		public override string ToString()
		{
			return string.Format("<b>{0}</b> {1} be used in place of <b>{2}</b>", _alteredCost, (_function == Function.Add) ? "can" : "<i>must</i>", _affectedCost);
		}
	}

	[ProtoContract]
	[UIField]
	public class Reduce : ResourceCostModifier
	{
		[ProtoContract(EnumPassthru = true)]
		public enum Function
		{
			Reduce,
			Set
		}

		private static readonly RangeByte DEFAULT_VALID_COUNT = new RangeByte(1, 5, 1, 5, 0, 0);

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Hide)]
		private PlayingCard.Filter _costToReduce;

		[ProtoMember(2)]
		[UIField(min = 0, max = 5, dynamicInitMethod = "_InitReducedCount")]
		[DefaultValue(1)]
		private int _reducedCount = 1;

		[ProtoMember(3)]
		[UIField]
		private RangeByte _requiredCostRange = DEFAULT_VALID_COUNT;

		[ProtoMember(4)]
		[UIField(validateOnChange = true)]
		private Function _function;

		public override int sortOrder => 100;

		private bool _requiredValidCountSpecified => _requiredCostRange != DEFAULT_VALID_COUNT;

		public override bool Equals(ResourceCostModifier other)
		{
			if (other is Reduce reduce && _reducedCount == reduce._reducedCount && _costToReduce == reduce._costToReduce && _requiredCostRange == reduce._requiredCostRange)
			{
				return _function == reduce._function;
			}
			return false;
		}

		public override AResourceCosts ProcessCost(AResourceCosts resourceCosts)
		{
			if (!(resourceCosts is ResourceCosts resourceCosts2))
			{
				return resourceCosts;
			}
			List<PlayingCard.Filter> cost = resourceCosts2.cost;
			int num = _reducedCount;
			int num2 = 0;
			foreach (PlayingCard.Filter item in cost)
			{
				if (_costToReduce.AreValid(item))
				{
					num2++;
				}
			}
			if (!_requiredCostRange.InRangeSmart(num2))
			{
				return resourceCosts;
			}
			if (_function == Function.Set)
			{
				num = num2 - _reducedCount;
			}
			int num3 = cost.Count - 1;
			while (num3 >= 0 && num > 0)
			{
				if (_costToReduce.AreValid(cost[num3].cards) && num-- > 0)
				{
					cost.RemoveAt(num3);
				}
				num3--;
			}
			return resourceCosts;
		}

		public override string ToString()
		{
			return string.Format("{0} <b>{1}</b> cost of {2}ability {3} <b>{4}</b>", EnumUtil.FriendlyName(_function), _costToReduce, _requiredCostRange.ToRangeString(DEFAULT_VALID_COUNT, "cost ", 100), (_function == Function.Reduce) ? "by" : "to", _reducedCount);
		}

		private void _InitReducedCount(UIFieldAttribute uiField)
		{
			uiField.label = ((_function == Function.Reduce) ? "Reduce Cost By" : "Set Cost To");
			uiField.min = ((_function == Function.Reduce) ? 1 : 0);
		}
	}

	[ProtoContract]
	[UIField]
	public class Add : ResourceCostModifier
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIDeepValueChange]
		private ResourceCosts _addedCosts;

		public override bool Equals(ResourceCostModifier other)
		{
			if (other is Add add)
			{
				return _addedCosts.Equals(add._addedCosts);
			}
			return false;
		}

		public override AResourceCosts ProcessCost(AResourceCosts resourceCosts)
		{
			resourceCosts.additionalCosts += _addedCosts.additionalCosts;
			if (resourceCosts is ResourceCosts resourceCosts2)
			{
				{
					foreach (PlayingCard.Filter item in _addedCosts.cost)
					{
						resourceCosts2.cost.Add(item);
					}
					return resourceCosts;
				}
			}
			return resourceCosts;
		}

		public override string ToString()
		{
			return $"Add [{_addedCosts}] to cost";
		}
	}

	[ProtoContract]
	[UIField]
	public class Override : ResourceCostModifier
	{
		[ProtoMember(1)]
		[UIField]
		private AResourceCosts _overrideCost;

		public override int sortOrder => int.MaxValue;

		public override bool Equals(ResourceCostModifier other)
		{
			if (other is Override @override)
			{
				return _overrideCost.Equals(@override._overrideCost);
			}
			return false;
		}

		public override AResourceCosts ProcessCost(AResourceCosts resourceCosts)
		{
			return (resourceCosts as FlexibleResourceCosts) ?? _overrideCost;
		}

		public override string ToString()
		{
			return $"Override cost to {_overrideCost}";
		}
	}

	public virtual int sortOrder => 0;

	public abstract bool Equals(ResourceCostModifier other);

	public abstract AResourceCosts ProcessCost(AResourceCosts resourceCosts);

	public int CompareTo(ResourceCostModifier other)
	{
		return sortOrder.CompareTo(other.sortOrder);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((ResourceCostModifier)obj);
	}

	public override int GetHashCode()
	{
		return GetType().GetHashCode();
	}
}
