using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class Statistics
{
	[ProtoMember(1)]
	private BInt _offense;

	[ProtoMember(2)]
	private BInt _defense;

	[ProtoMember(3)]
	private ClampedBInt _health;

	[ProtoMember(4)]
	private BInt _numberOfAttacks;

	[ProtoMember(5)]
	private BInt _shieldRetention;

	private Dictionary<StatType, PoolKeepItemListHandle<AppliedAction>> _appliedActions;

	public BInt offense => _offense ?? (_offense = new BInt());

	public BInt defense => _defense ?? (_defense = new BInt());

	public BInt health => _health ?? (_health = new ClampedBInt());

	public BInt numberOfAttacks => _numberOfAttacks ?? (_numberOfAttacks = new BInt());

	public BInt shieldRetention => _shieldRetention ?? (_shieldRetention = new BInt());

	private Dictionary<StatType, PoolKeepItemListHandle<AppliedAction>> appliedActions => _appliedActions ?? (_appliedActions = new Dictionary<StatType, PoolKeepItemListHandle<AppliedAction>>());

	public BInt this[StatType stat] => stat switch
	{
		StatType.Offense => offense, 
		StatType.Defense => defense, 
		StatType.Health => health, 
		StatType.NumberOfAttacks => numberOfAttacks, 
		StatType.ShieldRetention => shieldRetention, 
		_ => throw new ArgumentOutOfRangeException("stat", stat, null), 
	};

	private bool _shieldRetentionSpecified => (int)_shieldRetention != 0;

	public Statistics()
	{
	}

	public Statistics(int offense, int defense, int health, int numberOfAttacks = 1, int shieldRetention = 0)
	{
		this.offense.value = offense;
		this.defense.value = defense;
		this.health.value = health;
		this.numberOfAttacks.value = numberOfAttacks;
		this.shieldRetention.value = shieldRetention;
	}

	public void Register(StatType stat, AppliedAction appliedAction)
	{
		Dictionary<StatType, PoolKeepItemListHandle<AppliedAction>> dictionary = appliedActions;
		PoolKeepItemListHandle<AppliedAction> obj = appliedActions.GetValueOrDefault(stat) ?? Pools.UseKeepItemList<AppliedAction>();
		PoolKeepItemListHandle<AppliedAction> poolKeepItemListHandle = obj;
		dictionary[stat] = obj;
		poolKeepItemListHandle.value.AddSortedStable(appliedAction);
		Recalculate(stat);
	}

	public void Unregister(StatType stat, AppliedAction appliedAction)
	{
		List<AppliedAction> value = appliedActions[stat].value;
		if (value.Remove(appliedAction) && value.Count == 0)
		{
			appliedActions.RemoveAndRepool(stat);
		}
		else
		{
			Recalculate(stat);
		}
	}

	public void Recalculate(StatType stat)
	{
		PoolKeepItemListHandle<AppliedAction> valueOrDefault = appliedActions.GetValueOrDefault(stat);
		if (valueOrDefault == null)
		{
			return;
		}
		using (this[stat].Suppress())
		{
			foreach (AppliedAction item in valueOrDefault.value)
			{
				item.action.Unapply(item.context);
			}
			foreach (AppliedAction item2 in valueOrDefault.value)
			{
				item2.action.Apply(item2.context);
			}
		}
	}
}
