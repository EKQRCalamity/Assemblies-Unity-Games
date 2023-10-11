using System;

public class RewardDeckLayout : ADeckLayout<RewardPile, ATarget>
{
	public ACardLayout draw;

	public ACardLayout select;

	public ACardLayout cardPackSelect;

	public ACardLayout results;

	public ACardLayout discard;

	public ACardLayout leaderboard;

	public CardLayoutSoundPack vialSoundPack;

	public CardLayoutSoundPack cardPackSoundPack;

	public CardLayoutSoundPack classSealSoundPack;

	public CardLayoutSoundPack stoneSoundPack;

	public CardLayoutSoundPack deckSoundPack;

	protected override ACardLayout this[RewardPile? pile]
	{
		get
		{
			return pile switch
			{
				RewardPile.Draw => draw, 
				RewardPile.Select => select, 
				RewardPile.CardPackSelect => cardPackSelect, 
				RewardPile.Results => results, 
				RewardPile.Discard => discard, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case RewardPile.Draw:
					draw = value;
					break;
				case RewardPile.Select:
					select = value;
					break;
				case RewardPile.CardPackSelect:
					cardPackSelect = value;
					break;
				case RewardPile.Results:
					results = value;
					break;
				case RewardPile.Discard:
					discard = value;
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
		if (value is ExperienceVial vial)
		{
			return ExperienceVialView.Create(vial);
		}
		if (value is CardPack cardPack)
		{
			return CardPackView.Create(cardPack);
		}
		if (value is ClassSeal classSeal)
		{
			return ClassSealView.Create(classSeal);
		}
		if (value is GameStone gameStone)
		{
			return GameStoneView.Create(gameStone);
		}
		if (value is ADeck aDeck)
		{
			return ADeckView.Create(aDeck);
		}
		if (value is Leaderboard leaderboard)
		{
			return LeaderboardView.Create(leaderboard);
		}
		return null;
	}
}
