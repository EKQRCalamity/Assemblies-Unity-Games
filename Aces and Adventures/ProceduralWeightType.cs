using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ProceduralWeightType
{
	[UITooltip("Each item in the list is given the desired weight.")]
	PerItem,
	[UITooltip("The desired weight is split amongst the items in the list.")]
	PerPack
}
