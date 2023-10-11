using ProtoBuf;

[ProtoContract]
public class Leaderboard : ATarget
{
	[ProtoMember(1)]
	private DataRef<AdventureData> _adventure;

	[ProtoMember(2)]
	public int day;

	public DataRef<AdventureData> adventure => _adventure;

	private bool _adventureSpecified => _adventure.ShouldSerialize();

	private Leaderboard()
	{
	}

	public Leaderboard(DataRef<AdventureData> adventure, int day)
	{
		_adventure = adventure;
		this.day = day;
	}
}
