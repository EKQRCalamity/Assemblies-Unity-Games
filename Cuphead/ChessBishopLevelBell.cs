using System.Collections;
using UnityEngine;

public class ChessBishopLevelBell : AbstractProjectile
{
	private const int AnimatorBaseLayer = 0;

	private const int AnimatorSmokeTopLayer = 1;

	private const int AnimatorSmokeMiddleLayer = 2;

	private const int AnimatorSmokeBottomLayer = 3;

	[SerializeField]
	private Transform[] smokeTransforms;

	private LevelProperties.ChessBishop.Bishop properties;

	private AbstractPlayerController player;

	protected override bool DestroyedAfterLeavingScreen => true;

	public virtual ChessBishopLevelBell Init(Vector3 pos, AbstractPlayerController player, LevelProperties.ChessBishop.Bishop properties)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		this.properties = properties;
		this.player = player;
		StartCoroutine(move_cr());
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private IEnumerator move_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, properties.projectileDelayRange.RandomFloat());
		base.animator.SetTrigger("Attack");
		yield return base.animator.WaitForAnimationToEnd(this, "Intro", 0);
		Vector3 direction = (player.transform.position - base.transform.position).normalized;
		if (base.animator.GetInteger(AbstractProjectile.Variant) == 0)
		{
			base.animator.Play("A", 1);
			base.animator.Play("A", 2);
			base.animator.Play("IntroA", 3);
			Transform[] array = smokeTransforms;
			foreach (Transform transform in array)
			{
				transform.rotation = Quaternion.Euler(0f, 0f, 45f);
			}
			base.transform.SetEulerAngles(0f, 0f, 90f + MathUtils.DirectionToAngle(direction));
		}
		else
		{
			base.animator.Play("B", 1);
		}
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			base.transform.position += direction * properties.projectileSpeed * CupheadTime.FixedDelta;
			yield return wait;
		}
	}
}
