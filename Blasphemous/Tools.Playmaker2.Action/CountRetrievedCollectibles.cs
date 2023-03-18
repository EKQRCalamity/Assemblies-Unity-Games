using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Counts the number of already retrieved collectibles.")]
public class CountRetrievedCollectibles : FsmStateAction
{
	public FsmInt output;

	public override void Reset()
	{
		output = new FsmInt
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		output.Value = OssuaryManager.CountAlreadyRetrievedCollectibles();
		Finish();
	}
}
