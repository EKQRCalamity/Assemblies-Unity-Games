using ProtoBuf;

[ProtoContract]
[UIField("Target Resource Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Resource Card", tooltip = "Does nothing, but can be used to set initial targeting of an ability, or for media cues.")]
public class TargetResourceAction : AResourceAction
{
	public override bool hasEffectOnTarget => false;

	protected override string _ToStringUnique()
	{
		return "Target";
	}
}
