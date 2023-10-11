using System.Collections;

public class GameStepWaitFrame : GameStep
{
	private int _numberOfFrames;

	public GameStepWaitFrame(int numberOfFrames = 1)
	{
		_numberOfFrames = numberOfFrames;
	}

	protected override IEnumerator Update()
	{
		while (--_numberOfFrames >= 0)
		{
			yield return null;
		}
	}
}
