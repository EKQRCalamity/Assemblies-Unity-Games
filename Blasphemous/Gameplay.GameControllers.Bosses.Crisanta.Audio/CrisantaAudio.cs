using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Bosses.Crisanta.Audio;

public class CrisantaAudio : EntityAudio
{
	private const string Crisanta_DEATH = "CrisantaDeath";

	private const string Crisanta_NORMAL_ATTACK = "CrisantaNormalAttack";

	private const string Crisanta_GROUND_HIT = "CrisantaGroundHit";

	private const string Crisanta_TELEPORT_IN = "CrisantaTeleportIn";

	private const string Crisanta_TELEPORT_OUT = "CrisantaTeleportOut";

	private const string Crisanta_ROLL = "CrisantaRoll";

	private const string Crisanta_LANDING = "CrisantaLanding";

	private const string Crisanta_GUARD = "CrisantaGuard";

	private const string Crisanta_AIR_ATTACK = "CrisantaAirAttack";

	private const string Crisanta_SWORD_ATTACK = "CrisantaSwordAttack";

	private const string Crisanta_GUARD_ATTACK = "CrisantaGuardAttack";

	private const string Crisanta_TELEPORT_ATTACK = "CrisantaTeleportAttack";

	private const string Crisanta_PARRY = "CrisantaParry";

	private const string Crisanta_KNEE = "CrisantaKneeDown";

	private EventInstance _guardEventInstance;

	public void PlayDeath_AUDIO()
	{
	}

	public void PlayTeleportIn_AUDIO()
	{
		PlayOneShotEvent("CrisantaTeleportIn", FxSoundCategory.Motion);
	}

	public void PlayTeleportOut_AUDIO()
	{
		PlayOneShotEvent("CrisantaTeleportOut", FxSoundCategory.Motion);
	}

	public void PlayParry_AUDIO()
	{
		PlayOneShotEvent("CrisantaParry", FxSoundCategory.Motion);
	}

	public void PlayKnee_AUDIO()
	{
		PlayOneShotEvent("CrisantaKneeDown", FxSoundCategory.Motion);
	}

	public void PlayAirAttack_AUDIO()
	{
		PlayOneShotEvent("CrisantaAirAttack", FxSoundCategory.Attack);
	}

	public void PlayRoll_AUDIO()
	{
		PlayOneShotEvent("CrisantaRoll", FxSoundCategory.Motion);
	}

	public void PlayLanding_AUDIO()
	{
		PlayOneShotEvent("CrisantaLanding", FxSoundCategory.Motion);
	}

	public void PlayGuard_AUDIO()
	{
		PlayEvent(ref _guardEventInstance, "CrisantaGuard");
	}

	public void StopGuard_AUDIO()
	{
		StopEvent(ref _guardEventInstance);
	}

	public void PlaySwordAttack_AUDIO()
	{
		PlayOneShotEvent("CrisantaSwordAttack", FxSoundCategory.Attack);
	}

	public void PlayGuardAttack_AUDIO()
	{
		PlayOneShotEvent("CrisantaGuardAttack", FxSoundCategory.Attack);
	}

	public void PlayGroundHit_AUDIO()
	{
	}

	public void PlayLightAttack_AUDIO()
	{
		PlayGuardAttack_AUDIO();
	}
}
