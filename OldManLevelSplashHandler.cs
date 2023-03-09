using System.Collections.Generic;
using UnityEngine;

public class OldManLevelSplashHandler : AbstractPausableComponent
{
	[SerializeField]
	private Effect splashIn;

	[SerializeField]
	private Effect splashOut;

	private Vector3[] lastKnownPlayerPos = new Vector3[2]
	{
		Vector3.zero,
		Vector3.zero
	};

	public void SplashOut(float posX)
	{
		splashOut.Create(new Vector3(posX, base.transform.position.y));
	}

	public void SplashIn(float posX)
	{
		splashIn.Create(new Vector3(posX, base.transform.position.y));
	}

	private void Update()
	{
		Dictionary<int, AbstractPlayerController>.ValueCollection allPlayers = PlayerManager.GetAllPlayers();
		for (int i = 0; i < 2; i++)
		{
			AbstractPlayerController player = PlayerManager.GetPlayer((PlayerId)i);
			if (player == null || player.IsDead)
			{
				ref Vector3 reference = ref lastKnownPlayerPos[i];
				reference = Vector3.zero;
				continue;
			}
			if (lastKnownPlayerPos[i].y < base.transform.position.y)
			{
				if (!(player.transform.position.y > base.transform.position.y))
				{
				}
			}
			else if (player.transform.position.y <= base.transform.position.y)
			{
				SplashIn(player.transform.position.x);
				SFX_PlayerSplashIn();
			}
			ref Vector3 reference2 = ref lastKnownPlayerPos[i];
			reference2 = player.transform.position;
		}
	}

	private void SFX_PlayerSplashIn()
	{
		AudioManager.Play("sfx_dlc_omm_p3_stomachacid_splash");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_stomachacid_splash");
	}
}
