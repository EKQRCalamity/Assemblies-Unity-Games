using ProtoBuf;

[ProtoContract]
public class LevelUpLeaf : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum State
	{
		Old,
		New
	}

	[ProtoMember(1)]
	private State _state;

	public State state => _state;

	public LevelUpLeafView leafView => base.view as LevelUpLeafView;

	private LevelUpLeaf()
	{
	}

	public LevelUpLeaf(State state = State.Old)
	{
		_state = state;
	}
}
