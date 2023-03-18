using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using HutongGames.PlayMaker;
using Tools.Playmaker2.Action;

namespace Tools.PlayMaker.Action;

[ActionCategory("Blasphemous Action")]
public class BlockDash : FsmStateAction
{
	private Penitent penitent;

	private DialogStart dialogStart;

	private bool startdialog;

	public override void OnEnter()
	{
		penitent = Core.Logic.Penitent;
	}

	public override void OnExit()
	{
		penitent.Dash.StopCast();
		Finish();
	}
}
