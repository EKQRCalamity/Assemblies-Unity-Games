using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Returns the number of rescued cherubs")]
public class CheckRescuedCherubs : FsmStateAction
{
	public FsmInt maxCherubs;

	public FsmInt output;

	public override void OnEnter()
	{
		base.OnEnter();
		int value = CountRescuedCherubs(maxCherubs.Value);
		output.Value = value;
		Finish();
	}

	private int CountRescuedCherubs(int max)
	{
		int num = 0;
		for (int i = 0; i < max; i++)
		{
			string id = string.Format("RESCUED_CHERUB_{0}", (i + 1).ToString("00"));
			if (Core.Events.GetFlag(id))
			{
				num++;
			}
		}
		return num;
	}
}
