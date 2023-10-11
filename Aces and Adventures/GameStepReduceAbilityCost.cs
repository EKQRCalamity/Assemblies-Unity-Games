public class GameStepReduceAbilityCost : GameStepSelectAbilityFromDeck
{
	protected override bool _includeAppliedAbilities => true;

	protected override ContentRefDefaults.SelectAbilityData.SelectAbilityActions _selectedAbilityActions => ContentRef.Defaults.selectAbility.reduceCostAbilityActions;

	public GameStepReduceAbilityCost()
		: base(GameState.Instance.abilityDeck, MessageData.GameTooltips.ReduceAbilityCostInDeck)
	{
	}

	protected override bool? _CanSelectAbility(Ability ability)
	{
		return ability.cost.usesCards && ability.cost is ResourceCosts;
	}
}
