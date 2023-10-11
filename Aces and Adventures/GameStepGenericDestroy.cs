using System;

public class GameStepGenericDestroy : GameStep
{
	private readonly Action _onDestroy;

	private readonly Action _onCancel;

	public GameStepGenericDestroy(Action destroy, Action cancel = null)
	{
		_onDestroy = destroy;
		_onCancel = cancel;
	}

	protected override void OnCanceled()
	{
		_onCancel?.Invoke();
	}

	protected override void OnDestroy()
	{
		_onDestroy?.Invoke();
	}
}
