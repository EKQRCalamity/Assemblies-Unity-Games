using System;

public class GameStepGenericSimple : GameStep
{
	public Action onStart { get; set; }

	public GameStepGenericSimple()
	{
	}

	public GameStepGenericSimple(Action start)
	{
		onStart = start;
	}

	public override void Start()
	{
		onStart?.Invoke();
	}
}
