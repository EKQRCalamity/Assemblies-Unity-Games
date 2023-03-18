using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Ends Demake Run.")]
public class EndDemakeRun : FsmStateAction
{
	public FsmInt numSpecialItems;

	public override void OnEnter()
	{
		Core.DemakeManager.EndDemakeRun(completed: true, numSpecialItems.Value);
		Finish();
	}
}
