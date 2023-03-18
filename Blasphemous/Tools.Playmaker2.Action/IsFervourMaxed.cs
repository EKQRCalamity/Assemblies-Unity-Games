using Framework.Managers;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Get")]
public class IsFervourMaxed : FsmStateAction
{
	public FsmBool output;

	public override void Reset()
	{
		output = new FsmBool
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		output.Value = Core.Logic.Penitent.Stats.Fervour.Current == Core.Logic.Penitent.Stats.Fervour.CurrentMax;
		Debug.Log($"Fervour:  {Core.Logic.Penitent.Stats.Fervour.Current} Max: {Core.Logic.Penitent.Stats.Fervour.CurrentMax}");
		Finish();
	}
}
