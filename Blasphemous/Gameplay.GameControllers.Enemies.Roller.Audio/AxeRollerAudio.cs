using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Roller.Audio;

public class AxeRollerAudio : EntityAudio
{
	private const string AttackEventKey = "AxeRollerAttack";

	private const string BreathEventKey = "AxeRollerBreath";

	private const string ScratchEventKey = "AxeRollerSand";

	private const string DeathEventKey = "AxeRollerDeath";

	private const string ReloadEventKey = "AxeRollerReload";

	private const string RollEventKey = "AxeRollerRoll";

	private const string PreAttackEventKey = "AxeRollerPreAttack";

	private const string PreRollEventKey = "AxeRollerPreRoll";

	private EventInstance _idleEventInstance;

	private EventInstance _rollEventInstance;

	private bool _disposeFlag;

	public void PlayDeath()
	{
		PlayOneShotEvent("AxeRollerDeath", FxSoundCategory.Damage);
	}

	public void PlayAttack()
	{
		PlayOneShotEvent("AxeRollerAttack", FxSoundCategory.Attack);
	}

	public void PlayBreath()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("AxeRollerBreath", FxSoundCategory.Motion);
		}
	}

	public void PlayScratch()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("AxeRollerSand", FxSoundCategory.Motion);
		}
	}

	public void PlayPreAttack()
	{
		PlayOneShotEvent("AxeRollerPreAttack", FxSoundCategory.Attack);
	}

	public void PlayReloadAttack()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("AxeRollerReload", FxSoundCategory.Attack);
		}
	}

	public void PlayRolling()
	{
		if (_disposeFlag)
		{
			_rollEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_rollEventInstance.release();
			_rollEventInstance = default(EventInstance);
			_disposeFlag = false;
		}
		PlayEvent(ref _rollEventInstance, "AxeRollerRoll");
	}

	public void StopRolling()
	{
		_disposeFlag = true;
		_rollEventInstance.getParameter("End", out var instance);
		if (instance.isValid())
		{
			instance.setValue(1f);
		}
	}

	public void PlayAnticipateRoll()
	{
		PlayOneShotEvent("AxeRollerPreRoll", FxSoundCategory.Attack);
	}
}
