using System;

public class LevelUpLeafDeckLayout : ADeckLayout<LevelUpLeafPile, LevelUpLeaf>
{
	public ACardLayout main;

	protected override ACardLayout this[LevelUpLeafPile? pile]
	{
		get
		{
			return main;
		}
		set
		{
			if (pile.HasValue)
			{
				if (pile.GetValueOrDefault() != 0)
				{
					throw new ArgumentOutOfRangeException("pile", pile, null);
				}
				main = value;
			}
		}
	}

	protected override CardLayoutElement _CreateView(LevelUpLeaf value)
	{
		return LevelUpLeafView.Create(value);
	}
}
