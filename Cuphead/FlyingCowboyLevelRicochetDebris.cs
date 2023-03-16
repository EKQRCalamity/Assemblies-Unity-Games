using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelRicochetDebris : BasicUprightProjectile
{
	public enum BulletType
	{
		Nothing,
		Ricochet
	}

	private static readonly float GroundOffset = 100f;

	private static readonly float ShadowPositionOffset = -50f;

	private static readonly float ShadowStartOffset = 300f;

	private static readonly float ShadowEndOffset = 50f;

	private static readonly MinMax ShadowScaleRange = new MinMax(0.1f, 1f);

	private static readonly float[] AllowedRotations = new float[5] { -20f, -10f, 0f, 10f, 20f };

	private static int LastBitsIndex = 0;

	[SerializeField]
	private SpriteRenderer[] deathBits;

	[SerializeField]
	private BasicProjectile[] regularProjectiles;

	[SerializeField]
	private BasicProjectile[] parryableProjectiles;

	[SerializeField]
	private Transform shadowTransform;

	[SerializeField]
	private Effect deathEffect;

	private BulletType bulletType;

	private float bulletSpeed;

	private bool bulletParryable;

	public virtual BasicProjectile Create(Vector3 position, float speed, float bulletSpeed, BulletType bulletType, bool bulletParryable)
	{
		FlyingCowboyLevelRicochetDebris flyingCowboyLevelRicochetDebris = Create(position, MathUtils.DirectionToAngle(Vector3.down), speed) as FlyingCowboyLevelRicochetDebris;
		flyingCowboyLevelRicochetDebris.bulletType = bulletType;
		flyingCowboyLevelRicochetDebris.bulletSpeed = bulletSpeed;
		flyingCowboyLevelRicochetDebris.bulletParryable = bulletParryable;
		return flyingCowboyLevelRicochetDebris;
	}

	protected override void Start()
	{
		base.Start();
		base.animator.Update(0f);
		base.animator.Play(0, 0, Random.Range(0f, 1f));
		Vector3 localScale = base.transform.localScale;
		localScale.x *= Rand.PosOrNeg();
		base.transform.localScale = localScale;
		if (base.animator.GetInteger(AbstractProjectile.Variant) == 0)
		{
			base.transform.SetEulerAngles(0f, 0f, AllowedRotations.GetRandom());
		}
		StartCoroutine(fall_cr());
		StartCoroutine(shadowScale_cr());
		StartCoroutine(shadowPosition_cr());
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		SFX_COWGIRL_COWGIRL_P2_SafeHitPlayer();
	}

	private IEnumerator fall_cr()
	{
		while (base.transform.position.y > -360f + GroundOffset)
		{
			yield return null;
		}
		BulletType bulletType = this.bulletType;
		if (bulletType != 0 && bulletType == BulletType.Ricochet)
		{
			shootRicochetProjectile();
		}
		SFX_COWGIRL_COWGIRL_P2_SafeDropImpact();
		Die();
	}

	private IEnumerator shadowScale_cr()
	{
		shadowTransform.rotation = Quaternion.identity;
		float ground = -360f + GroundOffset;
		while (base.transform.position.y > ground + ShadowStartOffset)
		{
			yield return null;
		}
		float startY = ground + ShadowStartOffset;
		float endY = ground + ShadowEndOffset;
		base.animator.Play("On", 1);
		WaitForFrameTimePersistent wait = new WaitForFrameTimePersistent(1f / 24f);
		while (!base.dead)
		{
			float parentScale = base.transform.localScale.x;
			Vector3 scale = shadowTransform.localScale;
			scale.x = (scale.y = MathUtilities.LerpMapping(base.transform.position.y, startY, endY, ShadowScaleRange.min, ShadowScaleRange.max, clamp: true) / parentScale);
			shadowTransform.localScale = scale;
			yield return wait;
		}
		base.animator.Play("Off", 1);
	}

	private IEnumerator shadowPosition_cr()
	{
		float ground = -360f + GroundOffset;
		while (!base.dead)
		{
			Vector3 position = shadowTransform.position;
			position.y = ground + ShadowPositionOffset;
			shadowTransform.position = position;
			yield return null;
		}
	}

	private void shootRicochetProjectile()
	{
		AbstractPlayerController next = PlayerManager.GetNext();
		float rotation = MathUtils.DirectionToAngle(next.transform.position - base.transform.position);
		BasicProjectile basicProjectile;
		int num;
		if (bulletParryable)
		{
			num = Random.Range(0, parryableProjectiles.Length);
			basicProjectile = parryableProjectiles[num].Create(base.transform.position, rotation, bulletSpeed);
			num++;
		}
		else
		{
			num = Random.Range(0, regularProjectiles.Length);
			basicProjectile = regularProjectiles[num].Create(base.transform.position, rotation, bulletSpeed);
		}
		basicProjectile.SetParryable(bulletParryable);
		basicProjectile.GetComponent<SpriteRenderer>().sortingOrder = num;
	}

	protected override void Die()
	{
		RandomizeVariant();
		base.Die();
		bool flag = Rand.Bool();
		int num = (LastBitsIndex = ((LastBitsIndex == 0) ? (flag ? 1 : 2) : ((LastBitsIndex != 1) ? ((!flag) ? 1 : 0) : ((!flag) ? 2 : 0))));
		for (int i = 0; i < deathBits.Length; i++)
		{
			deathBits[i].enabled = i == num;
		}
		deathEffect.Create(new Vector3(base.transform.position.x, -360f + GroundOffset - 10f));
	}

	private void SFX_COWGIRL_COWGIRL_P2_SafeDropImpact()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p2_safedropimpact");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p2_safedropimpact");
	}

	private void SFX_COWGIRL_COWGIRL_P2_SafeHitPlayer()
	{
		AudioManager.Play("sfx_dlc_cowgirl_p2_safehitplayer");
		emitAudioFromObject.Add("sfx_dlc_cowgirl_p2_safehitplayer");
	}
}
