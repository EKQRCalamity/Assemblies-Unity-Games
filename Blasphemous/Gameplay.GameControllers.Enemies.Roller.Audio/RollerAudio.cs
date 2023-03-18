using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Roller.Audio;

public class RollerAudio : EntityAudio
{
	private const string AttackEventKey = "RangedRollerAttack";

	private const string BreathEventKey = "RangedRollerBreath";

	private const string ScratchEventKey = "RangedRollerSand";

	private const string DeathEventKey = "RangedRollerDeath";

	private const string ReloadEventKey = "RangedRollerReload";

	private const string RollEventKey = "RangedRollerRoll";

	private const string PreAttackEventKey = "RangedRollerPreAttack";

	private EventInstance _idleEventInstance;

	private EventInstance _rollEventInstance;

	private bool _disposeFlag;

	public void PlayDeath()
	{
		PlayOneShotEvent("RangedRollerDeath", FxSoundCategory.Damage);
	}

	public void PlayAttack()
	{
		PlayOneShotEvent("RangedRollerAttack", FxSoundCategory.Attack);
	}

	public void PlayBreath()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("RangedRollerBreath", FxSoundCategory.Motion);
		}
	}

	public void PlayScratch()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("RangedRollerSand", FxSoundCategory.Motion);
		}
	}

	public void PlayPreAttack()
	{
		PlayOneShotEvent("RangedRollerPreAttack", FxSoundCategory.Attack);
	}

	public void PlayReloadAttack()
	{
		if (Owner.SpriteRenderer.isVisible)
		{
			PlayOneShotEvent("RangedRollerReload", FxSoundCategory.Attack);
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
		PlayEvent(ref _rollEventInstance, "RangedRollerRoll");
	}

	public void StopRolling()
	{
		_disposeFlag = true;
		if (_rollEventInstance.isValid())
		{
			_rollEventInstance.getParameter("End", out var instance);
			if (instance.isValid())
			{
				instance.setValue(1f);
			}
		}
	}
}
