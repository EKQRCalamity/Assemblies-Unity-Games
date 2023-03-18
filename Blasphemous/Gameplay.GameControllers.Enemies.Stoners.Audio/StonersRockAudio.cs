using System.Collections.Generic;
using FMOD.Studio;
using Gameplay.GameControllers.Enemies.Stoners.Rock;
using Gameplay.GameControllers.Entities.Audio;

namespace Gameplay.GameControllers.Enemies.Stoners.Audio;

public class StonersRockAudio : EntityAudio
{
	public const string StonersBrokenRockEventKey = "StonersBrokenRock";

	private EventInstance _brokenRockEventInstance;

	private StonersRock _stonersRock;

	protected override void OnWake()
	{
		base.OnWake();
		EventInstances = new List<EventInstance>();
	}

	public void BrokenRock()
	{
		if (!_brokenRockEventInstance.isValid())
		{
			_brokenRockEventInstance = base.AudioManager.CreateCatalogEvent("StonersBrokenRock");
			_brokenRockEventInstance.setCallback(EntityAudio.SetPanning(_brokenRockEventInstance, base.transform.position), EVENT_CALLBACK_TYPE.CREATED);
			_brokenRockEventInstance.start();
			_brokenRockEventInstance.release();
		}
	}
}
