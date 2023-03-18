using System;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Amanecidas.Audio;

public class AmanecidasAudio : EntityAudio
{
	private Dictionary<string, EventInstance> eventRefsByEventId = new Dictionary<string, EventInstance>();

	private const string Amanecidas_SHIELD = "AmanecidasShield";

	private const string Amanecidas_SHIELD_DESTROY = "AmanecidasShieldDestroy";

	private const string Amanecidas_SHIELD_RECHARGE = "AmanecidasShieldRecharge";

	private const string Amanecidas_SHIELD_EXPLOSION = "AmanecidasShieldExplosion";

	private const string Amanecidas_HEAD_EXPLOSION = "AmanecidasHeadExplosion";

	private const string Amanecidas_ATTACK_CHARGE = "AmanecidasAttackCharge";

	private const string Amanecidas_ENERGY_CHARGE = "AmanecidasEnergyCharge";

	private const string Amanecidas_DEATH = "AmanecidasDeath";

	private const string Amanecidas_MOVE_FAST = "AmanecidasMoveFast";

	private const string Amanecidas_TELEPORT_IN = "AmanecidasTeleportIn";

	private const string Amanecidas_TELEPORT_OUT = "AmanecidasTeleportOut";

	private const string Amanecidas_TURN = "AmanecidasTurn";

	private const string Amanecidas_PAIN = "AmanecidasPain";

	private const string Amanecidas_RECOVER = "AmanecidasRecover";

	private const string Amanecidas_RECHARGE = "AmanecidasRecharge";

	private const string Amanecidas_WEAPON_SPAWN = "AmanecidasWeaponSpawn";

	private const string Amanecidas_TORNADO = "AmanecidasTornado";

	private const string Amanecidas_GROUND_PISTON = "GroundPiston";

	private const string Amanecidas_GROUND_ATTACK = "AmanecidasGroundAttack";

	private const string Amanecidas_FIRE_SHOT = "AmanecidasFireShot";

	private const string Amanecidas_ARROW_CHARGE = "AmanecidasNeedleCharge";

	private const string Amanecidas_ARROW_FIRE = "AmanecidasNeedleFire";

	private const string Amanecidas_ARROW_FIRE_FAST = "AmanecidasNeedleFireFast";

	private const string Amanecidas_ARROW_HOR_PREATTACK = "AmanecidasHorizontalPreattack";

	private const string Amanecidas_ARROW_VER_PREATTACK = "AmanecidasVerticalPreattack";

	private const string Amanecidas_ARROW_LASER_PREATTACK = "AmanecidasLaserShotPreattack";

	private const string Amanecidas_ARROWS_HITS_GROUND = "AmanecidasNeedleHitGround";

	private const string Amanecidas_RAY_FIRE = "AmanecidasRayFire";

	private const string Amanecidas_SMALL_RAY_FIRE = "AmanecidasSmallRayFire";

	private const string Amanecidas_AXE_BACK = "AmanecidasAxeBack";

	private const string Amanecidas_AXE_THROW_PREATTACK = "AmanecidasAxeThrowPreattack";

	private const string Amanecidas_AXE_THROW = "AmanecidasAxeThrow";

	private const string Amanecidas_AXE_ATTACK = "AmanecidasAxeAttack";

	private const string Amanecidas_AXE_SMASH_PREATTACK = "AmanecidasAxeSmashPreattack";

	private const string Amanecidas_AXE_SMASH = "AmanecidasAxeSmash";

	private const string Amanecidas_AXE_HITS_GROUND = "AmanecidasAxeHitGround";

	private const string Amanecidas_AXE_SPIN = "AmanecidasAxeSpin";

	private const string Amanecidas_SWORD_DASH_PREATTACK = "AmanecidasSwordDashPreattack";

	private const string Amanecidas_SWORD_DASH = "AmanecidasSwordDash";

