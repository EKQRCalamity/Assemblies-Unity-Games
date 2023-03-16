using UnityEngine;

public class PlatformingLevelWinAnimation : AbstractLevelHUDComponent
{
	public enum State
	{
		Paused,
		Unpaused,
		Complete
	}

	private const float FRAME_DELAY = 5f;

	public State CurrentState { get; private set; }

	public static PlatformingLevelWinAnimation Create()
	{
		return Object.Instantiate(Level.Current.LevelResources.platformingWin);
	}

	protected override void Awake()
	{
		base.Awake();
		_parentToHudCanvas = true;
	}

	private void OnAnimComplete()
	{
		CurrentState = State.Complete;
		Object.Destroy(base.gameObject);
	}
}
