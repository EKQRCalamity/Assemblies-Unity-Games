using System.Collections;

public class CreditsCutscene : Cutscene
{
	protected override void Start()
	{
		base.Start();
		CutsceneGUI.Current.pause.pauseAllowed = false;
		StartCoroutine(music_cr());
	}

	private IEnumerator music_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		if (CreditsScreen.goodEnding)
		{
			AudioManager.PlayBGM();
			OnlineManager.Instance.Interface.UnlockAchievement(PlayerId.Any, "GoodEnding");
		}
		else
		{
			AudioManager.PlayBGMPlaylistManually(goThroughPlaylistAfter: true);
			OnlineManager.Instance.Interface.UnlockAchievement(PlayerId.Any, "BadEnding");
		}
	}

	protected override void SetRichPresence()
	{
		OnlineManager.Instance.Interface.SetRichPresence(PlayerId.Any, "Ending", active: true);
	}
}
