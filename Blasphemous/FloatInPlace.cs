using DG.Tweening;
using UnityEngine;

public class FloatInPlace : MonoBehaviour
{
	private Tween swayTween;

	public float loopDuration;

	public float oscillationDistance;

	private void Start()
	{
		Vector2 vector = base.transform.localPosition;
		swayTween = base.transform.DOLocalMoveY(vector.y - oscillationDistance, loopDuration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
	}
}
