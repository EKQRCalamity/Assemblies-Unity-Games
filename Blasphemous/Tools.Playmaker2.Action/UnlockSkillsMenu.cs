using System.Collections;
using Gameplay.UI;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Show a Unlock skills UI")]
public class UnlockSkillsMenu : FsmStateAction
{
	private Coroutine routine;

	public override void OnEnter()
	{
		routine = StartCoroutine(ShowMenu());
	}

	private IEnumerator ShowMenu()
	{
		yield return UIController.instance.ShowUnlockSKillCourrutine();
		Finish();
	}

	public override void OnExit()
	{
		StopCoroutine(routine);
	}
}
