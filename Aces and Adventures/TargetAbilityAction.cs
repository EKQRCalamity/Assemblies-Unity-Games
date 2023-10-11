using ProtoBuf;

[ProtoContract]
[UIField("Target Ability Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Ability Card", tooltip = "Does nothing, but can be used to set initial targeting of an ability, or for media cues.")]
public class TargetAbilityAction : AAbilityAction
{
	public override bool hasEffectOnTarget => false;

	private TargetAbilityAction()
	{
	}

	public TargetAbilityAction(Target.AAbility targeting)
	{
		_target = targeting;
	}

	protected override string _ToStringUnique()
	{
		return "Target";
	}
}
