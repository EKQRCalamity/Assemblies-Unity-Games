public abstract class PlayerEvent<T> : GameEvent where T : PlayerEvent<T>, new()
{
	public static T _instance;

	public PlayerId playerId { get; private set; }

	public PlayerEvent()
	{
	}

	public static T Shared(PlayerId playerId)
	{
		if (_instance == null)
		{
			_instance = new T();
		}
		_instance.playerId = playerId;
		return _instance;
	}
}
