using UnityEngine;

public class MapDLC : Map
{
	private static bool haveVisited;

	[SerializeField]
	private AudioSource bakerySoundLoop;

	protected override void SelectMusic()
	{
		currentMusic = -2;
		CheckMusic(isRecheck: false);
		CheckIfBossesCompleted();
	}

	protected override void CheckMusic(bool isRecheck)
	{
		int num = currentMusic;
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne);
		PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout2 = PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo);
		if ((playerLoadout.charm == Charm.charm_curse && CharmCurse.CalculateLevel(PlayerId.PlayerOne) > -1) || (PlayerManager.Multiplayer && playerLoadout2.charm == Charm.charm_curse && CharmCurse.CalculateLevel(PlayerId.PlayerTwo) > -1))
		{
			num = (((playerLoadout.charm != Charm.charm_curse || !CharmCurse.IsMaxLevel(PlayerId.PlayerOne)) && (!PlayerManager.Multiplayer || playerLoadout2.charm != Charm.charm_curse || !CharmCurse.IsMaxLevel(PlayerId.PlayerTwo))) ? ((!PlayerData.Data.pianoAudioEnabled) ? 2 : 4) : ((!PlayerData.Data.pianoAudioEnabled) ? 3 : 5));
		}
		else if (PlayerData.Data.pianoAudioEnabled)
		{
			num = 1;
		}
		else
		{
			num = ((!haveVisited) ? (-1) : 0);
			haveVisited = true;
		}
		if ((currentMusic != -1 || num != 0) && (currentMusic != 0 || num != -1) && num != currentMusic)
		{
			currentMusic = num;
			if (currentMusic == -1)
			{
				AudioManager.PlayBGM();
			}
			else
			{
				AudioManager.StartBGMAlternate(currentMusic);
			}
		}
	}

	protected override void OnPlayerJoined(PlayerId playerId)
	{
		base.OnPlayerJoined(playerId);
		CheckMusic(isRecheck: true);
	}

	protected override void OnPlayerLeave(PlayerId playerId)
	{
		base.OnPlayerLeave(playerId);
		CheckMusic(isRecheck: true);
	}

	private void CheckIfBossesCompleted()
	{
		if (PlayerData.Data.CheckLevelsHaveMinDifficulty(Level.worldDLCBossLevels, Level.Mode.Normal))
		{
			bakerySoundLoop.gameObject.SetActive(value: false);
		}
		else
		{
			bakerySoundLoop.Play();
		}
	}
}
