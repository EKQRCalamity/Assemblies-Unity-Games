using System.Linq;
using ProtoBuf;

[ProtoContract]
public class TurnOrderSpace : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Inactive,
		Active
	}

	[ProtoMember(1)]
	private BBool _tapped;

	public BBool tapped
	{
		get
		{
			return _tapped ?? (_tapped = new BBool());
		}
		set
		{
			_tapped = value;
		}
	}

	public bool isActive => base.gameState.turnOrderSpaceDeck[this] == Pile.Active;

	public int? index
	{
		get
		{
			if (!isActive)
			{
				return null;
			}
			return base.gameState.turnOrderSpaceDeck.GetIndexOf(this);
		}
	}

	public int DistanceTo(AEntity entity)
	{
		int turnOrder = entity.gameState.GetTurnOrder(entity);
		int valueOrDefault = index.GetValueOrDefault();
		if (valueOrDefault <= turnOrder)
		{
			return turnOrder - valueOrDefault;
		}
		return valueOrDefault - turnOrder - 1;
	}

	public PoolKeepItemListHandle<AEntity> GetEntitiesToLeftOf()
	{
		return Pools.UseKeepItemList(base.gameState.turnOrderQueue.Take(index.GetValueOrDefault()).Reverse());
	}

	public PoolKeepItemListHandle<AEntity> GetEntitiesToRightOf()
	{
		return Pools.UseKeepItemList(base.gameState.turnOrderQueue.Skip(index.GetValueOrDefault()));
	}
}
