using System;
using System.Collections.Generic;
using Gameplay.GameControllers.Effects.Entity;
using UnityEngine;

public class AmanecidasAnimatorInyector : MonoBehaviour
{
	[Serializable]
	public struct AmanecidaBodyAnimators
	{
		public AMANECIDA_COLOR color;

		public RuntimeAnimatorController animatorController;

		public Color tintColor;

		public Material beamMaterial;
	}

	[Serializable]
	public struct AmanecidaWeaponAnimators
	{
		public AMANECIDA_WEAPON weapon;

		public RuntimeAnimatorController animatorController;

		public Material recolorMaterial;

		public Color baseColor;
	}

	public enum AMANECIDA_COLOR
	{
		BLUE,
		RED,
		SKYBLUE,
		WHITE,
		LAUDES
	}

	public enum AMANECIDA_WEAPON
	{
		HAND,
		AXE,
		LANCE,
		BOW,
		SWORD
	}

	private const string TRIGGER_CHARGE_ANTICIPATION = "CHARGE_ANTICIPATION";

	private const string TRIGGER_HURT = "HURT";

	private const string TRIGGER_BLINKSHOT = "BLINKSHOT";

	private const string TRIGGER_SUMMON_WEAPON = "SUMMON_WEAPON";

	private const string TRIGGER_MELEE_ATTACK = "MELEE_ATTACK";

	private const string TRIGGER_STOMP_ATTACK = "STOMP_ATTACK";

	private const string TRIGGER_DO_STOMP_DAMAGE = "DO_STOMP_DAMAGE";

	public Animator bodyAnimator;

	public Animator weaponAnimator;

	public List<AmanecidaBodyAnimators> bodyAnimatorsByType;

	public List<AmanecidaWeaponAnimators> weaponAnimatorsByType;

	public Animator energyChargeAnimator;

	private AMANECIDA_COLOR currentColor;

	private AMANECIDA_WEAPON currentWeapon;

	public Transform rotationParent;

	private SpriteRenderer bodySpr;

	private SpriteRenderer wpnSpr;

	public MasterShaderEffects shaderEffects;

	public MasterShaderEffects shaderEffectsWeapon;

	private Color amanecidaColor;

	public AMANECIDA_WEAPON GetWeapon()
	{
		return currentWeapon;
	}

	private void Awake()
	{
		bodySpr = bodyAnimator.GetComponent<SpriteRenderer>();
		wpnSpr = weaponAnimator.GetComponent<SpriteRenderer>();
	}

	public void Flip(bool flip)
	{
		bodySpr.flipX = flip;
		wpnSpr.flipX = flip;
	}

	public void SetAmanecidaColor(AMANECIDA_COLOR color)
	{
		currentColor = color;
		AmanecidaBodyAnimators amanecidaBodyAnimators = bodyAnimatorsByType.Find((AmanecidaBodyAnimators x) => x.color == color);
		bodyAnimator.runtimeAnimatorController = amanecidaBodyAnimators.animatorController;
		amanecidaColor = amanecidaBodyAnimators.tintColor;
	}

	public Material GetCurrentBeamMaterial()
	{
		return bodyAnimatorsByType.Find((AmanecidaBodyAnimators x) => x.color == currentColor).beamMaterial;
	}

	public void SetAmanecidaWeapon(AMANECIDA_WEAPON weapon)
	{
		currentWeapon = weapon;
		AmanecidaWeaponAnimators amanecidaWeaponAnimators = weaponAnimatorsByType.Find((AmanecidaWeaponAnimators x) => x.weapon == weapon);
		weaponAnimator.runtimeAnimatorController = amanecidaWeaponAnimators.animatorController;
		energyChargeAnimator.GetComponentInChildren<SpriteRenderer>().material = amanecidaWeaponAnimators.recolorMaterial;
		SetParticlesColor(amanecidaWeaponAnimators.baseColor);
	}

	private void SetParticlesColor(Color c)
	{
	}

	public void SetVelocity(Vector2 v)
	{
		bodyAnimator.SetFloat("xVelocity", v.x);
		bodyAnimator.SetFloat("yVelocity", v.y);
		weaponAnimator.SetFloat("xVelocity", v.x);
		weaponAnimator.SetFloat("yVelocity", v.y);
		float num = Mathf.Abs(v.x) / 5f;
	}

	public bool IsTurning()
	{
		return bodyAnimator.GetCurrentAnimatorStateInfo(0).IsName("TURN AROUND");
	}

	public bool IsOut()
	{
		return bodyAnimator.GetCurrentAnimatorStateInfo(0).IsName("OUT");
	}

	public void PlayTurnAround(bool instant = false)
	{
		float normalizedTime = 0f;
		if (instant)
		{
			normalizedTime = 0.96f;
		}
		bodyAnimator.Play("TURN AROUND", 0, normalizedTime);
		weaponAnimator.Play("TURN AROUND", 0, normalizedTime);
	}

	public void PlayHurt()
	{
		SetDualTrigger("HURT");
	}

	public void PlayBlinkshot()
	{
		SetDualTrigger("BLINKSHOT");
	}

	public void PlaySummonWeapon()
	{
		SetDualTrigger("SUMMON_WEAPON");
	}

	public void PlayMeleeAttack()
	{
		SetDualTrigger("MELEE_ATTACK");
	}

	public void PlayStompAttack(bool doStompDamage)
	{
		SetDualTrigger("STOMP_ATTACK");
		SetDualBool("DO_STOMP_DAMAGE", doStompDamage);
	}

