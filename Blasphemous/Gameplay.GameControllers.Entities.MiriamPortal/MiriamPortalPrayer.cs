using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Entities.MiriamPortal.AI;
using Gameplay.GameControllers.Entities.MiriamPortal.Animation;
using Gameplay.GameControllers.Entities.MiriamPortal.Audio;

namespace Gameplay.GameControllers.Entities.MiriamPortal;

public class MiriamPortalPrayer : Entity
{
	private bool turning;

	public MiriamPortalPrayerBehaviour Behaviour { get; private set; }

	public MiriamPortalPrayerAnimationHandler AnimationHandler { get; private set; }

	public MiriamPortalPrayerAudio Audio { get; private set; }

	public MiriamPortalPrayerAttack Attack { get; private set; }

	public GhostTrailGenerator GhostTrail { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Audio = GetComponentInChildren<MiriamPortalPrayerAudio>();
		Attack = GetComponentInChildren<MiriamPortalPrayerAttack>();
		Behaviour = GetComponentInChildren<MiriamPortalPrayerBehaviour>();
		AnimationHandler = GetComponentInChildren<MiriamPortalPrayerAnimationHandler>();
		AnimationHandler.MiriamPortalPrayer = this;
		GhostTrail = GetComponentInChildren<GhostTrailGenerator>();
	}
}
