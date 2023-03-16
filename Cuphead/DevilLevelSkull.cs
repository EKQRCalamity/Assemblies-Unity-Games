using System.Collections;
using UnityEngine;

public class DevilLevelSkull : AbstractProjectile
{
	private LevelProperties.Devil.SkullEye properties;

	public DevilLevelSkull Create(Vector2 pos, LevelProperties.Devil.SkullEye properties)
	{
		DevilLevelSkull devilLevelSkull = InstantiatePrefab<DevilLevelSkull>();
		devilLevelSkull.properties = properties;
		devilLevelSkull.transform.position = pos;
		devilLevelSkull.StartCoroutine(devilLevelSkull.main_cr());
		return devilLevelSkull;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!CupheadLevelCamera.Current.ContainsPoint(base.transform.position, new Vector2(1000f, 100f)))
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator main_cr()
	{
		yield return base.animator.WaitForAnimationToEnd(this, "Start");
		AbstractPlayerController player = PlayerManager.GetNext();
		Vector2 moveDir = ((Vector2)(player.transform.position - base.transform.position)).normalized;
		Vector2 velocity = moveDir * properties.initialMoveSpeed;
		float rotation = MathUtils.DirectionToAngle(player.transform.position - base.transform.position);
		float t2 = 0f;
		while (t2 < properties.initialMoveDuration)
		{
			t2 += CupheadTime.FixedDelta;
			base.transform.AddPosition(velocity.x * CupheadTime.FixedDelta, velocity.y * CupheadTime.FixedDelta);
			yield return new WaitForFixedUpdate();
		}
		float rotationSpeed = (float)Rand.PosOrNeg() * properties.swirlRotationSpeed;
		t2 = 0f;
		Vector2 spiralOrigin = base.transform.position;
		while (true)
		{
			t2 += CupheadTime.FixedDelta;
			rotation += rotationSpeed * CupheadTime.FixedDelta;
			base.transform.position = spiralOrigin + MathUtils.AngleToDirection(rotation) * properties.swirlMoveOutwardSpeed * t2;
			yield return new WaitForFixedUpdate();
		}
	}
}
