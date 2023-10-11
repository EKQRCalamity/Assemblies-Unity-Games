using System.Collections;

public abstract class AGameStepDeck : GameStep
{
	protected virtual void _OnBackPressed()
	{
	}

	protected override void OnEnable()
	{
		base.view.onBackPressed += _OnBackPressed;
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		base.view.onBackPressed -= _OnBackPressed;
	}
}
