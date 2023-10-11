using ProtoBuf;

[ProtoContract]
public class Chip : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Inactive,
		Attack,
		ActiveAttack
	}

	[ProtoMember(1)]
	private ChipType _type;

	public ChipType type => _type;

	public Pile pile => base.gameState.chipDeck[this].GetValueOrDefault();

	private Chip()
	{
	}

	public Chip(ChipType type)
	{
		_type = type;
	}

	public static implicit operator ChipType(Chip card)
	{
		return card._type;
	}

	public static implicit operator Chip(ChipType type)
	{
		return new Chip(type);
	}
}
