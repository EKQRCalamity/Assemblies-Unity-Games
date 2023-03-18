using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class BodyChainLink : MonoBehaviour
{
	public Transform targetLink;

	public Transform previousLink;

	public float distance = 1f;

	public bool fixedPoint;

	public bool fixedRotation;

	private SpriteRenderer spr;

	public float secAnimationDuration = 1f;

	public float secAnimationAmplitude = 1f;

	public float syncOffset;

	private Transform spriteChild;

	private bool animationStarted;

	private float animationCounter;

	public float rotationDamp = 0.5f;

	public float limitAngle = 30f;

	[FoldoutGroup("Debug", 0)]
	public float lastAngle;

	public float baseAngle;

	private void Awake()
	{
		spr = GetComponentInChildren<SpriteRenderer>();
		spriteChild = spr.transform;
	}

	private void Update()
	{
		if (!animationStarted)
		{
			if (animationCounter >= syncOffset)
			{
				SecondaryAnimation();
				animationStarted = true;
			}
			animationCounter += Time.deltaTime;
		}
	}

	private void SecondaryAnimation()
	{
		spriteChild.transform.DOLocalMoveY(secAnimationAmplitude, secAnimationDuration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
	}

	public void UpdateChainLink()
	{
		if (!fixedPoint || !fixedRotation)
		{
			Vector2 normalized = ((Vector2)targetLink.position - (Vector2)base.transform.position).normalized;
			float z = 57.29578f * Mathf.Atan2(normalized.y, normalized.x);
			if (!fixedRotation)
			{
				base.transform.rotation = Quaternion.Euler(0f, 0f, z);
			}
			if (!fixedPoint && Vector2.Distance(targetLink.position, base.transform.position) > distance)
			{
				Vector2 vector = (Vector2)targetLink.position - normalized * distance;
				base.transform.position = vector;
			}
		}
	}

	public void UpdateBackwardsChainLink()
	{
		if (!fixedPoint || !fixedRotation)
		{
			Vector2 normalized = ((Vector2)previousLink.position - (Vector2)base.transform.position).normalized;
			float z = 180f + 57.29578f * Mathf.Atan2(normalized.y, normalized.x);
			if (!fixedRotation)
			{
				base.transform.rotation = Quaternion.Euler(0f, 0f, z);
			}
			if (!fixedPoint && Vector2.Distance(previousLink.position, base.transform.position) > distance)
			{
				Vector2 vector = (Vector2)previousLink.position - normalized * distance * 0.9f;
				base.transform.position = vector;
			}
		}
	}

	public void ReverseUpdateChainLink()
	{
		Vector2 normalized = ((Vector2)targetLink.position - (Vector2)base.transform.position).normalized;
		float f = 57.29578f * Mathf.Atan2(normalized.y, normalized.x);
		float num = Mathf.Sign(f);
		bool flag = false;
		if (fixedPoint)
		{
			flag = true;
		}
		else
		{
			flag = ClampedLookAt(targetLink.position);
		}
		Vector3 vector = base.transform.position + base.transform.right * distance;
		targetLink.parent.position = vector - targetLink.localPosition;
	}

	public bool ClampedLookAt(Vector2 point)
	{
		bool flag = false;
		float num = 57.29578f * Mathf.Atan2(point.y, point.x);
		float num2 = Mathf.Sign(num - baseAngle);
		float num3 = Mathf.Abs(num - baseAngle);
		if (num3 > limitAngle)
		{
			flag = true;
		}
		Quaternion b = Quaternion.Euler(0f, 0f, (!flag) ? num : (baseAngle + num2 * limitAngle));
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, rotationDamp);
		return flag;
	}

	public void FlipSprite(bool flip)
	{
		spr.flipY = flip;
	}

	public void Freeze(bool state)
	{
		fixedPoint = state;
		fixedRotation = state;
	}

	private void OnDrawGizmosSelected()
	{
		if ((bool)targetLink)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(base.transform.position, targetLink.position);
			Gizmos.color = Color.green;
			Vector3 vector = (targetLink.position - base.transform.position).normalized * distance;
			Gizmos.DrawLine(base.transform.position, base.transform.position + vector);
		}
	}

	public float DistanceToPoint()
	{
		return Vector2.Distance(targetLink.position, base.transform.position);
	}
}
