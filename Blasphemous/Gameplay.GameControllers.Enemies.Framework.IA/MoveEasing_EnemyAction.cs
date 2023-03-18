using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class MoveEasing_EnemyAction : EnemyAction
{
	private float seconds;

	private Ease easingCurve;

	private Transform transformToMove;

	private Vector2 point;

	private bool timeScaled;

	private Action doOnTweenUpdate;

	private Tween t;

	private bool tweenOnX;

	private bool tweenOnY;

	private float overShootOrAmplitude;

	public EnemyAction StartAction(EnemyBehaviour e, Vector2 _target, float _seconds, Ease _easing, Transform _transformToMove = null, bool _timeScaled = true, Action _tweenUpdateCallback = null, bool _tweenOnX = true, bool _tweenOnY = true, float _overShootOrAmplitude = 1.7f)
	{
		point = _target;
		seconds = _seconds;
		easingCurve = _easing;
		transformToMove = _transformToMove;
		timeScaled = _timeScaled;
		doOnTweenUpdate = _tweenUpdateCallback;
		tweenOnX = _tweenOnX;
		tweenOnY = _tweenOnY;
		overShootOrAmplitude = _overShootOrAmplitude;
		return StartAction(e);
	}

	public EnemyAction StartAction(EnemyBehaviour e, float distance, float _seconds, Ease _easing, float _overShootOrAmplitude = 1.7f)
	{
		point = (Vector2)e.transform.position + new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized * distance;
		Vector2 target = point;
		return StartAction(e, target, _seconds, _easing, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: true, _overShootOrAmplitude);
	}

	protected override void DoOnStart()
	{
		base.DoOnStart();
	}

	protected override void DoOnStop()
	{
		t.Kill();
		base.DoOnStop();
	}

	protected override IEnumerator BaseCoroutine()
	{
		Transform transform = transformToMove ?? owner.transform;
		if (tweenOnX && tweenOnY)
		{
			t = transform.DOMove(point, seconds).SetEase(easingCurve);
		}
		else if (tweenOnX && !tweenOnY)
		{
			t = transform.DOMoveX(point.x, seconds).SetEase(easingCurve);
		}
		else if (!tweenOnX && tweenOnY)
		{
			t = transform.DOMoveY(point.y, seconds).SetEase(easingCurve);
		}
		else
		{
			Debug.LogError("MoveEasing_EnemyAction::BaseCoroutine: tweenOnX or tweenOnY should be set to true!");
		}
		t.easeOvershootOrAmplitude = overShootOrAmplitude;
		if (doOnTweenUpdate != null)
		{
			t.OnUpdate(delegate
			{
				doOnTweenUpdate();
			});
		}
		t.SetUpdate(!timeScaled);
		yield return t.WaitForCompletion();
		FinishAction();
	}
}
