using System;

public class GameStoneDeckLayout : ADeckLayout<GameStone.Pile, GameStone>
{
	public ACardLayout draw;

	public ACardLayout select;

	public ACardLayout discard;

	protected override ACardLayout this[GameStone.Pile? pile]
	{
		get
		{
			return pile switch
			{
				GameStone.Pile.Draw => draw, 
				GameStone.Pile.Select => select, 
				GameStone.Pile.Discard => discard, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case GameStone.Pile.Draw:
					draw = value;
					break;
				case GameStone.Pile.Select:
					select = value;
					break;
				case GameStone.Pile.Discard:
					discard = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("pile", pile, null);
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(GameStone value)
	{
		return GameStoneView.Create(value);
	}
}
