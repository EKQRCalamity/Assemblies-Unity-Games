using DG.Tweening;
using Framework.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class MoveOnStart : PoolObject
{
	[Header("Motion Params")]
	[Tooltip("The transform used to move the game object")]
	[SerializeField]
	private Transform motionObject;

	[Tooltip("The time taken to move from the start to finish positions during the movement")]
	public float MoveDuration;

	[Tooltip("Should the horizontal movement direction flip according to the Sprite Renderer?")]
	public bool FlipHorizontalDirectionByRenderer;

	[Tooltip("Should the vertical movement direction flip according to the Sprite Renderer?")]
	public bool FlipVerticalDirectionByRenderer;

	[ShowIf("showSpriteRendererAttribute", true)]
	[Tooltip("SpriteRenderer that is checked in order to flip the movement directions")]
	public SpriteRenderer SpriteRenderer;

	[Tooltip("How far the object should move when along the X axis")]
	public float HorizontalDistanceToMove;

	[Tooltip("Should the horizontal movement use a custom Animation Curve or a predefined Ease?")]
	public bool UseAnimationCurveForHorizontalMovement;

	[HideIf("UseAnimationCurveForHorizontalMovement", true)]
	[Tooltip("The Ease to use in the horizontal movement")]
	public Ease HorizontalMovementEase;

	[ShowIf("UseAnimationCurveForHorizontalMovement", true)]
	[Tooltip("The AnimationCurve to use in the horizontal movement")]
	public AnimationCurve HorizontalMovementAnimationCurve;

	[Tooltip("How far the object should move when along the Y axis")]
	public float VerticalDistanceToMove;

	[Tooltip("Should the vertical movement use a custom animation curve or a predefined ease?")]
	public bool UseAnimationCurveForVerticalMovement;

	[HideIf("UseAnimationCurveForVerticalMovement", true)]
	[Tooltip("The Ease to use in the vertical movement")]
	public Ease VerticalMovementEase;

	[ShowIf("UseAnimationCurveForVerticalMovement", true)]
	[Tooltip("The AnimationCurve to use in the horizontal movement")]
	public AnimationCurve VerticalMovementAnimationCurve;

	private Vector2 startPos;

	private bool showSpriteRendererAttribute => FlipHorizontalDirectionByRenderer || FlipVerticalDirectionByRenderer;

	private void Start()
	{
		startPos = motionObject.localPosition;
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		motionObject.localPosition = startPos;
		if (FlipHorizontalDirectionByRenderer && SpriteRenderer.flipX)
		{
			HorizontalDistanceToMove = 0f - HorizontalDistanceToMove;
		}
		if (FlipVerticalDirectionByRenderer && SpriteRenderer.flipY)
		{
			VerticalDistanceToMove = 0f - VerticalDistanceToMove;
		}
		float endValue = motionObject.position.x + HorizontalDistanceToMove;
		if (UseAnimationCurveForHorizontalMovement)
		{
			motionObject.DOMoveX(endValue, MoveDuration).SetEase(HorizontalMovementAnimationCurve);
		}
		else
		{
			motionObject.DOMoveX(endValue, MoveDuration).SetEase(HorizontalMovementEase);
		}
		float endValue2 = motionObject.position.y + VerticalDistanceToMove;
		if (UseAnimationCurveForVerticalMovement)
		{
			motionObject.DOMoveY(endValue2, MoveDuration).SetEase(VerticalMovementAnimationCurve);
		}
		else
		{
			motionObject.DOMoveY(endValue2, MoveDuration).SetEase(VerticalMovementEase);
		}
	}
}
