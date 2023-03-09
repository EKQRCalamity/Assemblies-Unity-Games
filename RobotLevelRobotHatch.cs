using System.Collections;
using UnityEngine;

public class RobotLevelRobotHatch : RobotLevelRobotBodyPart
{
	private float shotbotSpawnDelay;

	private int nearestEventFrame;

	[SerializeField]
	private GameObject[] damagedHatches;

	[SerializeField]
	private GameObject damageEffect;

	private IEnumerator damageEffectRoutine;

	private SpriteRenderer damageEffectRenderer;

	public override void InitBodyPart(RobotLevelRobot parent, LevelProperties.Robot properties, int primaryHP = 0, int secondaryHP = 1, float attackDelayMinus = 0f)
	{
		primaryAttackDelay = properties.CurrentState.shotBot.initialSpawnDelay.RandomFloat();
		secondaryAttackDelay = properties.CurrentState.bombBot.bombDelay;
		primaryHP = properties.CurrentState.shotBot.hatchGateHealth;
		attackDelayMinus = properties.CurrentState.shotBot.shotbotSpawnDelayMinus;
		shotbotSpawnDelay = properties.CurrentState.shotBot.shotbotDelay;
		base.InitBodyPart(parent, properties, primaryHP, secondaryHP, attackDelayMinus);
		base.animator.Play("Closed", 0, 0.75f);
		base.animator.Play("Loop", 1, 0.75f);
		base.animator.Play("Loop", 2, 0.75f);
		base.animator.Play("Loop", 3, 0.75f);
		StartPrimary();
		damageEffectRenderer = damageEffect.GetComponent<SpriteRenderer>();
	}

	protected override void OnPrimaryAttack()
	{
		if (current == state.primary)
		{
			StartCoroutine(openHatch_cr());
			primaryAttackDelay = properties.CurrentState.shotBot.shotbotWaveDelay.RandomFloat();
			base.OnPrimaryAttack();
		}
	}

	private IEnumerator openHatch_cr()
	{
		float elapsedTime = base.animator.GetCurrentAnimatorStateInfo(2).length;
		float normalizedTime3 = parent.animator.GetCurrentAnimatorStateInfo(7).normalizedTime;
		normalizedTime3 %= 1f;
		float delay3 = normalizedTime3 * 24f;
		int currentFrame = (int)(delay3 / 24f);
		if (currentFrame < 2)
		{
			delay3 = (float)((2 - currentFrame) / 24) * elapsedTime;
			nearestEventFrame = 2;
		}
		else if (currentFrame < 14)
		{
			delay3 = (float)((14 - currentFrame) / 24) * elapsedTime;
			nearestEventFrame = 14;
		}
		else
		{
			delay3 = (float)(24 - currentFrame) * elapsedTime;
			delay3 += (float)(2 - currentFrame) * elapsedTime;
			nearestEventFrame = 2;
		}
		yield return CupheadTime.WaitForSeconds(this, delay3);
		yield return null;
		normalizedTime3 = parent.animator.GetCurrentAnimatorStateInfo(7).normalizedTime;
		if (nearestEventFrame == 2)
		{
			base.animator.SetTrigger("IsOpenFrame2");
		}
		else if (nearestEventFrame == 14)
		{
			base.animator.SetTrigger("IsOpenFrame14");
		}
		if (current != 0)
		{
		}
	}

