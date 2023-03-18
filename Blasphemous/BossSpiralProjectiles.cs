using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Projectiles;
using Sirenix.OdinInspector;
using UnityEngine;

public class BossSpiralProjectiles : MonoBehaviour
{
	[Header("Design")]
	public float initialRadius = 3f;

	public float finalRadius = 20f;

	public float maxAngularSpeed = 180f;

	public float atkDuration = 4f;

	public float extensionTime = 1f;

	public Ease radiusGrowthEase;

	public Ease angularSpeedEase;

	[Header("References")]
	public Transform spinningTransform;

	public GameObject projectilePrefab;

	private float currentRadius;

	private List<Transform> dummies;

	private List<GameObject> currentProjectiles;

	private int numberOfProjectiles;

	private const int MAX_PROJECTILES = 20;

	[Header("Debug")]
	public int testAttackNumber = 8;

	public float testAttackDuration = 2f;

	public float testExtensionTime = 2f;

	private Tween radiusTween;

	private Tween speedTween;

	private void Start()
	{
		dummies = new List<Transform>();
		currentProjectiles = new List<GameObject>();
		for (int i = 0; i < 20; i++)
		{
			GameObject gameObject = new GameObject($"SpinningDummy_{i}");
			gameObject.transform.SetParent(spinningTransform);
			dummies.Add(gameObject.transform);
			gameObject.SetActive(value: false);
		}
		PoolManager.Instance.CreatePool(projectilePrefab, 20);
	}

	[Button("TEST ATTACK", ButtonSizes.Small)]
	private void TestAttack()
	{
		ActivateAttack(testAttackNumber, testAttackDuration, testExtensionTime);
	}

	public void ActivateAttack(int numberOfProjectiles, float atkDuration, float extensionTime)
	{
		this.atkDuration = atkDuration;
		this.extensionTime = extensionTime;
		this.numberOfProjectiles = numberOfProjectiles;
		spinningTransform.rotation = Quaternion.identity;
		if (speedTween != null)
		{
			speedTween.Kill();
		}
		if (radiusTween != null)
		{
			radiusTween.Kill();
		}
		PrepareDummies();
		SetRadius(initialRadius);
		SetAngularSpeed(0f);
		SetTweens();
	}

	private void SetTweens()
	{
		float duration = atkDuration * 0.5f;
		radiusTween = DOTween.To(GetCurrentRadius, SetRadius, finalRadius, atkDuration).SetEase(radiusGrowthEase).SetUpdate(UpdateType.Normal, isIndependentUpdate: false);
		radiusTween.OnComplete(delegate
		{
			KeepRotating(extensionTime);
		});
		speedTween = DOTween.To(GetAngularSpeed, SetAngularSpeed, maxAngularSpeed, duration).SetUpdate(UpdateType.Normal, isIndependentUpdate: false).SetEase(angularSpeedEase);
	}

	private void KeepRotating(float v)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.SetUpdate(UpdateType.Normal, isIndependentUpdate: false);
		sequence.AppendInterval(1f / 60f);
		sequence.OnStepComplete(delegate
		{
			UpdateAllDummies(finalRadius);
		});
		sequence.SetLoops((int)(60f * v));
		sequence.Play();
	}

	private float GetAngularSpeed()
	{
		return spinningTransform.GetComponent<SpinBehavior>().angularSpeed;
	}

	private void SetAngularSpeed(float spd)
	{
		spinningTransform.GetComponent<SpinBehavior>().angularSpeed = spd;
	}

	private float GetCurrentRadius()
	{
		return currentRadius;
	}

	private void SetRadius(float newRadius)
	{
		currentRadius = newRadius;
		UpdateAllDummies(newRadius);
	}

	private void UpdateAllDummies(float newRadius)
	{
		for (int i = 0; i < numberOfProjectiles; i++)
		{
			UpdateDummies(i, currentRadius);
		}
	}

	private void PrepareDummies()
	{
		dummies.ForEach(delegate(Transform x)
		{
			x.gameObject.SetActive(value: false);
		});
		currentProjectiles.Clear();
		for (int i = 0; i < numberOfProjectiles; i++)
		{
			Quaternion quaternion = Quaternion.Euler(0f, 0f, i * 360 / numberOfProjectiles);
			dummies[i].localPosition = quaternion * Vector2.right * initialRadius;
			dummies[i].gameObject.SetActive(value: true);
			GameObject gameObject = PoolManager.Instance.ReuseObject(projectilePrefab, dummies[i].transform.position, Quaternion.identity).GameObject;
			Projectile component = gameObject.GetComponent<Projectile>();
			component.timeToLive = atkDuration + extensionTime;
			component.ResetTTL();
			currentProjectiles.Add(gameObject);
		}
	}

	private void UpdateDummies(int index, float distance)
	{
		dummies[index].localPosition = dummies[index].localPosition.normalized * distance;
		currentProjectiles[index].transform.position = dummies[index].transform.position;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(spinningTransform.position, initialRadius);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(spinningTransform.position, finalRadius);
		if (dummies == null)
		{
			return;
		}
		for (int i = 0; i < dummies.Count; i++)
		{
			if (dummies[i].gameObject.activeInHierarchy)
			{
				Gizmos.DrawWireSphere(dummies[i].position, 0.5f);
			}
		}
	}
}
