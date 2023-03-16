using System.Collections;
using UnityEngine;

public class SnowCultLevelPeashot : BasicProjectile
{
	private const float GROUND_OFFSET = 15f;

	[SerializeField]
	private BoxCollider2D boxCollider;

	[SerializeField]
	private Effect sparkleEffect;

	protected override void Start()
	{
		base.Start();
		SFX_SNOWCULT_TarotCardTravelLoop();
	}

	public override void SetParryable(bool parryable)
	{
		base.SetParryable(parryable);
		if (parryable)
		{
			switch (Random.Range(0, 2))
			{
			case 0:
				base.animator.Play("SwordPink", 0, 0.25f);
				break;
			case 1:
				base.animator.Play("SunPink", 0, 0.25f);
				break;
			}
		}
		else
		{
			switch (Random.Range(0, 3))
			{
			case 0:
				base.animator.Play("Sword", 0, 0.25f);
				break;
			case 1:
				base.animator.Play("Sun", 0, 0.25f);
				break;
			case 2:
				base.animator.Play("Moon", 0, 0.25f);
				break;
			}
		}
		base.animator.Update(0f);
	}

	private IEnumerator dead_cr()
	{
		Speed = 0f;
		move = false;
		boxCollider.enabled = false;
		SFX_SNOWCULT_TarotCardHitGround();
		switch ((int)(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f * 10f))
		{
		case 0:
		case 5:
			base.animator.Play("Die");
			break;
		case 3:
		case 8:
			base.animator.Play("DieAngleA");
			break;
		case 2:
		case 7:
			base.animator.Play("DieAngleA");
			GetComponent<SpriteRenderer>().flipX = true;
			break;
		case 4:
		case 9:
			base.animator.Play("DieAngleB");
			break;
		case 1:
		case 6:
			base.animator.Play("DieAngleB");
			GetComponent<SpriteRenderer>().flipX = true;
			break;
		}
		base.animator.Update(0f);
		Vector3 impactPos = new Vector3(base.transform.position.x, Level.Current.Ground);
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.99f)
		{
			Vector3 offset = (Vector3)MathUtils.AngleToDirection(Random.Range(0, 360)) * base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime * 250f;
			offset.y *= 0.3f;
			sparkleEffect.Create(impactPos + offset);
			yield return CupheadTime.WaitForSeconds(this, Random.Range(0.005f, 0.01f));
		}
		Die();
	}

	protected override void Move()
	{
		base.transform.position += base.transform.up * (0f - Speed) * CupheadTime.FixedDelta - new Vector3(0f, _accumulativeGravity * CupheadTime.FixedDelta, 0f);
		if (base.transform.position.y <= (float)Level.Current.Ground + 15f || base.transform.position.x < (float)Level.Current.Left || base.transform.position.x > (float)Level.Current.Right)
		{
			StartCoroutine(dead_cr());
		}
	}

	public override void OnParryDie()
	{
		AudioManager.Stop("sfx_dlc_snowcult_p1_wizard_tarotcardattack_travel_loop");
		base.OnParryDie();
	}

	private void SFX_SNOWCULT_TarotCardTravelLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_snowcult_p1_wizard_tarotcardattack_travel_loop");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_tarotcardattack_travel_loop");
	}

	private void SFX_SNOWCULT_TarotCardHitGround()
	{
		AudioManager.Stop("sfx_dlc_snowcult_p1_wizard_tarotcardattack_travel_loop");
		AudioManager.Play("sfx_dlc_snowcult_p1_wizard_tarotcard_hitground");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_tarotcard_hitground");
	}
}
