using DG.Tweening;
using Framework.FrameworkCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class EntityDisplacement : Trait
{
	private const string DamageDisplacement = "DamageDisplacement";

	private const string NormalDisplacement = "RegularDisplacement";

	[FoldoutGroup("Hit Displacement Settings", 0)]
	[Tooltip("Displacement in Unity Units by hit force.")]
	public float DisplacementByForce;

	[FoldoutGroup("Hit Displacement Settings", 0)]
	[Tooltip("Displacement movement ease.")]
	public AnimationCurve DisplacementEase;

	[FoldoutGroup("Hit Displacement Settings", 0)]
	[Tooltip("Time taken during displacement.")]
	public float DisplacementTime;

	[FoldoutGroup("Hit Displacement Settings", 0)]
	[Tooltip("Entity is displaced when is damaged")]
	public bool OnHitDisplacement = true;

	[MinMaxSlider(0f, 4f, false)]
	public Vector2 DistanceFactor;

	private EntityMotionChecker MotionChecker { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		MotionChecker = base.EntityOwner.GetComponentInChildren<EntityMotionChecker>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.EntityOwner.OnDamaged += OnDamaged;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!(MotionChecker == null) && (MotionChecker.HitsBlock || !MotionChecker.HitsFloor))
		{
			if (DOTween.IsTweening("RegularDisplacement"))
			{
				DOTween.Kill("RegularDisplacement");
			}
			if (DOTween.IsTweening("DamageDisplacement"))
			{
				DOTween.Kill("DamageDisplacement");
			}
		}
	}

	private void OnDamaged()
	{
		if ((bool)MotionChecker && !MotionChecker.HitsBlock && MotionChecker.HitsFloor)
		{
			Hit lastHit = base.EntityOwner.EntityDamageArea.LastHit;
			Enemy enemy = (Enemy)base.EntityOwner;
			if ((!enemy || !enemy.IsStunt) && !base.EntityOwner.Status.Dead && OnHitDisplacement)
			{
				Move(lastHit);
			}
		}
	}

	public void Move(float horDisplacement, float timeLapse, Ease ease)
	{
		Vector3 position = base.EntityOwner.transform.position;
		Vector2 entityPosition = new Vector2(position.x, position.y);
		horDisplacement = ClampHorizontalDisplacement(entityPosition, horDisplacement);
		Vector2 vector = new Vector2(entityPosition.x + horDisplacement, entityPosition.y);
		base.EntityOwner.transform.DOMoveX(vector.x, DisplacementTime).SetEase(ease).SetId("RegularDisplacement");
	}

	private void Move(Hit hit)
	{
		Vector3 position = base.EntityOwner.transform.position;
		float horizontalDisplacement = GetHorizontalDisplacement(hit);
		horizontalDisplacement = ClampHorizontalDisplacement(position, horizontalDisplacement);
		Vector2 vector = new Vector2(position.x + horizontalDisplacement, position.y);
		base.EntityOwner.transform.DOMoveX(vector.x, DisplacementTime).SetEase(DisplacementEase).SetId("DamageDisplacement");
	}

	private float ClampHorizontalDisplacement(Vector2 entityPosition, float distance)
	{
		float num = 0.2f;
		entityPosition += num * Vector2.up;
		float result = distance;
		Vector2 b = entityPosition + distance * Vector2.right;
		if (MotionChecker.HitsBlockInPosition(entityPosition, Mathf.Sign(distance) * Vector2.right, distance, out var hitPoint, show: true))
		{
			result = Vector2.Distance(entityPosition, hitPoint);
			return Mathf.Floor(result);
		}
		int num2 = 5;
		for (int i = 0; i < num2; i++)
		{
			Vector2 vector = Vector2.Lerp(entityPosition, b, (float)i / (float)num2);
			if (!MotionChecker.HitsFloorInPosition(vector, 0.5f, out hitPoint, show: true))
			{
				return Vector2.Distance(entityPosition, vector);
			}
			bool flag = false;
		}
		return result;
	}

	private float GetHorizontalDisplacement(Hit hit)
	{
		if (!hit.AttackingEntity)
		{
			return 0f;
		}
		Vector3 position = hit.AttackingEntity.transform.position;
		float displacementByDistance = GetDisplacementByDistance(base.EntityOwner.transform.position, position);
		float num = DisplacementByForce * hit.Force * displacementByDistance;
		if (position.x > base.EntityOwner.transform.position.x)
		{
			num *= -1f;
		}
		return num;
	}

	private float GetDisplacementByDistance(Vector2 a, Vector2 b)
	{
		float value = Vector2.Distance(a, b);
		value = Mathf.Clamp(value, DistanceFactor.x, DistanceFactor.y);
		return (DistanceFactor.y - value) / (DistanceFactor.y - DistanceFactor.x);
	}

	private void OnDestroy()
	{
		base.EntityOwner.OnDamaged -= OnDamaged;
	}
}
