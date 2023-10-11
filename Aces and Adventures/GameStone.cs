using ProtoBuf;

[ProtoContract]
public class GameStone : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Draw,
		Select,
		Discard
	}

	[ProtoMember(1)]
	private DataRef<GameData> _game;

	public DataRef<GameData> game => _game;

	private bool _gameSpecified => _game.ShouldSerialize();

	private GameStone()
	{
	}

	public GameStone(DataRef<GameData> game)
	{
		_game = game;
	}
}
