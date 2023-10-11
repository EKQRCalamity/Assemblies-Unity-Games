using System.Collections;

public class GameStepStringChoice : GameStep
{
	private string[] _choices;

	private string _choice;

	public string choice => _choice;

	public GameStepStringChoice(params string[] choices)
	{
		_choices = choices;
	}

	protected override IEnumerator Update()
	{
		yield break;
	}
}
