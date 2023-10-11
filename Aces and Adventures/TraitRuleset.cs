using ProtoBuf;

[UIField]
[ProtoContract(EnumPassthru = true)]
public enum TraitRuleset
{
	[UIField("Standard Ruleset", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	[UITooltip("Use standard rules for level up trait availability.")]
	Standard,
	[UIField("Unrestricted Ruleset", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	[UITooltip("Allows picking any one of the nine available level up traits during each level up.")]
	Unrestricted,
	[UIField("Unlocked Ruleset", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	[UITooltip("All traits will be unlocked regardless of character level.")]
	Unlocked
}
