public class GameStepStateChange : GameStep
{
	private AStateView _state;

	private bool _enabled;

	public GameStepStateChange(AStateView state, bool enabled = true)
	{
		_state = state;
		_enabled = enabled;
	}

	public override void Start()
	{
		_state.enabled = _enabled;
	}
}
