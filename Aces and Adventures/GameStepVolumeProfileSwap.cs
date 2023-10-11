using UnityEngine.Rendering;

public class GameStepVolumeProfileSwap : GameStep
{
	private Volume _start;

	private Volume _end;

	public GameStepVolumeProfileSwap(Volume start, Volume end)
	{
		_start = start;
		_end = end;
	}

	public override void Start()
	{
		_start.weight = 0f;
		_end.weight = 1f;
	}
}
