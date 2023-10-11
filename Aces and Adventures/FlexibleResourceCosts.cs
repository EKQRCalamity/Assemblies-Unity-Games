using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Flexible Playing Cards", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class FlexibleResourceCosts : AResourceCosts
{
	private static readonly ResourceBlueprint<GameObject> _CostBlueprint = "GameState/Ability/Cost/FlexibleCost";

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	private PlayingCard.Filter _filter;

	[ProtoMember(2)]
	[UIField(min = 0, max = 5)]
	private int _maxCount;

	public PlayingCard.Filter filter
	{
		get
		{
			return _filter;
		}
		set
		{
			_filter = value;
		}
	}

	public override ResourceCardCounts cardCounts => EnumUtil<ResourceCardCounts>.AllFlagsExcept(ResourceCardCounts.Zero);

	public override PoolKeepItemListHandle<ResourceCard> GetActivationCards(IEnumerable<ResourceCard> availableCards)
	{
		PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(availableCards.Where((ResourceCard r) => _filter.AreValid(r)).Take((_maxCount > 0) ? _maxCount : int.MaxValue));
		bool num = poolKeepItemListHandle.Count > 0;
		if (!num)
		{
			poolKeepItemListHandle.IfNotNullDispose();
		}
		if (!num)
		{
			return null;
		}
		return poolKeepItemListHandle;
	}

	public override AbilityPreventedBy? GetMissingResourceType(IEnumerable<ResourceCard> availableCards)
	{
		return _filter;
	}

	public override IEnumerable<PlayingCard.Filter> GetResourceFilters()
	{
		yield return _filter;
	}

	protected override IEnumerable<AbilityPreventedBy?> _GetUniqueAbilityPreventedBys()
	{
		yield return null;
	}

	protected override IEnumerable<GameObject> _GetUniqueCostIcons()
	{
		yield return Object.Instantiate((GameObject)_CostBlueprint).GetOrAddComponent<LocalizedStringRef>().SetData(ResourceCostIconType.Flexible.GetTooltip())
			.gameObject;
	}

	protected override void _WildIntoCost(PoolKeepItemListHandle<ResourceCard> activationCards)
	{
		foreach (ResourceCard item in activationCards.value)
		{
			_filter.WildIntoValid(item);
		}
	}

	public override string ToString()
	{
		PlayingCard.Filter filter = _filter;
		return "(x) " + filter.ToString() + ((_maxCount > 0) ? $" (Max of {_maxCount})".SizeIfNotEmpty() : "") + _additionalCosts.ToString().PreSpaceIfNotEmpty();
	}

	public override bool Equals(AResourceCosts other)
	{
		if (other is FlexibleResourceCosts flexibleResourceCosts && _filter == flexibleResourceCosts._filter && _maxCount == flexibleResourceCosts._maxCount)
		{
			return base.Equals(other);
		}
		return false;
	}
}
