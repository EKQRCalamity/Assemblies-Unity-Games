using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

public class TPOFollowerAudioEvents : MonoBehaviour
{
	private void Awake()
	{
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		penitent.Audio.Mute = true;
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	public void PlayJumpFX()
	{
		Core.Audio.PlayOneShot("event:/SFX/DEMAKE/DTPOJump");
	}

	public void PlayAttackFX()
	{
		Core.Audio.PlayOneShot("event:/SFX/DEMAKE/DTPOAttack");
	}

	public void PlayDashFX()
	{
	}

	public void PlayHurtFX()
	{
		Core.Audio.PlayOneShot("event:/SFX/DEMAKE/DTPOHurt");
	}

	public void PlayDeathFX()
	{
		Core.Audio.PlayOneShot("event:/SFX/DEMAKE/DTPODeath");
	}

	public void PlayRespawnFX()
	{
		Core.Audio.PlayOneShot("event:/SFX/DEMAKE/DTPORespawn");
	}
}
