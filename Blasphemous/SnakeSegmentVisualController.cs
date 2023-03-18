using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class SnakeSegmentVisualController : MonoBehaviour
{
	public enum STATES
	{
		IDLE,
		MOVING_UP,
		MOVING_DOWN
	}

	[Serializable]
	public struct SnakeSegmentVisualStateInfo
	{
		public STATES State;

		public float TweeningDelay;

		public float TweeningTime;

		public Ease TweeningEase;

		public float SpriteWidthAtStart;

		public float SpriteWidthAtEnd;

		public bool ShouldLoopTween;

		[ShowIf("ShouldLoopTween", true)]
		public int NumLoops;

		[ShowIf("ShouldLoopTween", true)]
		public LoopType LoopType;
	}

	public SpriteRenderer SpriteRenderer;

	public List<SnakeSegmentVisualStateInfo> StatesInfo = new List<SnakeSegmentVisualStateInfo>();

	private STATES currentState;

	private Tween activeTween;

	public bool IsIdle => currentState == STATES.IDLE;

	public bool IsMovingUp => currentState == STATES.MOVING_UP;

	public bool IsMovingDown => currentState == STATES.MOVING_DOWN;

	public void Start()
	{
		GoToIdle();
	}

	[HideIf("IsIdle", true)]
	[Button(ButtonSizes.Small)]
	public void GoToIdle()
	{
		currentState = STATES.IDLE;
		UpdateSnakeSegmentVisually(goesToIdle: false);
	}

	[ShowIf("IsIdle", true)]
	[Button(ButtonSizes.Small)]
	public void MoveUp()
	{
		currentState = STATES.MOVING_UP;
		UpdateSnakeSegmentVisually(goesToIdle: true);
	}

	[ShowIf("IsIdle", true)]
	[Button(ButtonSizes.Small)]
	public void MoveDown()
	{
		currentState = STATES.MOVING_DOWN;
		UpdateSnakeSegmentVisually(goesToIdle: true);
	}

	private void UpdateSnakeSegmentVisually(bool goesToIdle)
	{
		SnakeSegmentVisualStateInfo snakeSegmentVisualStateInfo = StatesInfo.Find((SnakeSegmentVisualStateInfo x) => x.State == currentState);
		if (!Application.isPlaying)
		{
			return;
		}
		activeTween.Kill();
		activeTween = DOTween.To(delegate(float x)
		{
			SpriteRenderer.size = new Vector2(x, SpriteRenderer.size.y);
		}, snakeSegmentVisualStateInfo.SpriteWidthAtStart, snakeSegmentVisualStateInfo.SpriteWidthAtEnd, snakeSegmentVisualStateInfo.TweeningTime).SetEase(snakeSegmentVisualStateInfo.TweeningEase).SetDelay(snakeSegmentVisualStateInfo.TweeningDelay);
		if (goesToIdle)
		{
			activeTween.OnComplete(delegate
			{
				GoToIdle();
			});
		}
		else if (snakeSegmentVisualStateInfo.ShouldLoopTween)
		{
			activeTween.SetLoops(snakeSegmentVisualStateInfo.NumLoops, snakeSegmentVisualStateInfo.LoopType);
		}
	}
}
