using UnityEngine;

public class BonusCardView : AdventureTargetView
{
	public new static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/BonusCardView";

	[Header("Bonus Card")]
	public StringEvent onExperienceChange;

	public BonusCard bonus
	{
		get
		{
			return base.target as BonusCard;
		}
		set
		{
			base.target = value;
		}
	}

	protected override void _OnCardChange()
	{
		base._OnCardChange();
		onExperienceChange?.InvokeLocalized(this, () => MessageData.Instance.game.adventureResult[AdventureResultType.BonusExperience].SetVariables(("New", bonus.isNew && !bonus.oneTimeOnly), ("Unique", bonus.oneTimeOnly), ("Experience", bonus.experience)).Localize());
		onCardFrontChange?.Invoke(AbilityCardSkins.Default[bonus.gameState.player.characterClass].cardFronts[bonus.rank]);
	}
}
