using System.Collections.Generic;

public class GameStepCopyAbility : GameStepSelectAbilityFromDeck
{
	protected override bool _includeAppliedAbilities => true;

	protected override ContentRefDefaults.SelectAbilityData.SelectAbilityActions _selectedAbilityActions => ContentRef.Defaults.selectAbility.copyAbilityActions;

	public GameStepCopyAbility()
		: base(GameState.Instance.abilityDeck, MessageData.GameTooltips.CopyAbilityInDeck)
	{
	}

	protected override IEnumerable<GameStep> _GetStepsToRunAfterSelectActions()
	{
		Ability abilityCopy = null;
		yield return new GameStepGenericSimple(delegate
		{
			abilityCopy = (Ability)base.state.rewardDeck.Add(ProtoUtil.CloneTarget(_selectedAbility), RewardPile.Draw);
			base.view.adventureDeckLayout.inspectHand.Add(abilityCopy.view);
			abilityCopy.view.ClearTransitions();
			abilityCopy.view.frontIsVisible = true;
			abilityCopy.view.transform.CopyFrom(_selectedAbility.view.transform);
		});
		yield return new GameStepWait(1f);
		yield return new GameStepGenericSimple(delegate
		{
			_selectedAbilityLayout.Add(_selectedAbility.view);
			_selectedAbility.view.ClearExitTransitions();
			base.state.abilityDeck.Transfer(abilityCopy, Ability.Pile.Draw);
		});
	}
}
