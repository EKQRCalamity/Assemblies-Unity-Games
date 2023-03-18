using System;
using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Snake;

public class SnakeSegmentsMovementController : MonoBehaviour
{
	public enum STAGES
	{
		OUT = 4,
		FLOOR = 0,
		STAGE_ONE = 1,
		STAGE_TWO = 2
	}

	[Serializable]
	public struct SnakeSegmentsStageInfo
	{
		public STAGES Stage;

		public CameraNumericBoundaries CamBounds;

		public List<GameObject> PositionMarkers;

		public GameObject BattleBoundsCenterMarker;

		public List<float> DelaysToStartMoving;
	}

	[Serializable]
	public struct RainInfo
	{
		public ParticleSystem RainSystem;

		public float BaseRate;
	}

	public SnakeBehaviour SnakeBehaviour;

	public List<SnakeSegmentVisualController> SnakeSegments = new List<SnakeSegmentVisualController>();

	public List<SnakeSegmentsStageInfo> StagesInfo = new List<SnakeSegmentsStageInfo>();

	public float TweeningTime = 1f;

	public Ease TweeningEase = Ease.InQuad;

	public STAGES CurrentStage = STAGES.OUT;

	public List<RainInfo> RainsInfo = new List<RainInfo>();

	public Transform RainParticlesRoot;

	private ProCamera2DNumericBoundaries boundaries;

	private bool goingUp;

	private bool goingDown;

	private Coroutine curCoroutine;

	private void Start()
	{
		boundaries = UnityEngine.Camera.main.GetComponent<ProCamera2DNumericBoundaries>();
		if (CurrentStage != STAGES.OUT)
		{
			UpdateSnakeSegmentsPosition(movingUpwards: false, STAGES.OUT);
		}
	}

	private void Update()
	{
		if (goingUp)
		{
			float x = Core.Logic.Penitent.GetPosition().x;
			float t = Mathf.InverseLerp(SnakeBehaviour.SnakeLeftCorner.position.x, SnakeBehaviour.SnakeRightCorner.position.x, x);
			float num = Mathf.Lerp(0f, 3f, t);
			ProCamera2D.Instance.ApplyInfluence(Vector2.up * num);
			boundaries.UseTopBoundary = false;
		}
		else if (goingDown && CurrentStage != 0)
		{
			boundaries.UseBottomBoundary = false;
		}
	}

	[Button(ButtonSizes.Small)]
	public void MoveToNextStage()
	{
		switch (CurrentStage)
		{
		case STAGES.OUT:
			UpdateSnakeSegmentsPosition(movingUpwards: true, STAGES.FLOOR);
			break;
		case STAGES.FLOOR:
			UpdateSnakeSegmentsPosition(movingUpwards: true, STAGES.STAGE_ONE);
			break;
		case STAGES.STAGE_ONE:
			UpdateSnakeSegmentsPosition(movingUpwards: true, STAGES.STAGE_TWO);
			break;
		case STAGES.STAGE_TWO:
			break;
		case (STAGES)3:
			break;
		}
	}

	[Button(ButtonSizes.Small)]
	public void MoveToPrevStage()
	{
		switch (CurrentStage)
		{
		case STAGES.OUT:
			break;
		case STAGES.FLOOR:
			UpdateSnakeSegmentsPosition(movingUpwards: false, STAGES.OUT);
			break;
		case STAGES.STAGE_ONE:
			UpdateSnakeSegmentsPosition(movingUpwards: false, STAGES.FLOOR);
			break;
		case STAGES.STAGE_TWO:
			UpdateSnakeSegmentsPosition(movingUpwards: false, STAGES.STAGE_ONE);
			break;
		case (STAGES)3:
			break;
		}
	}

	public void ChangeSegmentsAnim(bool movingUpwards)
	{
		for (int i = 0; i < SnakeSegments.Count; i++)
		{
			SnakeSegmentVisualController snakeSegmentVisualController = SnakeSegments[i];
			if (movingUpwards)
			{
				snakeSegmentVisualController.MoveUp();
			}
			else
			{
				snakeSegmentVisualController.MoveDown();
			}
		}
	}

	private IEnumerator WaitToSetCamBoundsGoingUp(SnakeSegmentsStageInfo targetStageInfo)
	{
		float centerY = Mathf.Lerp(targetStageInfo.CamBounds.TopBoundary, targetStageInfo.CamBounds.BottomBoundary, 0.5f);
		yield return new WaitUntil(() => UnityEngine.Camera.main.transform.position.y > centerY);
		targetStageInfo.CamBounds.SetBoundaries();
		goingUp = false;
	}

	private IEnumerator WaitToSetCamBoundsGoingDown(SnakeSegmentsStageInfo targetStageInfo)
	{
		float centerY = Mathf.Lerp(targetStageInfo.CamBounds.TopBoundary, targetStageInfo.CamBounds.BottomBoundary, 0.5f);
		yield return new WaitUntil(() => UnityEngine.Camera.main.transform.position.y < centerY);
		targetStageInfo.CamBounds.SetBoundaries();
		goingDown = false;
	}

