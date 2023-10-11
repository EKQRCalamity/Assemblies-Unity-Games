using UnityEngine;

public class AdventureResultCardView : AdventureTargetView
{
	public new static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/AdventureResultCardView";

	public AdventureResultCard result
	{
		get
		{
			return base.target as AdventureResultCard;
		}
		set
		{
			base.target = value;
		}
	}

	protected override void _OnCardChange()
	{
		base._OnCardChange();
		onCardFrontChange?.Invoke(AbilityCardSkins.Default[result.gameState.player.characterClass].cardFronts[result.rank.ToAbilityRank()]);
	}
}
