using System.Collections.Generic;

public class GameStepAbilityCastBegin : GameStep
{
	public Ability ability { get; }

	public List<ATarget> targets { get; }

	public GameStepAbilityCastBegin(Ability ability, List<ATarget> targets)
	{
		this.ability = ability;
		this.targets = targets;
	}

	protected override void OnFirstEnabled()
	{
		ability?.cost.ConsumeResources(ability.owner);
	}

	public override void Start()
	{
		base.state.SignalBeginToUseAbility(ability, targets);
	}
}
