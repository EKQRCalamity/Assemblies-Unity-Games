using System.Collections;
using UnityEngine;

public class AirplaneLevelBoomerang : AbstractProjectile
{
	private const float xMax = 725f;

	private float delay;

	private bool onLeft;

	private float speedForward;

	private float easeDistanceForward;

	private float speedReturn;

	private float easeDistanceReturn;

	[SerializeField]
	private SpriteRenderer rend;

	private int id;

	public AirplaneLevelBoomerang Create(Vector2 pos, float speedF, float easeDF, float speedR, float easeDR, float delay, bool onLeft, int id)
	{
		AirplaneLevelBoomerang airplaneLevelBoomerang = base.Create() as AirplaneLevelBoomerang;
		airplaneLevelBoomerang.transform.position = pos;
		airplaneLevelBoomerang.DamagesType.OnlyPlayer();
		airplaneLevelBoomerang.delay = delay;
		airplaneLevelBoomerang.onLeft = onLeft;
		airplaneLevelBoomerang.speedForward = speedF;
		airplaneLevelBoomerang.easeDistanceForward = easeDF;
		airplaneLevelBoomerang.speedReturn = speedR;
		airplaneLevelBoomerang.easeDistanceReturn = easeDR;
		airplaneLevelBoomerang.id = id;
		return airplaneLevelBoomerang;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase == CollisionPhase.Enter)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void Start()
	{
		base.Start();
		if (!base.CanParry)
		{
			base.animator.Play((!Rand.Bool()) ? "B" : "A");
		}
		StartCoroutine(move_cr());
		SFX_DOGFIGHT_BoneShot_Loop();
	}

	private IEnumerator move_cr()
	{
		rend.enabled = true;
		float end = ((!onLeft) ? (-725f + easeDistanceForward) : (725f - easeDistanceForward));
		bool flipSprite = ((!onLeft) ? true : false);
		YieldInstruction wait = new WaitForFixedUpdate();
		GetComponent<SpriteRenderer>().flipX = flipSprite;
		while ((onLeft && Mathf.Sign(base.transform.position.x - end) == -1f) || (!onLeft && Mathf.Sign(base.transform.position.x - end) == 1f))
		{
			base.transform.position += Vector3.right * speedForward * CupheadTime.FixedDelta * (onLeft ? 1 : (-1));
			yield return wait;
		}
		float t = 0f;
		float tMax = easeDistanceForward / speedForward * 2f;
		float start = end;
		end = ((!onLeft) ? (-725f) : 725f);
		while (t < tMax)
		{
			t += CupheadTime.FixedDelta;
			yield return wait;
			base.transform.position = new Vector3(Mathf.Lerp(start, end, EaseUtils.EaseOutSine(0f, 1f, Mathf.InverseLerp(0f, tMax, t))), base.transform.position.y);
		}
		base.transform.position = new Vector3(end, base.transform.position.y);
		yield return CupheadTime.WaitForSeconds(this, delay);
		GetComponent<SpriteRenderer>().flipX = !flipSprite;
		t = 0f;
		tMax = easeDistanceReturn / speedReturn * 2f;
		start = base.transform.position.x;
		end = ((!onLeft) ? (-725f + easeDistanceReturn) : (725f - easeDistanceReturn));
		while (t < tMax)
		{
			t += CupheadTime.FixedDelta;
			yield return wait;
			base.transform.position = new Vector3(Mathf.Lerp(start, end, EaseUtils.EaseInSine(0f, 1f, Mathf.InverseLerp(0f, tMax, t))), base.transform.position.y);
		}
		base.transform.position = new Vector3(end, base.transform.position.y);
		end = ((!onLeft) ? 1025f : (-1025f));
		while ((onLeft && Mathf.Sign(base.transform.position.x - end) == 1f) || (!onLeft && Mathf.Sign(base.transform.position.x - end) == -1f))
		{
			base.transform.position += Vector3.right * speedReturn * CupheadTime.FixedDelta * ((!onLeft) ? 1 : (-1));
			yield return wait;
		}
		SFX_DOGFIGHT_BoneShot_StopLoop();
		Die();
		yield return null;
	}

	public override void OnParry(AbstractPlayerController player)
	{
		base.OnParry(player);
		SFX_DOGFIGHT_BoneShot_StopLoop();
	}

	private void SFX_DOGFIGHT_BoneShot_Loop()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p1_bulldog_boneshot_0" + (id + 1), 0.12f, 0.1f);
		AudioManager.PlayLoop("sfx_dlc_dogfight_p1_bulldog_boneshot_0" + (id + 1));
		emitAudioFromObject.Add("sfx_dlc_dogfight_p1_bulldog_boneshot_0" + (id + 1));
	}

	private void SFX_DOGFIGHT_BoneShot_StopLoop()
	{
		AudioManager.Stop("sfx_dlc_dogfight_p1_bulldog_boneshot_0" + (id + 1));
	}
}
