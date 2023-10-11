using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Playing Cards", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class ResourceCosts : AResourceCosts
{
	[ProtoMember(1, OverwriteList = true)]
	[UIField(collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<PlayingCard.Filter> _cost;

	public List<PlayingCard.Filter> cost => _cost ?? (_cost = new List<PlayingCard.Filter>());

	public override bool usesCards => cost.Count > 0;

	public override bool isFree
	{
		get
		{
			if (!usesCards)
			{
				return base.additionalCosts.isFree;
			}
			return false;
		}
	}

	public override ResourceCardCounts cardCounts => (ResourceCardCounts)(1 << cost.Count);

	public ResourceCosts()
	{
	}

	public ResourceCosts(IEnumerable<PlayingCard.Filter> costs)
	{
		_cost = new List<PlayingCard.Filter>(costs);
	}

	public override PoolKeepItemListHandle<ResourceCard> GetActivationCards(IEnumerable<ResourceCard> availableCards)
	{
		if (cost.Count == 0)
		{
			return Pools.UseKeepItemList<ResourceCard>();
		}
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(availableCards);
		if (poolKeepItemListHandle.Count < cost.Count)
		{
			return null;
		}
		using PoolListHandle<PoolKeepItemListHandle<ResourceCard>> poolListHandle = Pools.UseList<PoolKeepItemListHandle<ResourceCard>>();
		for (int i = 0; i < cost.Count; i++)
		{
			PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = Pools.UseKeepItemList<ResourceCard>();
			foreach (ResourceCard item in poolKeepItemListHandle.value)
			{
				if (((PlayingCardTypes)item & cost[i].cards) != (PlayingCardTypes)0L)
				{
					poolKeepItemListHandle2.Add(item);
				}
			}
			poolListHandle.Add(poolKeepItemListHandle2);
			if (poolKeepItemListHandle2.Count == 0)
			{
				return null;
			}
		}
		using PoolKeepItemHashSetHandle<ResourceCard> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<ResourceCard>();
		foreach (IEnumerable<ResourceCard> item2 in poolListHandle.value.SingleItemPerListPermutations())
		{
			foreach (ResourceCard item3 in item2)
			{
				if (!poolKeepItemHashSetHandle.Add(item3))
				{
					break;
				}
			}
			if (poolKeepItemHashSetHandle.Count == cost.Count)
			{
				return Pools.UseKeepItemList(poolKeepItemHashSetHandle.value);
			}
			poolKeepItemHashSetHandle.value.Clear();
		}
		return null;
	}

	public override AbilityPreventedBy? GetMissingResourceType(IEnumerable<ResourceCard> availableCards)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(availableCards);
		if (poolKeepItemListHandle.Count < cost.Count)
		{
			return AbilityPreventedBy.ResourceCard;
		}
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = Pools.UseKeepItemList<ResourceCard>();
		foreach (IEnumerable<PlayingCard.Filter> item in cost.Permutations())
		{
			poolKeepItemListHandle2.value.ClearAndCopyFrom(poolKeepItemListHandle.value);
			foreach (PlayingCard.Filter item2 in item)
			{
				int num = poolKeepItemListHandle2.value.Count - 1;
				while (true)
				{
					if (num >= 0)
					{
						if (item2.AreValid(poolKeepItemListHandle2[num]))
						{
							break;
						}
						num--;
						continue;
					}
					return item2;
				}
				poolKeepItemListHandle2.RemoveAt(num);
			}
		}
		return null;
	}

	public override IEnumerable<PlayingCard.Filter> GetResourceFilters()
	{
		foreach (PlayingCard.Filter item in cost)
		{
			yield return item;
		}
	}

	protected override void _WildIntoCost(PoolKeepItemListHandle<ResourceCard> activationCards)
	{
		if (cost.Count == 0)
		{
			return;
		}
		PoolKeepItemDictionaryHandle<ResourceCard, int> matchCountMap = Pools.UseKeepItemDictionary<ResourceCard, int>();
		try
		{
			foreach (ResourceCard item in activationCards.value)
			{
				foreach (PlayingCard.Filter item2 in cost)
				{
					if (item2.AreValid(item))
					{
						matchCountMap[item] = matchCountMap.value.GetValueOrDefault(item) + 1;
					}
				}
			}
			activationCards.value.Sort((ResourceCard a, ResourceCard b) => matchCountMap[a] - matchCountMap[b]);
			using PoolKeepItemDictionaryHandle<int, ResourceCard> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<int, ResourceCard>();
			foreach (IEnumerable<ResourceCard> item3 in activationCards.value.Permutations())
			{
				foreach (ResourceCard item4 in item3)
				{
					for (int i = 0; i < cost.Count; i++)
					{
						if (cost[i].AreValid(item4) && !poolKeepItemDictionaryHandle.ContainsKey(i))
						{
							poolKeepItemDictionaryHandle[i] = item4;
							break;
						}
					}
				}
				if (poolKeepItemDictionaryHandle.Count == cost.Count)
				{
					foreach (KeyValuePair<int, ResourceCard> item5 in poolKeepItemDictionaryHandle.value)
					{
						cost[item5.Key].WildIntoValid(item5.Value);
					}
					break;
				}
				poolKeepItemDictionaryHandle.value.Clear();
			}
		}
		finally
		{
			if (matchCountMap != null)
			{
				((IDisposable)matchCountMap).Dispose();
			}
		}
	}

	public override bool Equals(AResourceCosts other)
	{
		if (other is ResourceCosts resourceCosts && _cost.SequenceEqual(resourceCosts._cost))
		{
			return base.Equals(other);
		}
		return false;
	}

	public override string ToString()
	{
		if (!cost.IsNullOrEmpty() || (bool)base.additionalCosts)
		{
			return cost.Select((PlayingCard.Filter f) => f.ToString()).Concat(base.additionalCosts.GetCostStrings()).ToStringSmart(" & ");
		}
		return "Free";
	}
}