	private void Open()
	{
		if (current != state.secondary)
		{
			GetComponent<SpriteRenderer>().enabled = true;
			SpriteRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer spriteRenderer in componentsInChildren)
			{
				spriteRenderer.enabled = true;
			}
		}
	}

	private void Close()
	{
		if (current != state.secondary)
		{
			GetComponent<SpriteRenderer>().enabled = false;
			SpriteRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer spriteRenderer in componentsInChildren)
			{
				spriteRenderer.enabled = false;
			}
		}
	}

	private IEnumerator closeHatch_cr()
	{
		GetComponent<SpriteRenderer>().enabled = true;
		base.animator.SetTrigger("IsClosing");
		yield return base.animator.WaitForAnimationToEnd(this, waitForEndOfFrame: true);
		yield return null;
		if (current == state.primary)
		{
			isAttacking = false;
		}
		yield return null;
	}

	private void SpawnShotbotWave()
	{
		StartCoroutine(spawnShotbotWave_cr());
	}

	private IEnumerator spawnShotbotWave_cr()
	{
		for (int i = 0; i < properties.CurrentState.shotBot.shotbotCount; i++)
		{
			GameObject shotbot = Object.Instantiate(primary, base.transform.position + Vector3.right * 80f + Vector3.down * 20f, Quaternion.identity);
			shotbot.GetComponent<RobotLevelHatchShotbot>().InitShotbot(properties.CurrentState.shotBot.shotbotHealth, properties.CurrentState.shotBot.bulletSpeed, properties.CurrentState.shotBot.pinkBulletCount, properties.CurrentState.shotBot.shotbotShootDelay, properties.CurrentState.shotBot.shotbotFlightSpeed);
			yield return CupheadTime.WaitForSeconds(this, shotbotSpawnDelay);
			if (current != 0)
			{
				break;
			}
		}
		yield return CupheadTime.WaitForSeconds(this, 0.4f);
		StartCoroutine(closeHatch_cr());
	}

	protected override void OnSecondaryAttack()
	{
		HomingProjectile homingProjectile = secondary.GetComponent<RobotLevelHatchBombBot>().Create(base.transform.position, 180f, properties.CurrentState.bombBot.initialBombMovementSpeed, properties.CurrentState.bombBot.bombHomingSpeed, properties.CurrentState.bombBot.bombRotationSpeed, properties.CurrentState.bombBot.bombLifeTime, properties.CurrentState.bombBot.bombInitialMovementDuration.RandomFloat(), 4f, PlayerManager.GetNext());
		homingProjectile.GetComponent<RobotLevelHatchBombBot>().InitBombBot(properties.CurrentState.bombBot);
		homingProjectile.transform.right = Vector3.down;
		if (currentHealth[1] <= 0f)
		{
			base.gameObject.SetActive(value: false);
			StopAllCoroutines();
		}
		base.OnSecondaryAttack();
	}

	protected override void OnPrimaryDeath()
	{
		if (current != state.secondary && currentHealth[0] <= 0f)
		{
			AudioManager.Play("robot_lower_chest_port_destroyed");
			emitAudioFromObject.Add("robot_lower_chest_port_destroyed");
			base.animator.Play("Off");
			GetComponent<BoxCollider2D>().enabled = false;
			StartSecondary();
			DeathEffect();
			StopCoroutine(openHatch_cr());
			StopCoroutine(closeHatch_cr());
			base.enabled = false;
			SpriteRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer spriteRenderer in componentsInChildren)
			{
				spriteRenderer.enabled = false;
			}
			GameObject[] array = damagedHatches;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActive(value: true);
				gameObject.GetComponent<SpriteRenderer>().enabled = true;
			}
		}
		base.OnPrimaryDeath();
	}

	protected override void ExitCurrentAttacks()
	{
		if (current == state.primary)
		{
			StopCoroutine(openHatch_cr());
			StartCoroutine(closeHatch_cr());
		}
		if (current == state.secondary)
		{
			StopCoroutine(secondaryAttack_cr());
		}
		base.ExitCurrentAttacks();
	}

	public void InitAnims()
	{
		base.animator.SetTrigger("OnRobotIntro");
	}

	protected override void Die()
	{
		if (damageEffectRoutine != null)
		{
			StopCoroutine(damageEffectRoutine);
		}
		damageEffect.SetActive(value: false);
		SpriteRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			spriteRenderer.enabled = false;
		}
		base.Die();
	}

	protected override void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.OnDamageTaken(info);
		if (damageEffectRoutine != null)
		{
			StopCoroutine(damageEffectRoutine);
		}
		damageEffectRoutine = damageEffect_cr();
		StartCoroutine(damageEffectRoutine);
	}

	private IEnumerator damageEffect_cr()
	{
		for (int i = 0; i < 3; i++)
		{
			damageEffectRenderer.enabled = true;
			damageEffect.SetActive(value: true);
			yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
			damageEffect.SetActive(value: false);
			yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		}
	}
}
