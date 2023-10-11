using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum AutoSelectSingleTargetType
{
	[UITooltip("Single target abilities always require clicking a target, regardless if there is only one valid target.")]
	Never,
	[UITooltip("Reactive abilities which only have single valid target automatically select them when used.")]
	Reactions,
	[UITooltip("Abilities which only have a single valid target automatically select them when used.")]
	Always
}
