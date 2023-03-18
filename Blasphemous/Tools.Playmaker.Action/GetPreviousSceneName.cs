using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.PlayMaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Gets the name of the previous scene.")]
public class GetPreviousSceneName : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmString storeName;

	public override void Reset()
	{
		storeName = null;
	}

	public override void OnEnter()
	{
		storeName.Value = Core.LevelManager.GetLastSceneName();
		Finish();
	}
}
