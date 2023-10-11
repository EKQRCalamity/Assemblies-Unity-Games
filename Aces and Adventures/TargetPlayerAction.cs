using ProtoBuf;

[ProtoContract]
[UIField("Target Player", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Player", tooltip = "Does nothing, but can be used to set initial targeting of an ability, or for media cues.")]
public class TargetPlayerAction : APlayerAction
{
	public override bool hasEffectOnTarget => false;

	protected override string _ToStringUnique()
	{
		return "Target";
	}
}
