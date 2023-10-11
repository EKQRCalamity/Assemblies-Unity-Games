using System.Collections.Generic;

public class GameStepRemoveAbility : GameStepSelectAbilityFromDeck
{
	protected override ContentRefDefaults.SelectAbilityData.SelectAbilityActions _selectedAbilityActions => ContentRef.Defaults.selectAbility.removeAbilityActions;

	public GameStepRemoveAbility(int count)
		: base(GameState.Instance.abilityDeck, MessageData.GameTooltips.RemoveAbilityFromDeck, count)
	{
	}

	protected override IEnumerable<GameStep> _GetStepsToRunAfterSelectActions()
	{
		if (_done)
		{
			yield return new GameStepWait(0.5f);
		}
		yield return new GameStepGenericSimple(delegate
		{
			base.state.exileDeck.Transfer(_selectedAbility, ExilePile.ClearGameState);
			_selectedAbility.view.ClearExitTransitions();
		});
	}
}
