using ProtoBuf;

[ProtoContract]
public class MapCompass : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Inactive,
		Active
	}
}
