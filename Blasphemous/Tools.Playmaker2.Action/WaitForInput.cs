using HutongGames.PlayMaker;
using Rewired;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Pauses the execution of the FSM until certain code is received from ReWired.")]
public class WaitForInput : FsmStateAction
{
	[Tooltip("The name of the GameObject to find. You can leave this empty if you specify a Tag.")]
	public FsmString ReWiredCode;

	private Player player;

	public override void OnEnter()
	{
		player = ReInput.players.GetPlayer(0);
	}

	public override void OnUpdate()
	{
		if (player != null && player.GetButtonDown(ReWiredCode.Value))
		{
			player = null;
			Finish();
		}
	}
}
