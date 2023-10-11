using ProtoBuf;

[ProtoContract]
[UIField("Target Combatant", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant", tooltip = "Does nothing, but can be used to set initial targeting of an ability, or for media cues.")]
public class TargetCombatantAction : ACombatantAction
{
	public override bool hasEffectOnTarget => false;

	public override Target target => _target ?? (_target = new Target.Combatant.Select());

	protected override string _ToStringUnique()
	{
		return "Target";
	}
}
