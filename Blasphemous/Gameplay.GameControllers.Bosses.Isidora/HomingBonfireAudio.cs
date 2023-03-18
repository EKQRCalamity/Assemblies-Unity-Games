using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class HomingBonfireAudio : EntityAudio
{
	[SerializeField]
	[EventRef]
	private string Brazier_Ignition2;

	[SerializeField]
	[EventRef]
	private string Brazier_Broken;

	public void PlayBrazierBroken()
	{
		PlaySimpleOneShot(Brazier_Broken);
	}

	public void PlayBrazierIgnitionPhase2()
	{
		PlaySimpleOneShot(Brazier_Ignition2);
	}

	public void PlaySimpleOneShot(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			Core.Audio.PlayOneShot(key);
		}
	}
}
