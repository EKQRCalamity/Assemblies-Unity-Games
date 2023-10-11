using System.Collections.Generic;

public class GameStepGroupAbilityAct : GameStepGroup
{
	public Ability ability { get; }

	public bool usesCards { get; private set; }

	protected override bool _changesContext => true;

	public GameStepGroupAbilityAct(Ability ability)
	{
		this.ability = ability;
	}

	protected override IEnumerable<GameStep> _GetSteps()
	{
		yield return new GameStepAbilityActBegin(ability);
		ActionContext context = new ActionContext(ability.owner, ability).SetCapturedValue(ability.reactionCapturedValue);
		foreach (AAction action in ability.data.actions)
		{
			foreach (GameStep actGameStep in action.GetActGameSteps(context))
			{
				yield return actGameStep;
			}
		}
		yield return new GameStepAbilityActComplete(ability);
	}

	public void SetUsesCards(bool setUsesCards)
	{
		usesCards = setUsesCards;
	}
}
