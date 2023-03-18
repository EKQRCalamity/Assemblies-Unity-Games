using Framework.Managers;
using Framework.Penitences;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Checks current Penitence and raises that event.")]
public class PenitenceCheckCurrent : FsmStateAction
{
	public FsmEvent noPenitenceActive;

	public FsmEvent penitencePE01Active;

	public FsmEvent penitencePE02Active;

	public FsmEvent penitencePE03Active;

	public override void OnEnter()
	{
		IPenitence currentPenitence = Core.PenitenceManager.GetCurrentPenitence();
		if (currentPenitence == null)
		{
			base.Fsm.Event(noPenitenceActive);
		}
		else if (currentPenitence is PenitencePE01)
		{
			base.Fsm.Event(penitencePE01Active);
		}
		else if (currentPenitence is PenitencePE02)
		{
			base.Fsm.Event(penitencePE02Active);
		}
		else if (currentPenitence is PenitencePE03)
		{
			base.Fsm.Event(penitencePE03Active);
		}
		else
		{
			Debug.LogError("Current active penitence is not one of the first three!");
		}
		Finish();
	}
}
