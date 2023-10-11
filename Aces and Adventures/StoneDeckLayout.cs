using System.Collections.Generic;

public class StoneDeckLayout : ADeckLayout<Stone.Pile, Stone>
{
	public ACardLayout inactive;

	public ACardLayout turnInactive;

	public ACardLayout playerTurn;

	public ACardLayout playerReaction;

	public ACardLayout enemyTurn;

	public ACardLayout cancelInactive;

	public ACardLayout cancel;

	public ACardLayout cancelFloating;

	private Dictionary<StoneType, Id<Stone>> _map = new Dictionary<StoneType, Id<Stone>>();

	protected override ACardLayout this[Stone.Pile? pile]
	{
		get
		{
			return pile switch
			{
				Stone.Pile.Inactive => inactive, 
				Stone.Pile.TurnInactive => turnInactive, 
				Stone.Pile.PlayerTurn => playerTurn, 
				Stone.Pile.PlayerReaction => playerReaction, 
				Stone.Pile.EnemyTurn => enemyTurn, 
				Stone.Pile.CancelInactive => cancelInactive, 
				Stone.Pile.Cancel => cancel, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case Stone.Pile.Inactive:
					inactive = value;
					break;
				case Stone.Pile.TurnInactive:
					turnInactive = value;
					break;
				case Stone.Pile.PlayerTurn:
					playerTurn = value;
					break;
				case Stone.Pile.PlayerReaction:
					playerReaction = value;
					break;
				case Stone.Pile.EnemyTurn:
					enemyTurn = value;
					break;
				case Stone.Pile.CancelInactive:
					cancelInactive = value;
					break;
				case Stone.Pile.Cancel:
					cancel = value;
					break;
				}
			}
		}
	}

	public Stone this[StoneType type]
	{
		get
		{
			Stone stone = _map.GetValueOrDefault(type).value;
			if (stone == null)
			{
				Id<Stone> id2 = (_map[type] = base.deck.Add(new Stone(type), (type == StoneType.Cancel) ? Stone.Pile.CancelInactive : Stone.Pile.Inactive));
				stone = id2;
			}
			return stone;
		}
	}

	protected override CardLayoutElement _CreateView(Stone value)
	{
		Id<Stone> id2 = (_map[value] = value);
		return StoneView.Create(id2);
	}

	public void Transfer(StoneType type, Stone.Pile pile)
	{
		base.deck.Transfer(this[type], pile);
	}
}
