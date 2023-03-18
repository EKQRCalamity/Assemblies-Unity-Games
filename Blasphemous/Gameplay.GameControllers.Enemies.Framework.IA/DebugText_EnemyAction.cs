using System.Collections;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class DebugText_EnemyAction : EnemyAction
{
	private string text;

	public EnemyAction StartAction(EnemyBehaviour e, string _text)
	{
		text = _text;
		return StartAction(e);
	}

	protected override void DoOnStart()
	{
		base.DoOnStart();
		Debug.Log("<color=blue>DebugText_EA:" + text + "</color>");
	}

	protected override IEnumerator BaseCoroutine()
	{
		yield return null;
		FinishAction();
	}
}
