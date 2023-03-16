using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RumRunnersLevelSnout : AbstractCollidableObject
{
	public enum AttackType
	{
		Quick,
		Fake,
		Tongue
	}

	private static readonly Vector3 OffscreenCoord = new Vector3(0f, 1500f);

	[SerializeField]
	private Transform copballLaunchOrigin;

	[SerializeField]
	private RumRunnersLevelCopBall copBallPrefab;

	[SerializeField]
	private Effect dirtEffect;

	[SerializeField]
	private RumRunnersLevelAnteater parent;

	[SerializeField]
	private Transform shadowTransform;

	[SerializeField]
	private DamageReceiver[] damageReceivers;

	[SerializeField]
	private Transform tonguePokeFXTransform;

	[SerializeField]
	private Effect fakeTongueSpittleEffect;

	private LevelProperties.RumRunners properties;

	private Vector2 snoutScale;

	private List<RumRunnersLevelCopBall> copBallList = new List<RumRunnersLevelCopBall>();

	private bool onLeft;

	private bool endNormal;

	private bool endTongue;

	private PatternString copBallLaunchAnglePattern;

	public bool isAttacking { get; private set; }

	private void Start()
	{
		DamageReceiver[] array = damageReceivers;
		foreach (DamageReceiver damageReceiver in array)
		{
			damageReceiver.OnDamageTaken += OnDamageTaken;
		}
		snoutScale = base.transform.localScale;
		base.transform.position = OffscreenCoord;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		parent.DoDamage(info.damage);
	}

	public void Setup(LevelProperties.RumRunners properties)
	{
		this.properties = properties;
		copBallLaunchAnglePattern = new PatternString(properties.CurrentState.copBall.copBallLaunchAngleString);
	}

	public void Attack(Vector3 position, Vector2 shadowPosition, bool onLeft, AttackType attackType)
	{
		Vector3 position2 = position;
		position2.x = ((!onLeft) ? Level.Current.Right : Level.Current.Left);
		Vector3 one = Vector3.one;
		one.x *= (onLeft ? 1 : (-1));
		dirtEffect.Create(position2, one);
		base.transform.position = position;
		shadowTransform.localPosition = shadowPosition;
		StartCoroutine(attack_cr(onLeft, attackType));
	}

	private IEnumerator attack_cr(bool onLeft, AttackType attackType)
	{
		LevelProperties.RumRunners.AnteaterSnout p = properties.CurrentState.anteaterSnout;
		this.onLeft = onLeft;
		endNormal = (endTongue = false);
		base.transform.SetScale((!onLeft) ? (0f - snoutScale.x) : snoutScale.x, snoutScale.y);
		parent.SetEyeSide(onLeft);
		base.animator.SetBool("Fake", attackType == AttackType.Fake);
		base.animator.SetBool("Tongue", attackType == AttackType.Tongue);
		base.animator.SetTrigger("Attack");
		isAttacking = true;
		if (attackType == AttackType.Fake || attackType == AttackType.Tongue)
		{
			float fullOutBoilDelay = p.snoutFullOutBoilDelay;
			if (fullOutBoilDelay > 0f)
			{
				yield return base.animator.WaitForAnimationToStart(this, "FullOutHold");
				yield return CupheadTime.WaitForSeconds(this, fullOutBoilDelay);
			}
			base.animator.SetTrigger("HoldComplete");
		}
		if (attackType == AttackType.Tongue)
		{
			yield return base.animator.WaitForAnimationToStart(this, "TongueHold");
			yield return CupheadTime.WaitForSeconds(this, p.tongueHoldDuration);
			base.animator.SetBool("Tongue", value: false);
			while (!endTongue)
			{
				yield return null;
			}
		}
		else
		{
			while (!endNormal)
			{
				yield return null;
			}
		}
		isAttacking = false;
		if (attackType == AttackType.Tongue)
		{
			yield return base.animator.WaitForAnimationToStart(this, "Off");
		}
		else
		{
			yield return base.animator.WaitForAnimationToEnd(this, "QuickEnd");
		}
		base.transform.position = OffscreenCoord;
	}

	private void animationEvent_EndNormalAttack()
	{
		endNormal = true;
	}

	private void animationEvent_TriggerTongueEyes()
	{
		parent.TriggerEyesTurnaround();
	}

	private void animationEvent_EndFakeTongueAttack()
	{
		Effect effect = fakeTongueSpittleEffect.Create(tonguePokeFXTransform.position);
		if (!onLeft)
		{
			Vector3 localScale = effect.transform.localScale;
			localScale.x *= -1f;
			effect.transform.localScale = localScale;
		}
	}

	private void animationEvent_EndTongueAttack()
	{
		endTongue = true;
	}

	private void animationEvent_FlipIfNecessary()
	{
		float num = copBallLaunchAnglePattern.PopFloat();
		base.animator.SetBool("ThrowDown", num < 0f);
	}

	private void animationEvent_SpawnCopBall()
	{
		LevelProperties.RumRunners.CopBall copBall = properties.CurrentState.copBall;
		do
		{
			copBallList.RemoveAll((RumRunnersLevelCopBall b) => b == null || b.leaveScreen);
			if (copBallList.Count >= copBall.copBallMaxCount)
			{
				copBallList[0].leaveScreen = true;
			}
		}
		while (copBallList.Count >= copBall.copBallMaxCount);
		float @float = copBallLaunchAnglePattern.GetFloat();
		RumRunnersLevelCopBall rumRunnersLevelCopBall = copBallPrefab.Spawn();
		float angle = ((!onLeft) ? (180f - @float) : @float);
		rumRunnersLevelCopBall.Init(copballLaunchOrigin.position, MathUtils.AngleToDirection(angle), copBall.copBallSpeed, copBall.copBallHP, copBall, copballLaunchOrigin);
		copBallList.Add(rumRunnersLevelCopBall);
	}

	private void animationEvent_FireCopBall()
	{
		if (copBallList[copBallList.Count - 1] != null)
		{
			copBallList[copBallList.Count - 1].Launch();
		}
	}

	public void Death()
	{
		StopAllCoroutines();
		foreach (RumRunnersLevelCopBall copBall in copBallList)
		{
			if (copBall != null)
			{
				copBall.Death(playAudio: false);
			}
		}
		base.gameObject.SetActive(value: false);
	}

	private void AnimationEvent_SFX_RUMRUN_P3_AntEater_Attack_Enter()
	{
		if (base.animator.GetBool("Tongue"))
		{
			AudioManager.Play("sfx_dlc_rumrun_p3_anteater_attack_snout_tongue_fullouthold");
			emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_attack_snout_tongue_fullouthold");
		}
		else
		{
			AudioManager.Play("sfx_dlc_rumrun_p3_anteater_attack_short_enter");
			emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_attack_short_enter");
		}
	}

	private void AnimationEvent_SFX_RUMRUN_P3_AntEater_Attack_Tongue()
	{
		if (base.animator.GetBool("Tongue"))
		{
			AudioManager.Play("sfx_dlc_rumrun_p3_anteater_attack_snout_tongue_attack");
		}
	}

	private void AnimationEvent_SFX_RUMRUN_P3_AntEater_Attack_ShortExit()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_anteater_attack_short_exit");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_attack_short_exit");
	}

	private void AnimationEvent_SFX_RUMRUN_P3_AntEater_Attack_SpitBallCop()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_anteater_attack_snout_tongue_spitballcop");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_anteater_attack_snout_tongue_spitballcop");
	}

	private void AnimationEvent_SFX_RUMRUN_P3_BallCop_SpitVocalShouts()
	{
		AudioManager.Play("sfx_dlc_rumrun_p3_ballcop_spitvocalshouts");
		emitAudioFromObject.Add("sfx_dlc_rumrun_p3_ballcop_spitvocalshouts");
	}
}
