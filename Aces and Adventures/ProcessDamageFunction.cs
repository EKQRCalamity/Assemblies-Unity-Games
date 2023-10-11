using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ProcessDamageFunction
{
	[UITooltip("Add <i>Damage Adjustment</i> to current damage being dealt.")]
	Add,
	[UITooltip("Subtract <i>Damage Adjustment</i> to current damage being dealt.")]
	Subtract,
	[UITooltip("Set current damage being dealt to <i>Damage Adjustment</i>.")]
	Set,
	[UITooltip("Multiply the damage multiplier of the current damage being dealt by <i>Damage Adjustment</i>.")]
	Multiply,
	[UITooltip("Multiply the damage denominator of the current damage being dealt by <i>Damage Adjustment</i>.")]
	Divide
}
