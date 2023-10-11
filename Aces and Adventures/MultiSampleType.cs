using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UIField]
public enum MultiSampleType
{
	UseQualityPreset,
	Off,
	[UIField("x2", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	X2,
	[UIField("x4", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	X4,
	[UIField("x8", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	X8
}
