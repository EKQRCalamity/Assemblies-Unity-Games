using Framework.Managers;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Respawn from current Miriam course.")]
public class RespawnMiriam : FsmStateAction
{
	public FsmBool UseFade;

	public FsmColor FadeColor;

	public override void Reset()
	{
		UseFade = new FsmBool();
		UseFade.UseVariable = false;
		UseFade.Value = true;
		FadeColor = new FsmColor();
		FadeColor.UseVariable = false;
		FadeColor.Value = Color.white;
	}

	public override void OnEnter()
	{
		bool useFade = UseFade == null || UseFade.Value;
		Color value = ((FadeColor == null) ? Color.black : FadeColor.Value);
		Core.SpawnManager.RespawnMiriamSameLevel(useFade, value);
		Finish();
	}
}
