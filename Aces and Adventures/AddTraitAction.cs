using ProtoBuf;

[ProtoContract]
[UIField("Add Trait", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class AddTraitAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(excludedValuesMethod = "_ExcludeTrait", collapse = UICollapseType.Open)]
	private DataRef<AbilityData> _trait;

	[ProtoMember(15)]
	private Id<Ability> _traitId;

	public DataRef<AbilityData> trait => _trait;

	protected override bool _canTick => false;

	private bool _traitSpecified => _trait.ShouldSerialize();

	private bool _traitIdSpecified => _traitId.shouldSerialize;

	protected override void _Apply(ActionContext context, ACombatant combatant)
	{
		_traitId = combatant.AddTrait(_trait);
		if (context.hasAbility)
		{
			context.gameState.addedTraitMap[_traitId] = context.ability;
		}
	}

	protected override void _Unapply(ActionContext context, ACombatant combatant)
	{
		combatant.RemoveTrait(_traitId).ReleaseId();
		context.gameState.addedTraitMap.Remove(_traitId);
		_traitId = Id<Ability>.Null;
	}

	protected override string _ToStringUnique()
	{
		return "Add <b>" + _trait.GetFriendlyName() + "</b> trait to";
	}

	private bool _ExcludeTrait(DataRef<AbilityData> abilityDataRef)
	{
		return !abilityDataRef.data.type.IsTrait();
	}
}
