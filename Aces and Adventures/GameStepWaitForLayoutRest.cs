using System.Collections;

public class GameStepWaitForLayoutRest : GameStep
{
	public ACardLayout layout;

	protected override bool shouldBeCanceled => !layout;

	public GameStepWaitForLayoutRest(ACardLayout layout)
	{
		this.layout = layout;
	}

	protected override IEnumerator Update()
	{
		while (!layout.IsAtRest())
		{
			yield return null;
		}
	}
}
