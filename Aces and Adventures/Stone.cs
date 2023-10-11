using ProtoBuf;

[ProtoContract]
public class Stone : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Inactive,
		TurnInactive,
		PlayerTurn,
		PlayerReaction,
		EnemyTurn,
		CancelInactive,
		Cancel
	}

	[ProtoMember(1)]
	private StoneType _type;

	public StoneType type => _type;

	public Pile pile => base.gameState.stoneDeck[this].GetValueOrDefault();

	private Stone()
	{
	}

	public Stone(StoneType stoneType)
	{
		_type = stoneType;
	}

	public static implicit operator StoneType(Stone stone)
	{
		return stone._type;
	}

	public static implicit operator Stone(StoneType stoneType)
	{
		return new Stone(stoneType);
	}
}