	private const string Amanecidas_SWORD_PREATTACK = "AmanecidasSwordPreattack";

	private const string Amanecidas_SWORD_ATTACK = "AmanecidasSwordAttack";

	private const string Amanecidas_SWORD_FIRE_PREATTACK = "AmanecidasSwordFirePreattack";

	private const string Amanecidas_SWORD_SPIN_PROJECTILE = "AmanecidasSpinProyectile";

	private const string Amanecidas_LANCE_DASH_PREATTACK = "AmanecidasLanceDashPreattack";

	private const string Amanecidas_LANCE_SPIKE_APPEAR = "SpikeAppear";

	private const string Amanecidas_LANCE_HITS_GROUND = "SpikeHitGround";

	private const string Amanecidas_LANCE_SPIKE_DESTROY = "SpikeDestroy";

	private const string Amanecidas_RAY_DASH_PREATTACK = "RayDashPreattack";

	private const string Amanecidas_DASH_CHARGE = "AmanecidasDashCharge";

	private const string Amanecidas_LAUDES_CHANGE_WEAPON = "LaudesChangeWeapon";

	private const string shieldDamageKey = "ShieldEnergy";

	public void PlayShield_AUDIO(float percentage)
	{
		Play_AUDIO("AmanecidasShield");
		SetShieldParam(eventRefsByEventId["AmanecidasShield"], percentage);
	}

