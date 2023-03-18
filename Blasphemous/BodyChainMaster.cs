using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class BodyChainMaster : MonoBehaviour
{
	public enum CHAIN_UPDATE_MODES
	{
		NORMAL,
		BACKWARDS
	}

	[FoldoutGroup("Configuration", 0)]
	public List<BodyChainLink> links;

	public CHAIN_UPDATE_MODES chainMode;

	public float bobPointsRadius = 2f;

	public float bobPointDistance = 1.5f;

	public Ease bobEase;

	private Vector3 bobOrigin;

	private List<Vector2> topPoints;

	private List<Vector2> botPoints;

	private bool goUp;

	public float moveDuration = 0.5f;

	public bool IsStBobbing;

	public bool IsAttacking;

	public float secAnimationDuration = 3f;

	public float secAnimationAmplitude = 0.5f;

	public float secAnimationStep = 0.2f;

	public float maxAttackDistance = 10f;

	public float attackDuration = 1.4f;

	public SpriteRenderer spr;

	public GameObject explosionFX;

	[FoldoutGroup("Configuration", 0)]
	[Button("Auto assign links", ButtonSizes.Small)]
	public void AssignLinks()
	{
		for (int num = links.Count - 1; num >= 0; num--)
		{
			if (num < links.Count - 1)
			{
				links[num].previousLink = links[num + 1].transform.GetChild(1);
			}
			if (num > 0)
			{
				links[num].targetLink = links[num - 1].transform.GetChild(0);
				links[num].syncOffset = (float)(num + 1) * secAnimationStep;
			}
			else
			{
				links[num].targetLink = base.transform.GetChild(0);
			}
		}
	}

	[FoldoutGroup("Debug", 0)]
	[Button("Update chain", ButtonSizes.Small)]
	public void UpdateChain()
	{
		for (int i = 0; i < links.Count; i++)
		{
			links[i].UpdateChainLink();
		}
	}

	[FoldoutGroup("Debug", 0)]
	[Button("Update BACKWARDS chain", ButtonSizes.Small)]
	public void UpdateBackwardsChain()
	{
		for (int num = links.Count - 2; num >= 0; num--)
		{
			links[num].UpdateBackwardsChainLink();
		}
		base.transform.position = links[0].transform.GetChild(1).position;
		base.transform.rotation = links[0].transform.rotation;
	}

	[FoldoutGroup("Debug", 0)]
	[Button("Update reverse chain", ButtonSizes.Small)]
	public void UpdateReverseChain()
	{
		for (int num = links.Count - 1; num >= 0; num--)
		{
			links[num].ReverseUpdateChainLink();
			if (num > 1)
			{
				Vector3 right = links[num].transform.right;
				links[num - 1].baseAngle = 57.29578f * Mathf.Atan2(right.y, right.x);
			}
		}
	}

	public void Repullo()
	{
		InterruptActions();
		Vector3 vector = base.transform.position - Core.Logic.Penitent.transform.position;
		vector += Vector3.up;
		float num = 5f;
		MoveWithEase(base.transform.position + vector.normalized * num, 0.5f, Ease.OutBack);
	}

	[FoldoutGroup("Debug", 0)]
	[Button("Update chain fixed point", ButtonSizes.Small)]
	public void UpdateFixedPoint()
	{
		for (int num = links.Count - 1; num >= 0; num--)
		{
			float num2 = links[num].DistanceToPoint();
			float num3 = num2 - links[num].distance;
			if (num2 > 0f)
			{
				Vector2 vector = ((Vector2)(links[num].targetLink.position - links[num].transform.position)).normalized * num3;
				for (int i = 0; i < num; i++)
				{
					links[i].transform.position -= (Vector3)vector;
				}
				base.transform.position -= (Vector3)vector;
			}
		}
	}

	[FoldoutGroup("Debug", 0)]
	[Button("TEST INTRO ENTRY", ButtonSizes.Small)]
	public void TestEntry()
	{
		SplineFollower component = GetComponent<SplineFollower>();
		component.followActivated = true;
		component.OnMovingToNextPoint += OnNextPointOnSight;
		component.OnMovementCompleted += OnMovementCompleted;
	}

	private void Start()
	{
		PoolManager.Instance.CreatePool(explosionFX, links.Count + 1);
		SecondaryAnimation();
	}

	private void SecondaryAnimation()
	{
		spr.transform.DOLocalMoveY(secAnimationAmplitude, secAnimationDuration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
	}

	private void OnNextPointOnSight(Vector2 obj)
	{
		Vector2 vector = obj - (Vector2)base.transform.position;
		base.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vector.y, vector.x) * 57.29578f);
	}

	private void OnMovementCompleted()
	{
		SplineFollower component = GetComponent<SplineFollower>();
		component.OnMovingToNextPoint -= OnNextPointOnSight;
		component.OnMovementCompleted -= OnMovementCompleted;
		int num = 5;
		for (int i = num; i < links.Count; i++)
		{
			links[i].Freeze(state: true);
		}
	}

	public void FlipAllSprites(bool flip)
	{
		foreach (BodyChainLink link in links)
		{
			link.FlipSprite(flip);
		}
		FlipSprite(flip);
	}

	public void LookRight()
	{
		FlipAllSprites(flip: false);
	}

	public void LookLeft()
	{
		FlipAllSprites(flip: true);
	}

	private void FlipSprite(bool flip)
	{
		spr.flipY = flip;
	}

	private void Update()
	{
		switch (chainMode)
		{
		case CHAIN_UPDATE_MODES.NORMAL:
			UpdateChain();
			UpdateFixedPoint();
			break;
		case CHAIN_UPDATE_MODES.BACKWARDS:
			UpdateBackwardsChain();
			break;
		}
	}

	public void StartBob()
	{
		Vector2 vector = (Vector2)base.transform.position + Vector2.up * 3f;
		base.transform.DOMove(vector, 0.5f).SetLoops(5, LoopType.Yoyo).SetEase(Ease.InOutQuad);
	}

	public void EndBob()
	{
		base.transform.DOKill();
	}

	public List<Vector2> GeneratePointsAroundPoint(Vector2 p, float r, int n)
	{
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < n; i++)
		{
			list.Add(GenerateRandomPointInRadius(p, r));
		}
		return list;
	}

	private Vector2 GenerateRandomPointInRadius(Vector2 center, float r)
	{
		return center + new Vector2(UnityEngine.Random.Range(0f - r, r), UnityEngine.Random.Range(0f - r, r));
	}

	public Tween MoveWithEase(Vector2 targetPoint, float duration, Ease easingCurve, Action<Transform> callback = null)
	{
		return base.transform.DOMove(targetPoint, duration).SetEase(easingCurve).OnComplete(delegate
		{
			if (callback != null)
			{
				callback(base.transform);
			}
		});
	}

	public void SnakeAttack(Vector2 offset, Action callback = null)
	{
		StartCoroutine(SnakeAttackSequence(offset, callback));
	}

	private float GetMaxRange()
	{
		float num = 0f;
		for (int i = 0; i < links.Count; i++)
		{
			num += links[i].distance * 2f;
			if (links[i].fixedPoint)
			{
				break;
			}
		}
		return num + 1f;
	}

	private Vector3 GetCoilPos()
	{
		for (int i = 0; i < links.Count; i++)
		{
			if (links[i].fixedPoint)
			{
				return links[i].transform.position;
			}
		}
		return links[links.Count - 1].transform.position;
	}

	private IEnumerator SnakeAttackSequence(Vector2 offset, Action callbackBeforeAttack = null)
	{
		float anticipationTweenDuration = 0.4f;
		IsAttacking = true;
		float maxRange2 = GetMaxRange();
		maxRange2 = Mathf.Min(maxRange2, maxAttackDistance);
		Vector2 origin = base.transform.position;
		Vector3 dir = Core.Logic.Penitent.transform.position + (Vector3)offset - (Vector3)origin;
		Tween curTween2 = base.transform.DOMove(base.transform.position - dir.normalized, anticipationTweenDuration).SetEase(Ease.OutCubic);
		yield return curTween2.WaitForCompletion();
		callbackBeforeAttack?.Invoke();
		curTween2 = base.transform.DOPunchPosition(dir.normalized * maxRange2, attackDuration, 2, 0.1f);
		yield return curTween2.WaitForCompletion();
		IsAttacking = false;
	}

	public void StartDeathSequence()
	{
		InterruptActions();
		StartCoroutine(DeathSequence());
	}

	private IEnumerator DeathSequence()
	{
		yield return new WaitForSeconds(0.2f);
		PoolManager.Instance.ReuseObject(explosionFX, base.transform.position, Quaternion.identity);
		base.transform.GetComponentInChildren<SpriteRenderer>().enabled = false;
		yield return new WaitForSeconds(0.2f);
		for (int i = 0; i < links.Count; i++)
		{
			PoolManager.Instance.ReuseObject(explosionFX, links[i].transform.position, Quaternion.identity);
			links[i].GetComponentInChildren<SpriteRenderer>().enabled = false;
			links[i].GetComponentInChildren<Collider2D>().enabled = false;
			Core.Logic.CameraManager.ProCamera2DShake.Shake(0.2f, Vector3.down * 2f, 4, 0.2f, 0f);
			yield return new WaitForSeconds(0.2f);
		}
		DestroyAll();
	}

	private void DestroyAll()
	{
		for (int i = 0; i < links.Count; i++)
		{
			UnityEngine.Object.Destroy(links[i].gameObject);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void InterruptActions()
	{
		StopAllCoroutines();
		base.transform.DOKill();
		IsAttacking = false;
	}

	public void LookAtTarget(Vector3 point, float rotationSpeedFactor = 5f)
	{
		Vector2 vector = point - base.transform.position;
		Quaternion b = Quaternion.Euler(0f, 0f, 57.29578f * Mathf.Atan2(vector.y, vector.x));
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, rotationSpeedFactor * Time.deltaTime);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position + Vector3.up * bobPointDistance, bobPointsRadius);
		Gizmos.DrawWireSphere(base.transform.position + Vector3.down * bobPointDistance, bobPointsRadius);
		if (topPoints != null)
		{
			for (int i = 0; i < topPoints.Count; i++)
			{
				Gizmos.DrawWireSphere(topPoints[i], 0.05f);
			}
		}
		if (botPoints != null)
		{
			for (int j = 0; j < botPoints.Count; j++)
			{
				Gizmos.DrawWireSphere(botPoints[j], 0.05f);
			}
		}
	}

	public void AffixBody(bool affix, int affixIndex = 9)
	{
		links[affixIndex].fixedPoint = affix;
	}

	public void ForceStopAttack()
	{
		IsAttacking = false;
		StopAllCoroutines();
		DOTween.Kill(base.gameObject);
	}
}