	public void ClearStompAttackTrigger()
	{
		ResetDualTrigger("STOMP_ATTACK");
	}

	public void PlayChargeAnticipation(bool isHorizontalCharge)
	{
		SetDualTrigger("CHARGE_ANTICIPATION");
		SetHorizontalCharge(isHorizontalCharge);
	}

	public void ClearAllTriggers()
	{
		ResetDualTrigger("HURT");
		ResetDualTrigger("CHARGE_ANTICIPATION");
		ResetDualTrigger("BLINKSHOT");
		ResetDualTrigger("MELEE_ATTACK");
		ResetDualTrigger("STOMP_ATTACK");
		ResetDualTrigger("SUMMON_WEAPON");
	}

	public void ClearAll(bool includeTriggers)
	{
		if (includeTriggers)
		{
			ClearAllTriggers();
		}
		SetRecharging(active: false);
		SetMeleeAnticipation(v: false);
		SetLunge(v: false);
		SetBlink(value: false);
		SetCharge(v: false);
	}

	public void SetRecharging(bool active)
	{
		SetDualBool("RECHARGE", active);
	}

	public void SetEnergyCharge(bool active)
	{
		energyChargeAnimator.SetBool("ACTIVE", active);
	}

	public void SetStuck(bool active)
	{
		SetDualBool("STUCK_WEAPON", active);
	}

	public void SetTired(bool active)
	{
		SetDualBool("TIRED", active);
	}

	public void SetShockwaveAnticipation(bool v)
	{
		SetDualBool("SHOCKWAVE_ANTICIPATION", v);
	}

	public void SetBlink(bool value)
	{
		SetDualBool("BLINK", value);
	}

	public void SetBow(bool value)
	{
		SetDualBool("BOW_LOOP", value);
	}

	public void SetMeleeHold(bool value)
	{
		SetDualBool("MELEE_HOLD_LOOP", value);
	}

	public void SetCharge(bool v)
	{
		ResetDualTrigger("CHARGE_ANTICIPATION");
		SetDualBool("CHARGE_LOOP", v);
	}

	public void Parry()
	{
		SetDualTrigger("PARRY");
	}

	public void SetLunge(bool v)
	{
		SetDualBool("LUNGE_LOOP", v);
	}

	public void SetMeleeAnticipation(bool v)
	{
		SetDualBool("MELEE_ANTICIPATION", v);
	}

	public void SetHorizontalCharge(bool v)
	{
		SetDualBool("HORIZONTAL_CHARGE", v);
	}

	private void ResetDualTrigger(string name)
	{
		bodyAnimator.ResetTrigger(name);
		weaponAnimator.ResetTrigger(name);
	}

	private void SetDualTrigger(string name)
	{
		bodyAnimator.SetTrigger(name);
		weaponAnimator.SetTrigger(name);
	}

	private void SetDualBool(string name, bool value)
	{
		bodyAnimator.SetBool(name, value);
		weaponAnimator.SetBool(name, value);
	}

	public void SetWeaponVisible(bool visible)
	{
		wpnSpr.enabled = visible;
	}

	public void FlipSpriteWithAngle(float angle)
	{
		Flip(flip: false);
		float num = 1.5f;
		float num2 = 1f;
		Debug.Log("ANGLE: " + angle);
		if (angle > 90f && angle < 270f)
		{
			bodySpr.flipY = true;
			wpnSpr.flipY = true;
			Debug.Log("FLIP Y");
		}
		else
		{
			bodySpr.flipY = false;
			wpnSpr.flipY = false;
			Debug.Log("NO FLIP Y");
		}
		num2 = (bodySpr.flipY ? 1 : (-1));
		bodySpr.transform.localPosition = new Vector2(0f, num * num2);
		wpnSpr.transform.localPosition = new Vector2(0f, num * num2);
	}

	public void ActivateIntroColor()
	{
		shaderEffects.SetColorTint(amanecidaColor, 1f, enabled: true);
		shaderEffectsWeapon.SetColorTint(amanecidaColor, 1f, enabled: true);
	}

	public void DeactivateIntroColor()
	{
		shaderEffects.SetColorTint(amanecidaColor, 0f, enabled: false);
		shaderEffectsWeapon.SetColorTint(amanecidaColor, 0f, enabled: false);
	}

	public void SetSpriteRotation(float angle, float bowAngleDifference)
	{
		float z = ((!(angle > 90f) || !(angle < 270f)) ? (angle + bowAngleDifference) : (angle - bowAngleDifference));
		Quaternion localRotation = Quaternion.Euler(0f, 0f, z);
		rotationParent.transform.localRotation = localRotation;
	}

	public Vector2 GetCurrentUp()
	{
		return bodySpr.transform.up * ((!bodySpr.flipY) ? 1 : (-1));
	}

	public void ClearRotationAndFlip()
	{
		bodySpr.flipY = false;
		wpnSpr.flipY = false;
		float y = -1.5f;
		bodySpr.transform.localPosition = new Vector2(0f, y);
		wpnSpr.transform.localPosition = new Vector2(0f, y);
		SetSpriteRotation(0f, 0f);
	}

	public void PlayDeath()
	{
		SetDualTrigger("DEATH");
	}

	public void ShowSprites(bool show)
	{
		wpnSpr.enabled = show;
		bodySpr.enabled = show;
	}
}
