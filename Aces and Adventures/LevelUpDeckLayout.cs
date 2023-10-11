using System;

public class LevelUpDeckLayout : ADeckLayout<LevelUpPile, ATarget>
{
	public ACardLayout vialDraw;

	public ACardLayout vial;

	public ACardLayout vialPour;

	public ACardLayout vialDiscard;

	public ACardLayout seals;

	public ACardLayout activeSeal;

	public ACardLayout potDraw;

	public ACardLayout pot;

	public ACardLayout potDiscard;

	public ACardLayout leafExit;

	public ACardLayout presentLevelUp;

	public ACardLayout discardLevelUp;

	public ACardLayout levelUps;

	public ACardLayout levelUpsView;

	public ACardLayout levelUpsTransition;

	protected override ACardLayout this[LevelUpPile? pile]
	{
		get
		{
			return pile switch
			{
				LevelUpPile.VialDraw => vialDraw, 
				LevelUpPile.Vial => vial, 
				LevelUpPile.VialPour => vialPour, 
				LevelUpPile.VialDiscard => vialDiscard, 
				LevelUpPile.Seals => seals, 
				LevelUpPile.ActiveSeal => activeSeal, 
				LevelUpPile.PotDraw => potDraw, 
				LevelUpPile.Pot => pot, 
				LevelUpPile.PotDiscard => potDiscard, 
				LevelUpPile.LeafExit => leafExit, 
				LevelUpPile.PresentLevelUp => presentLevelUp, 
				LevelUpPile.DiscardLevelUp => discardLevelUp, 
				LevelUpPile.LevelUps => levelUps, 
				LevelUpPile.LevelUpsView => levelUpsView, 
				LevelUpPile.LevelUpsTransition => levelUpsTransition, 
				null => null, 
				_ => throw new ArgumentOutOfRangeException("pile", pile, null), 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case LevelUpPile.VialDraw:
					vialDraw = value;
					break;
				case LevelUpPile.Vial:
					vial = value;
					break;
				case LevelUpPile.VialPour:
					vialPour = value;
					break;
				case LevelUpPile.VialDiscard:
					vialDiscard = value;
					break;
				case LevelUpPile.Seals:
					seals = value;
					break;
				case LevelUpPile.ActiveSeal:
					activeSeal = value;
					break;
				case LevelUpPile.PotDraw:
					potDraw = value;
					break;
				case LevelUpPile.Pot:
					pot = value;
					break;
				case LevelUpPile.PotDiscard:
					potDiscard = value;
					break;
				case LevelUpPile.LeafExit:
					leafExit = value;
					break;
				case LevelUpPile.PresentLevelUp:
					presentLevelUp = value;
					break;
				case LevelUpPile.DiscardLevelUp:
					discardLevelUp = value;
					break;
				case LevelUpPile.LevelUps:
					levelUps = value;
					break;
				case LevelUpPile.LevelUpsView:
					levelUpsView = value;
					break;
				case LevelUpPile.LevelUpsTransition:
					levelUpsTransition = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("pile", pile, null);
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(ATarget value)
	{
		if (value is IAdventureCard)
		{
			return AdventureTargetView.Create(value);
		}
		if (value is ExperienceVial experienceVial)
		{
			return ExperienceVialView.Create(experienceVial);
		}
		if (value is ClassSeal classSeal)
		{
			return ClassSealView.Create(classSeal);
		}
		if (value is LevelUpPlant plant)
		{
			return LevelUpPlantView.Create(plant);
		}
		if (value is LevelUpLeaf leaf)
		{
			return LevelUpLeafView.Create(leaf);
		}
		if (value is LevelUpReward reward)
		{
			return LevelUpRewardView.Create(reward);
		}
		return null;
	}
}
