using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(9, typeof(AdditionalResourceCost))]
[ProtoInclude(10, typeof(CardCount))]
[ProtoInclude(11, typeof(CostType))]
[ProtoInclude(12, typeof(Free))]
public abstract class ResourceCostFilter
{
	[ProtoContract]
	[UIField(tooltip = "Check if cost of ability contains filtered cards in a certain range count.")]
	public class CardCount : ResourceCostFilter
	{
		private static readonly RangeByte DEFAULT_COUNT = new RangeByte(1, 5, 0, 5, 0, 0);

		[ProtoMember(1)]
		[UIField]
		private RangeByte _count = DEFAULT_COUNT;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Hide)]
		private PlayingCard.Filter _filter;

		private bool _countSpecified => _count != DEFAULT_COUNT;

		protected override bool _IsValid(ActionContext context, AResourceCosts cost)
		{
			return _count.InRangeSmart(cost.GetResourceFilters().Count((PlayingCard.Filter cardTypes) => !_filter || ((bool)cardTypes && _filter.AreValid(cardTypes))));
		}

		protected override string _ToString()
		{
			return _count.ToRangeString(null, "", 50) + " " + (_filter ? _filter.ToString() : "").SpaceIfNotEmpty() + "Cost";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Check if additional costs of ability fall within a given cost range.\n<i>Attacks, Shields, HP, etc.</i>")]
	public class AdditionalResourceCost : ResourceCostFilter
	{
		private static readonly AdditionalResourceCosts MAX = new AdditionalResourceCosts(5, 5, 5, 5);

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		private AdditionalResourceCosts _minCosts;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		private AdditionalResourceCosts _maxCosts = MAX;

		private bool _minCostsSpecified => _minCosts;

		private bool _maxCostsSpecified => _maxCosts != MAX;

		protected override bool _IsValid(ActionContext context, AResourceCosts cost)
		{
			RangeByte rangeByte = new RangeByte(_minCosts.hp, _maxCosts.hp, 0, 100, 0, 0);
			RangeByte rangeByte2 = new RangeByte(_minCosts.shield, _maxCosts.shield, 0, 100, 0, 0);
			RangeByte rangeByte3 = new RangeByte(_minCosts.attack, _maxCosts.attack, 0, 100, 0, 0);
			RangeByte rangeByte4 = new RangeByte(_minCosts.heroAbility, _maxCosts.heroAbility, 0, 100, 0, 0);
			AdditionalResourceCosts additionalCosts = cost.additionalCosts;
			if (rangeByte.InRangeSmart((int)additionalCosts.hp) && rangeByte2.InRangeSmart((int)additionalCosts.shield) && rangeByte3.InRangeSmart((int)additionalCosts.attack))
			{
				return rangeByte4.InRangeSmart((int)additionalCosts.heroAbility);
			}
			return false;
		}

		protected override string _ToString()
		{
			RangeByte value = new RangeByte(0, 5, 0, 100, 0, 0);
			RangeByte rangeByte = new RangeByte(_minCosts.hp, _maxCosts.hp, 0, 5, 0, 0);
			RangeByte rangeByte2 = new RangeByte(_minCosts.shield, _maxCosts.shield, 0, 5, 0, 0);
			RangeByte rangeByte3 = new RangeByte(_minCosts.attack, _maxCosts.attack, 0, 5, 0, 0);
			RangeByte rangeByte4 = new RangeByte(_minCosts.heroAbility, _maxCosts.heroAbility, 0, 5, 0, 0);
			return rangeByte.ToRangeString(value, "HP ", 100) + rangeByte2.ToRangeString(value, "Shield ", 100) + rangeByte3.ToRangeString(value, "Attacks ", 100) + rangeByte4.ToRangeString(value, "Hero Ability ", 100) + "Cost";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Is cost Standard, Flexible, or Poker cost.")]
	public class CostType : ResourceCostFilter
	{
		[ProtoContract(EnumPassthru = true)]
		public enum Type
		{
			Standard,
			Flexible,
			Poker
		}

		[ProtoMember(1)]
		[UIField]
		private Type _type;

		protected override bool _IsValid(ActionContext context, AResourceCosts cost)
		{
			return _type switch
			{
				Type.Standard => cost is ResourceCosts, 
				Type.Flexible => cost is FlexibleResourceCosts, 
				Type.Poker => cost is PokerResourceCost, 
				_ => false, 
			};
		}

		protected override string _ToString()
		{
			return EnumUtil.FriendlyName(_type) + " Cost";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Cost uses no cards, and has no additional costs.")]
	public class Free : ResourceCostFilter
	{
		[ProtoMember(1)]
		[UIField]
		private bool _invert;

		protected override bool _IsValid(ActionContext context, AResourceCosts cost)
		{
			return _invert ^ cost.isFree;
		}

		protected override string _ToString()
		{
			return _invert.ToText("!") + "Free Cost";
		}
	}

	[ProtoMember(1)]
	[UIField]
	protected bool _useNaturalCost;

	protected abstract bool _IsValid(ActionContext context, AResourceCosts cost);

	protected abstract string _ToString();

	public bool IsValid(ActionContext context, Ability targetAbility)
	{
		AResourceCosts aResourceCosts = ((!_useNaturalCost) ? targetAbility?.cost : targetAbility?.naturalCost);
		if (aResourceCosts != null)
		{
			return _IsValid(context, aResourceCosts);
		}
		return false;
	}

	public sealed override string ToString()
	{
		return _ToString() + _useNaturalCost.ToText(" (Natural)".SizeIfNotEmpty());
	}
}
