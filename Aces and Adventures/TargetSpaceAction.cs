using ProtoBuf;

[ProtoContract]
[UIField("Target Turn Order Space", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Turn Order Space", tooltip = "Does nothing, but can be used to set initial targeting of an ability, or for media cues.")]
public class TargetSpaceAction : ASpaceAction
{
	public override bool hasEffectOnTarget => false;

	protected override string _ToStringUnique()
	{
		return "Target";
	}
}
