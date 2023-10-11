using ProtoBuf;

[ProtoContract]
[UIField("Target Stone", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Tutorial Only", tooltip = "Used to target stones for tutorial purposes.")]
public class TargetStoneAction : AAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private Target.Stone _target;

	public override bool hasEffectOnTarget => false;

	public override Target target => _target;

	protected override string _ToStringUnique()
	{
		return "Target";
	}
}