	private void UpdateSnakeSegmentsPosition(bool movingUpwards, STAGES targetStage)
	{
		SnakeSegmentsStageInfo targetStageInfo = StagesInfo.Find((SnakeSegmentsStageInfo x) => x.Stage == targetStage);
		if (Application.isPlaying)
		{
			SetCamCoroutine(targetStageInfo, movingUpwards);
		}
		UpdateRain(targetStageInfo, movingUpwards);
		for (int i = 0; i < SnakeSegments.Count; i++)
		{
			if (Application.isPlaying)
			{
				Tween t = MoveSnakeSegment(targetStageInfo, movingUpwards, i);
				if (i != 0)
				{
					continue;
				}
				if (targetStage != 0)
				{
					SnakeBehaviour.Snake.SnakeAnimatorInyector.BackgroundAnimationSetSpeed(2f, 1f);
				}
				t.OnComplete(delegate
				{
					if (curCoroutine != null)
					{
						StopCoroutine(curCoroutine);
					}
					if (targetStage != 0)
					{
						SnakeBehaviour.Snake.SnakeAnimatorInyector.BackgroundAnimationSetSpeed(1f, 1f);
					}
					goingUp = false;
					goingDown = false;
					targetStageInfo.CamBounds.SetBoundaries();
					CurrentStage = targetStage;
					SnakeBehaviour.BattleBounds.position = targetStageInfo.BattleBoundsCenterMarker.transform.position;
				});
			}
			else
			{
				float num = targetStageInfo.PositionMarkers[i].transform.position.y - SnakeSegments[i].transform.position.y;
				SnakeSegments[i].transform.position += Vector3.up * num;
				CurrentStage = targetStage;
				SnakeBehaviour.BattleBounds.position = targetStageInfo.BattleBoundsCenterMarker.transform.position;
			}
		}
	}

	private Tween MoveSnakeSegment(SnakeSegmentsStageInfo targetStageInfo, bool movingUpwards, int i)
	{
		SnakeSegmentVisualController segment = SnakeSegments[i];
		Tween tween = segment.transform.DOMoveY(targetStageInfo.PositionMarkers[i].transform.position.y, TweeningTime).SetEase(TweeningEase).SetDelay(targetStageInfo.DelaysToStartMoving[i]);
		if (movingUpwards)
		{
			tween.OnPlay(delegate
			{
				segment.MoveUp();
			});
		}
		else
		{
			tween.OnPlay(delegate
			{
				segment.MoveDown();
			});
		}
		return tween;
	}

	public void InstantSetCamAsStart()
	{
		StagesInfo.Find((SnakeSegmentsStageInfo x) => x.Stage == STAGES.OUT).CamBounds.SetBoundaries();
	}

	private void SetCamCoroutine(SnakeSegmentsStageInfo targetStageInfo, bool movingUpwards)
	{
		if (curCoroutine != null)
		{
			StopCoroutine(curCoroutine);
		}
		if (movingUpwards)
		{
			goingUp = true;
			curCoroutine = StartCoroutine(WaitToSetCamBoundsGoingUp(targetStageInfo));
		}
		else
		{
			goingDown = true;
			curCoroutine = StartCoroutine(WaitToSetCamBoundsGoingDown(targetStageInfo));
		}
	}

	private void UpdateRain(SnakeSegmentsStageInfo targetStageInfo, bool movingUpwards)
	{
		Tween tween = RainParticlesRoot.DOMoveY(targetStageInfo.CamBounds.TopBoundary, TweeningTime).SetEase(TweeningEase).SetDelay(targetStageInfo.DelaysToStartMoving[0]);
		foreach (RainInfo item in RainsInfo)
		{
			ParticleSystem.EmissionModule emission = item.RainSystem.emission;
			float rateOverTimeMultiplier = emission.rateOverTimeMultiplier;
			Tween t = DOTween.To(() => emission.rateOverTimeMultiplier, delegate(float x)
			{
				emission.rateOverTimeMultiplier = x;
			}, (!movingUpwards) ? (rateOverTimeMultiplier * 0.5f) : (rateOverTimeMultiplier * 2f), TweeningTime);
			t.SetEase(TweeningEase).SetDelay(targetStageInfo.DelaysToStartMoving[0]);
		}
	}

	public void ResetRain()
	{
		RainParticlesRoot.transform.position = new Vector3(RainParticlesRoot.transform.position.x, StagesInfo[0].CamBounds.TopBoundary, RainParticlesRoot.transform.position.z);
		foreach (RainInfo item in RainsInfo)
		{
			ParticleSystem.EmissionModule emission = item.RainSystem.emission;
			emission.rateOverTimeMultiplier = item.BaseRate;
		}
	}
}
