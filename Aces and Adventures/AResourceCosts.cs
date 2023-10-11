using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(ResourceCosts))]
[ProtoInclude(11, typeof(FlexibleResourceCosts))]
[ProtoInclude(12, typeof(PokerResourceCost))]
public abstract class AResourceCosts : IEquatable<AResourceCosts>
{
	[ProtoMember(1)]
	[UIField]
	[UIDeepValueChange]
	protected AdditionalResourceCosts _additionalCosts;

	public AdditionalResourceCosts additionalCosts
	{
		get
		{
			return _additionalCosts;
		}
		set
		{
			_additionalCosts = value;
		}
	}

	public abstract ResourceCardCounts cardCounts { get; }

	public virtual bool usesCards => true;

	public virtual bool isFree => false;

	private bool _additionalCostsSpecified => _additionalCosts;

	public AbilityPreventedBy? HasResources(ACombatant combatant)
	{
		return _additionalCosts.HasResources(combatant);
	}

	public void ConsumeResources(ACombatant combatant)
	{
		_additionalCosts.ConsumeResources(combatant);
	}

	public virtual PoolKeepItemListHandle<ResourceCard> GetActivationCards(IEnumerable<ResourceCard> availableCards)
	{
		return null;
	}

	public virtual AbilityPreventedBy? GetMissingResourceType(IEnumerable<ResourceCard> availableCards)
	{
		return null;
	}

	protected virtual IEnumerable<AbilityPreventedBy?> _GetUniqueAbilityPreventedBys()
	{
		yield break;
	}

	public IEnumerable<AbilityPreventedBy?> GetAbilityPreventedBys()
	{
		foreach (AbilityPreventedBy? item in _GetUniqueAbilityPreventedBys())
		{
			yield return item;
		}
		foreach (PlayingCard.Filter resourceFilter in GetResourceFilters())
		{
			yield return resourceFilter.abilityPreventedBy;
		}
		foreach (AbilityPreventedBy abilityPreventedBy in additionalCosts.GetAbilityPreventedBys())
		{
			yield return abilityPreventedBy;
		}
	}

	public virtual IEnumerable<PlayingCard.Filter> GetResourceFilters()
	{
		yield break;
	}

	protected virtual IEnumerable<GameObject> _GetUniqueCostIcons()
	{
		yield break;
	}

	protected virtual void _WildIntoCost(PoolKeepItemListHandle<ResourceCard> activationCards)
	{
	}

	public void WildIntoCost(IEnumerable<ResourceCard> activationCards)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(activationCards);
		PoolHandle<ResourceCard.FreezeWildValueSnapshot> item = poolKeepItemListHandle.value.FreezeWildValues();
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = GetActivationCards(poolKeepItemListHandle.value);
		Pools.Repool(item);
		if (!poolKeepItemListHandle2 || poolKeepItemListHandle2.Count != poolKeepItemListHandle.Count)
		{
			_WildIntoCost(poolKeepItemListHandle);
		}
	}

	public IEnumerable<GameObject> GetCostIcons()
	{
		foreach (GameObject item in _GetUniqueCostIcons())
		{
			yield return item;
		}
		foreach (PlayingCard.Filter resourceFilter in GetResourceFilters())
		{
			yield return resourceFilter.GetCostIcon();
		}
		foreach (GameObject costIcon in additionalCosts.GetCostIcons())
		{
			yield return costIcon;
		}
	}

	public virtual bool Equals(AResourceCosts other)
	{
		AdditionalResourceCosts value = _additionalCosts;
		AdditionalResourceCosts? obj = other?.additionalCosts;
		return value == obj;
	}
}
