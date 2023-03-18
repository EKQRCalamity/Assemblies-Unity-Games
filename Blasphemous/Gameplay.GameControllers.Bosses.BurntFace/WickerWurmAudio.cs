using FMOD.Studio;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class WickerWurmAudio : EntityAudio
{
	private const string DeathEventKey = "BlindBabyDeath";

	private const string CryEventKey = "BlindBabyCry";

	private const string WurmFastMoveKey = "BlindBabyWurmMove";

	private const string WurmLongMoveKey = "BlindBabyWurmLongMove";

	private const string WurmAttackKey = "BlindBabyWurmAttack";

	private const string WurmPreAttackKey = "BlindBabyWurmPreAttack";

	private const string WurmAliveKey = "BlindBabyWurmAlive";

	private const string FireKey = "FireLoop";

	private const string WurmHitKey = "BlindBabyWurmHit";

	private const string EndParamKey = "End";

	private const string WurmBiteKey = "BlindBabyWurmBite";

	private const string GrabKey = "BlindBabyGrab";

	private const string Scorpion1Key = "Scorpion1";

	private const string Scorpion2Key = "Scorpion2";

	private const string ScorpionHitKey = "ScorpionHit";

	private const string AppearKey = "BabyAppear";

	private EventInstance _aliveEventInstance;

	private EventInstance _fireEventInstance;

	public void PlayDeath_AUDIO()
	{
		StopAlive_AUDIO();
		PlayOneShotEvent("BlindBabyDeath", FxSoundCategory.Damage);
	}

	public void PlayFire_AUDIO()
	{
		PlayEvent(ref _fireEventInstance, "FireLoop");
	}

	public void PlayBabyAppear_AUDIO()
	{
		PlayOneShotEvent("BabyAppear", FxSoundCategory.Motion);
	}

	public void PlayBite_AUDIO()
	{
		PlayOneShotEvent("BlindBabyWurmBite", FxSoundCategory.Damage);
	}

	public void PlayScorpion1_AUDIO()
	{
		PlayOneShotEvent("Scorpion1", FxSoundCategory.Damage);
	}

	public void PlayScorpion2_AUDIO()
	{
		PlayOneShotEvent("Scorpion2", FxSoundCategory.Damage);
	}

	public void PlayScorpionHit_AUDIO()
	{
		PlayOneShotEvent("ScorpionHit", FxSoundCategory.Damage);
	}

	public void PlayBabyGrab_AUDIO()
	{
		PlayOneShotEvent("BlindBabyGrab", FxSoundCategory.Damage);
	}

	public void PlayCry_AUDIO()
	{
		PlayOneShotEvent("BlindBabyCry", FxSoundCategory.Motion);
	}

	public void PlayHit_AUDIO()
	{
		PlayOneShotEvent("BlindBabyWurmHit", FxSoundCategory.Damage);
	}

	public void PlaySnakeLongMove_AUDIO()
	{
		PlayOneShotEvent("BlindBabyWurmLongMove", FxSoundCategory.Motion);
	}

	public void PlaySnakeMove_AUDIO()
	{
		PlayOneShotEvent("BlindBabyWurmMove", FxSoundCategory.Motion);
	}

	public void PlayPreAttack_AUDIO()
	{
		PlayOneShotEvent("BlindBabyWurmPreAttack", FxSoundCategory.Attack);
	}

	public void PlayAttack_AUDIO()
	{
		PlayOneShotEvent("BlindBabyWurmAttack", FxSoundCategory.Attack);
	}

	public void StopAlive_AUDIO()
	{
		StopEvent(ref _aliveEventInstance);
	}

	public void PlayAlive_AUDIO()
	{
		StopEvent(ref _aliveEventInstance);
		PlayEvent(ref _aliveEventInstance, "BlindBabyWurmAlive");
	}

	public void StopAll()
	{
		StopEvent(ref _aliveEventInstance);
		StopEvent(ref _fireEventInstance);
	}

	private void OnDestroy()
	{
		StopAll();
	}
}
