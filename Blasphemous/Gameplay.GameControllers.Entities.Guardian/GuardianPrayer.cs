using Gameplay.GameControllers.Entities.Guardian.AI;
using Gameplay.GameControllers.Entities.Guardian.Animation;
using Gameplay.GameControllers.Entities.Guardian.Audio;

namespace Gameplay.GameControllers.Entities.Guardian;

public class GuardianPrayer : Entity
{
	private bool turning;

	public GuardianPrayerBehaviour Behaviour { get; private set; }

	public GuardianPrayerAnimationHandler AnimationHandler { get; private set; }

	public GuardianPrayerAudio Audio { get; private set; }

	public GuardianPrayerAttack Attack { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Audio = GetComponentInChildren<GuardianPrayerAudio>();
		Attack = GetComponentInChildren<GuardianPrayerAttack>();
		Behaviour = GetComponentInChildren<GuardianPrayerBehaviour>();
		AnimationHandler = GetComponentInChildren<GuardianPrayerAnimationHandler>();
		AnimationHandler.GuardianPrayer = this;
	}
}