	private void SetShieldParam(EventInstance eventInstance, float value)
	{
		try
		{
			eventInstance.getParameter("ShieldEnergy", out var instance);
			instance.setValue(value);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + ex.StackTrace);
		}
	}

	public void PlayShieldDestroy_AUDIO()
	{
		PlayOneShotEvent("AmanecidasShieldDestroy", FxSoundCategory.Attack);
	}

	public void PlayShieldRecharge_AUDIO()
	{
		PlayOneShotEvent("AmanecidasShieldRecharge", FxSoundCategory.Attack);
	}

	public void PlayShieldExplosion_AUDIO()
	{
		PlayOneShotEvent("AmanecidasShieldExplosion", FxSoundCategory.Attack);
	}

	public void PlayHeadExplosion_AUDIO()
	{
		PlayOneShotEvent("AmanecidasHeadExplosion", FxSoundCategory.Attack);
	}

	public void PlayAttackCharge_AUDIO()
	{
		PlayOneShotEvent("AmanecidasAttackCharge", FxSoundCategory.Motion);
	}

	public void PlayEnergyCharge_AUDIO()
	{
		PlayOneShotEvent("AmanecidasEnergyCharge", FxSoundCategory.Motion);
	}

	public void PlayDeath_AUDIO()
	{
		PlayOneShotEvent("AmanecidasDeath", FxSoundCategory.Motion);
	}

	public void PlayMoveFast_AUDIO()
	{
		PlayOneShotEvent("AmanecidasMoveFast", FxSoundCategory.Motion);
	}

	public void PlayPain_AUDIO()
	{
		PlayOneShotEvent("AmanecidasPain", FxSoundCategory.Motion);
	}

	public void PlayRecover_AUDIO()
	{
		PlayOneShotEvent("AmanecidasRecover", FxSoundCategory.Motion);
	}

	public void StopShieldRecharge_AUDIO()
	{
		Stop_AUDIO("AmanecidasShieldRecharge");
	}

	public void PlayGroundPiston_AUDIO()
	{
		PlayOneShotEvent("GroundPiston", FxSoundCategory.Motion);
	}

	public void PlayGroundAttack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasGroundAttack", FxSoundCategory.Attack);
	}

	public void PlayArrowCharge_AUDIO()
	{
		PlayOneShotEvent("AmanecidasNeedleCharge", FxSoundCategory.Attack);
	}

	public void PlayArrowFire_AUDIO()
	{
		PlayOneShotEvent("AmanecidasNeedleFire", FxSoundCategory.Attack);
	}

	public void PlayArrowFireFast_AUDIO()
	{
		PlayOneShotEvent("AmanecidasNeedleFireFast", FxSoundCategory.Attack);
	}

	public void PlayHorizontalPreattack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasHorizontalPreattack", FxSoundCategory.Attack);
	}

	public void PlayVerticalPreattack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasVerticalPreattack", FxSoundCategory.Attack);
	}

	public void PlayLaserPreattack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasLaserShotPreattack", FxSoundCategory.Attack);
	}

	public void PlayArrowHitsGround_AUDIO()
	{
		PlayOneShotEvent("AmanecidasNeedleHitGround", FxSoundCategory.Attack);
	}

	public void PlayRayFire_AUDIO()
	{
		PlayOneShotEvent("AmanecidasRayFire", FxSoundCategory.Attack);
	}

	public void PlayAxeHitsGround_AUDIO()
	{
		PlayOneShotEvent("AmanecidasAxeHitGround", FxSoundCategory.Attack);
	}

	public void PlayAxeThrowPreattack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasAxeThrowPreattack", FxSoundCategory.Motion);
	}

	public void PlayAxeThrow_AUDIO()
	{
		PlayOneShotEvent("AmanecidasAxeThrow", FxSoundCategory.Attack);
	}

	public void PlayAxeAttack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasAxeAttack", FxSoundCategory.Attack);
	}

	public void PlayAxeSmashPreattack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasAxeSmashPreattack", FxSoundCategory.Motion);
	}

	public void PlayAxeSmash_AUDIO()
	{
		PlayOneShotEvent("AmanecidasAxeSmash", FxSoundCategory.Attack);
	}

	public void PlaySwordDashPreattack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasSwordDashPreattack", FxSoundCategory.Attack);
	}

	public void PlaySwordDash_AUDIO()
	{
		PlayOneShotEvent("AmanecidasSwordDash", FxSoundCategory.Attack);
	}

	public void PlaySwordPreattack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasSwordPreattack", FxSoundCategory.Attack);
	}

	public void PlaySwordAttack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasSwordAttack", FxSoundCategory.Attack);
	}

	public void PlaySwordFirePreattack_AUDIO()
	{
		PlayOneShotEvent("AmanecidasSwordFirePreattack", FxSoundCategory.Attack);
	}

	public void PlayBeamDashPreattack_AUDIO()
	{
		PlayOneShotEvent("RayDashPreattack", FxSoundCategory.Attack);
	}

	public void PlayDashCharge_AUDIO()
	{
		Play_AUDIO("AmanecidasDashCharge");
	}

	public void StopDashCharge_AUDIO()
	{
		Stop_AUDIO("AmanecidasDashCharge");
	}

	public void PlayLaudesChangeWeapon_AUDIO()
	{
		PlayOneShotEvent("LaudesChangeWeapon", FxSoundCategory.Motion);
	}

	public void PlayOneShot_AUDIO(string eventId, FxSoundCategory category)
	{
		PlayOneShotEvent(eventId, category);
	}

	public void Play_AUDIO(string eventId)
	{
		if (eventRefsByEventId.TryGetValue(eventId, out var value))
		{
			StopEvent(ref value);
			eventRefsByEventId.Remove(eventId);
		}
		value = default(EventInstance);
		PlayEvent(ref value, eventId, checkSpriteRendererVisible: false);
		eventRefsByEventId[eventId] = value;
	}

	public void Stop_AUDIO(string eventId)
	{
		if (eventRefsByEventId.TryGetValue(eventId, out var value))
		{
			StopEvent(ref value);
			eventRefsByEventId.Remove(eventId);
		}
	}

	public void StopAll()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(StopAll));
		foreach (string key in eventRefsByEventId.Keys)
		{
			EventInstance eventInstance = eventRefsByEventId[key];
			StopEvent(ref eventInstance);
		}
		eventRefsByEventId.Clear();
	}
}
