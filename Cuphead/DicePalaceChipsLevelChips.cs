using System;
using System.Collections;
using UnityEngine;

public class DicePalaceChipsLevelChips : LevelProperties.DicePalaceChips.Entity
{
	[Serializable]
	public class ChipPieces
	{
		public Transform chipTransform;

		public Vector3 startPosition;

		public float rotationSpeed;
	}

	private const float FRAME_TIME = 1f / 24f;

	[SerializeField]
	private ChipPieces[] chips;

	[SerializeField]
	private Transform mainLayer;

	[SerializeField]
	private GameObject hat;

	private float leftScreenXPos;

	private float rightScreenXPos;

	private float rightScreenXPosStart;

	private int currentAttackCount;

	private int maxAttacksPerCycle;

	private bool chipInFlight;

	private bool currentlyFloating;

	private bool firstTimeMoving = true;

	private DamageReceiver damageReceiver;

	private bool DeathSoundPlaying;

	private bool SpinSoundPlaying;

	private bool ExpandSoundPlaying;

	protected override void Awake()
	{
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		base.Awake();
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	protected virtual float hitPauseCoefficient()
	{
		return (!GetComponent<DamageReceiver>().IsHitPaused) ? 1f : 0f;
	}

	public override void LevelInit(LevelProperties.DicePalaceChips properties)
	{
		Level.Current.OnLevelStartEvent += StartAttacking;
		Level.Current.OnWinEvent += Death;
		leftScreenXPos = (float)Level.Current.Left + 100f;
		rightScreenXPos = (float)Level.Current.Right - 100f;
		rightScreenXPosStart = chips[0].chipTransform.position.x;
		for (int i = 0; i < chips.Length; i++)
		{
			chips[i].startPosition = chips[i].chipTransform.position;
		}
		currentAttackCount = 0;
		base.LevelInit(properties);
	}

	private void StartAttacking()
	{
		StartCoroutine(chipAttack_cr());
	}

	private IEnumerator chipAttack_cr()
	{
		LevelProperties.DicePalaceChips.Chips p = base.properties.CurrentState.chips;
		yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.chips.initialAttackDelay);
		int mainStringIndex = UnityEngine.Random.Range(0, p.chipAttackString.Length);
		int dir = -1;
		int attackIndex = UnityEngine.Random.Range(0, maxAttacksPerCycle);
		while (true)
		{
			string[] currentAttackChips = p.chipAttackString[mainStringIndex].Split(',');
			maxAttacksPerCycle = currentAttackChips.Length;
			for (int j = 0; j < chips.Length; j++)
			{
				float rotationSpeed = ((!Rand.Bool()) ? (-5f) : 5f);
				chips[j].rotationSpeed = rotationSpeed;
			}
			base.animator.SetBool("IsSpread", value: true);
			yield return base.animator.WaitForAnimationToStart(this, "Spread_Open");
			float startPos = chips[chips.Length - 1].chipTransform.position.y;
			float frameTime = 0f;
			float time = 1.5f;
			float t2 = 0f;
			int counter2 = 0;
			while (t2 < time)
			{
				float val = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t2 / time);
				frameTime += (float)CupheadTime.Delta * base.animator.speed;
				t2 += (float)CupheadTime.Delta * base.animator.speed;
				if (frameTime > 1f / 24f)
				{
					frameTime -= 1f / 24f;
					for (int num = chips.Length - 1; num >= 0; num--)
					{
						float num2 = ((num != 0) ? (chips[num].chipTransform.GetComponent<Renderer>().bounds.size.y / 1.7f) : (chips[num].chipTransform.GetComponent<Renderer>().bounds.size.y / 5.5f));
						float b = startPos + (float)counter2 * num2;
						Vector3 position = chips[num].chipTransform.position;
						position.y = Mathf.Lerp(position.y, b, val);
						chips[num].chipTransform.position = position;
						counter2 = (counter2 + 1) % chips.Length;
						float num3 = Mathf.Sin(t2 / 0.7f);
						chips[num].chipTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, num3 * chips[num].rotationSpeed));
					}
				}
				yield return null;
			}
			currentlyFloating = true;
			ChipPieces[] array = chips;
			foreach (ChipPieces chipPieces in array)
			{
				StartCoroutine(rotate_chips_cr(chipPieces.chipTransform, chipPieces.rotationSpeed, 0.7f, t2));
			}
			yield return null;
			for (int i = attackIndex; i < currentAttackChips.Length; i++)
			{
				string[] currentAttackChipsMultiple = currentAttackChips[i].Split('-');
				SFX_DicePalaceChipsShoot();
				string[] array2 = currentAttackChipsMultiple;
				foreach (string chip in array2)
				{
					if (chip[0] == 'D')
					{
						yield return CupheadTime.WaitForSeconds(this, Parser.FloatParse(chip.Substring(1)));
					}
					else if (currentAttackCount < maxAttacksPerCycle - 1)
					{
						StartCoroutine(moveChip_cr(base.transform.GetChild(Parser.IntParse(chip) - 1).transform, dir, lastChipOfCycle: false));
					}
					else
					{
						StartCoroutine(moveChip_cr(base.transform.GetChild(Parser.IntParse(chip) - 1).transform, dir, lastChipOfCycle: true));
					}
				}
				currentAttackCount++;
				if (currentAttackCount >= maxAttacksPerCycle)
				{
					currentAttackCount = 0;
					attackIndex = UnityEngine.Random.Range(0, maxAttacksPerCycle);
				}
				else
				{
					yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.chips.chipAttackDelay);
				}
				attackIndex = 0;
			}
			while (chipInFlight)
			{
				yield return null;
			}
			yield return CupheadTime.WaitForSeconds(this, 1f);
			time = 0.3f;
			t2 = 0f;
			counter2 = 0;
			base.animator.SetBool("IsSpread", value: false);
			yield return base.animator.WaitForAnimationToStart(this, "Spread_Close");
			yield return CupheadTime.WaitForSeconds(this, 0.4f);
			currentlyFloating = false;
			while (t2 < time)
			{
				float val2 = EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t2 / time);
				for (int num4 = chips.Length - 1; num4 >= 0; num4--)
				{
					float num5 = 0f;
					if (num4 != chips.Length - 1)
					{
						num5 = 10f;
					}
					float b2 = chips[num4].startPosition.y - (float)counter2 * num5;
					Vector3 position2 = chips[num4].chipTransform.position;
					position2.y = Mathf.Lerp(position2.y, b2, val2);
					chips[num4].chipTransform.position = position2;
					counter2 = (counter2 + 1) % chips.Length;
				}
				t2 += (float)CupheadTime.Delta;
				yield return null;
			}
			dir *= -1;
			yield return null;
			mainStringIndex = (mainStringIndex + 1) % p.chipAttackString.Length;
			float tt = 0f;
			while (tt < (float)base.properties.CurrentState.chips.attackCycleDelay)
			{
				tt += (float)CupheadTime.Delta * base.animator.speed;
				yield return null;
			}
		}
	}

	private void FlipSprite()
	{
		mainLayer.transform.SetScale(0f - mainLayer.transform.localScale.x, 1f, 1f);
		ChipPieces[] array = chips;
		foreach (ChipPieces chipPieces in array)
		{
			Vector3 position = chipPieces.chipTransform.position;
			position.y = chipPieces.startPosition.y;
			chipPieces.chipTransform.position = position;
		}
	}

	private IEnumerator moveChip_cr(Transform chip, int dir, bool lastChipOfCycle)
	{
		chipInFlight = lastChipOfCycle;
		float start = ((dir != 1) ? rightScreenXPos : leftScreenXPos);
		float end = ((dir != 1) ? leftScreenXPos : rightScreenXPos);
		Vector3 pos = chip.position;
		if (firstTimeMoving)
		{
			start = rightScreenXPosStart;
		}
		float pct = 0f;
		while (pct < 1f)
		{
			pos.x = start + (end - start) * pct;
			chip.position = pos;
			pct += (float)CupheadTime.Delta * base.properties.CurrentState.chips.chipSpeedMultiplier * hitPauseCoefficient() * base.animator.speed;
			yield return null;
		}
		pos.x = end;
		chip.position = pos;
		chipInFlight = false;
		if (lastChipOfCycle)
		{
			firstTimeMoving = false;
		}
	}

	private IEnumerator rotate_chips_cr(Transform chip, float speed, float time, float t)
	{
		while (currentlyFloating)
		{
			t += (float)CupheadTime.Delta;
			float phase = Mathf.Sin(t / time);
			chip.localRotation = Quaternion.Euler(new Vector3(0f, 0f, phase * speed));
			yield return null;
		}
		chip.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
		yield return null;
	}

	private void Death()
	{
		StopAllCoroutines();
		base.animator.SetBool("IsSpread", value: true);
		base.animator.SetTrigger("OnDeath");
		chips[0].chipTransform.SetScale(mainLayer.transform.localScale.x, 1f, 1f);
		StartCoroutine(head_fall_cr());
		for (int i = 1; i < chips.Length; i++)
		{
			StartCoroutine(chips_die(chips[i].chipTransform));
		}
	}

	private IEnumerator chips_die(Transform chip)
	{
		float speed = 2500f;
		float angle = UnityEngine.Random.Range(0, 360);
		Vector3 dir = MathUtils.AngleToDirection(0f - angle);
		chip.GetComponent<Collider2D>().enabled = false;
		while (true)
		{
			chip.position += dir * speed * CupheadTime.FixedDelta;
			yield return null;
		}
	}

	private void SpawnHat()
	{
		hat.SetActive(value: true);
		StartCoroutine(hat_fall_cr());
	}

	private IEnumerator head_fall_cr()
	{
		float velocity = 800f;
		float posY = (float)Level.Current.Ground + chips[0].chipTransform.GetComponent<Collider2D>().bounds.size.y / 1.2f;
		while (chips[0].chipTransform.position.y > posY)
		{
			chips[0].chipTransform.position += Vector3.down * velocity * CupheadTime.Delta;
			yield return null;
		}
		base.animator.SetTrigger("Continue");
		yield return null;
	}

	private IEnumerator hat_fall_cr()
	{
		float velocity = 30f;
		while (hat.transform.position.y > -250f)
		{
			hat.transform.position += Vector3.down * velocity * CupheadTime.Delta;
			yield return null;
		}
		hat.GetComponent<Animator>().SetTrigger("Continue");
		yield return null;
	}

	private void SFX_DicePalaceChipsIntro()
	{
		AudioManager.Play("chips_intro");
		emitAudioFromObject.Add("chips_intro");
		AudioManager.Play("vox_intro");
		emitAudioFromObject.Add("vox_intro");
	}

	private void SFX_DicePalaceChipsDeath()
	{
		if (!DeathSoundPlaying)
		{
			AudioManager.PlayLoop("chips_death");
			emitAudioFromObject.Add("chips_death");
			AudioManager.Play("vox_die");
			emitAudioFromObject.Add("vox_die");
			DeathSoundPlaying = true;
		}
	}

	private void SFX_DicePalaceChipsExpand()
	{
		if (!ExpandSoundPlaying)
		{
			AudioManager.Play("chips_expand");
			emitAudioFromObject.Add("chips_expand");
			AudioManager.Play("vox_idle");
			emitAudioFromObject.Add("vox_idle");
			ExpandSoundPlaying = true;
		}
	}

	private void SFX_DicePalaceChipsRetract()
	{
		AudioManager.Play("chips_retract");
		AudioManager.Play("vox_idle");
		ExpandSoundPlaying = false;
	}

	private void SFX_DicePalaceChipsShoot()
	{
		AudioManager.Play("chips_shoot");
		emitAudioFromObject.Add("chips_shoot");
	}

	private void SFX_DicePalaceChipsSpinLoop()
	{
		if (!SpinSoundPlaying)
		{
			AudioManager.PlayLoop("chips_spin_loop");
			emitAudioFromObject.Add("chips_spin_loop");
			SpinSoundPlaying = true;
		}
	}

	private void SFX_DicePalaceChipsSpinLoopStop()
	{
		AudioManager.Stop("chips_spin_loop");
		SpinSoundPlaying = false;
	}

	private void SFX_DicePalaceChipsBounce()
	{
		AudioManager.Play("chips_bounce");
	}
}
